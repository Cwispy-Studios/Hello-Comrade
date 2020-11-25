using UnityEngine;
using UnityEngine.InputSystem;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player
{
  [RequireComponent(typeof(GroundDetector))]
  public class PlayerMovementController : MonoBehaviourPun
  {
    [Tooltip("Player Objects")]
    [SerializeField] private Camera playerCamera = null;
    [SerializeField] private GameObject neck = null;
    [Tooltip("Movement")]
    [SerializeField, Range(0.1f, 10f)] private float moveSpeed = 1.5f;
    [SerializeField, Range(1f, 3f)] private float runSpeedMultiplier = 2f;
    [SerializeField, Range(0.1f, 1f)] private float sneakSpeedMultiplier = 0.5f;
    [Tooltip("Slope and Step")]
    [SerializeField, Range(0f, 0.2f)] private float maxStepDistance = 0.15f;
    [SerializeField, Range(0f, 90f)] private float maxSlopeAngle = 40f;
    [Tooltip("Jumping and Gravity")]
    [SerializeField] private float jumpForceMultiplier = 25f;
    [SerializeField] private float gravityForce = 30f;
    [SerializeField] private float gravityForceMultiplier = 2.5f;

    private GroundDetector groundDetector = null;
    private Rigidbody physicsController = null;
    private Animator animator = null;

    private Vector3 moveInput = Vector3.zero;
    private bool isRunning = false;
    private bool jumpThisFrame = false;
    private bool isCrouching = false;
    private bool isSneaking = false;

    private void Awake()
    {
      if (!photonView.IsMine && PhotonNetwork.IsConnected)
      {
        Destroy(playerCamera.gameObject);
        Destroy(GetComponent<PlayerInput>());
        return;
      }

      if (playerCamera == null)
      {
        Debug.LogError("Error! Player's camera object is missing or not assigned!", this);
      }

      if (neck == null)
      {
        Debug.LogError("Error! Player's neck object is missing or not assigned!", this);
      }

      groundDetector = GetComponent<GroundDetector>();

      if (groundDetector == null)
      {
        Debug.LogError("Error! Player's Ground Detector component is missing!", this);
      }

      physicsController = GetComponent<Rigidbody>();

      if (physicsController == null)
      {
        Debug.LogError("Error! Player's Rigidbody component is missing!", this);
      }

      animator = GetComponent<Animator>();

      if (animator == null)
      {
        Debug.LogError("Error! Player's Animator component is missing!", this);
      }
    }

    private void Jump()
    {
      jumpThisFrame = false;

      animator.SetTrigger("Jump");

      physicsController.AddForce(Vector3.up * jumpForceMultiplier, ForceMode.Impulse);
    }

    private void ApplyGravity()
    {
      float gravityMultiplier = physicsController.velocity.y < 0f ? gravityForceMultiplier : 1f;
      physicsController.AddForce(Vector3.down * gravityForce * gravityMultiplier, ForceMode.Acceleration);
    }

    private void FixedUpdate()
    {
      if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

      // Already means player is grounded and can jump
      if (jumpThisFrame)
      {
        Jump();
      }

      // Check if grounded
      else
      {
        // Not grounded apply gravity
        if (!groundDetector.IsGrounded)
        {
          ApplyGravity();
        }
      }

      if (moveInput != Vector3.zero)
      {
        MovePlayer();
      }

      else
      {
        animator.SetBool("Is Walking", false);
        animator.SetBool("Is Running", false);
      }
    }

    private void MovePlayer()
    {
      animator.SetFloat("Speed", moveInput.z);
      animator.SetFloat("Direction", moveInput.x);
      animator.SetBool("Is Walking", true);
      animator.SetBool("Is Running", isRunning);

      if (isSneaking)
      {
        animator.speed = sneakSpeedMultiplier;
      }

      else animator.speed = 1f;

      // Get rotation of camera on 2D axis and the right rotation for horizontal movement
      float cameraAngleRad = playerCamera.transform.eulerAngles.y * Mathf.Deg2Rad;
      float cameraRightAngleRad = (playerCamera.transform.eulerAngles.y + 90f) * Mathf.Deg2Rad;

      // Get direction vectors 
      Vector3 verticalDirectionVector = new Vector3(Mathf.Sin(cameraAngleRad), 0f, Mathf.Cos(cameraAngleRad));
      Vector3 horizontalDirectionVector = new Vector3(Mathf.Sin(cameraRightAngleRad), 0f, Mathf.Cos(cameraRightAngleRad));

      float speed = moveSpeed;

      if (isRunning)
      {
        speed *= runSpeedMultiplier;
      }

      else if (isSneaking)
      {
        speed *= sneakSpeedMultiplier;
      }

      Vector3 vectorDirection = ((verticalDirectionVector * moveInput.z) + (horizontalDirectionVector * moveInput.x));

      if (physicsController.SweepTest(vectorDirection, out RaycastHit hit, speed))
      {
        if (hit.point.y - physicsController.position.y <= maxStepDistance)
        {
          float hitAngle = Vector3.Angle(hit.normal, Vector3.up);

          if (hitAngle <= maxSlopeAngle)
          {
            vectorDirection = Vector3.ProjectOnPlane(vectorDirection, hit.normal);
          }

          // Step up
          else
          {
            Vector3 currentPosition = physicsController.position;
            currentPosition.y = hit.point.y;

            physicsController.position = currentPosition;
          }
        }
      }
      
      else
      {
        // Check if grounded, then project on the plane
        if (groundDetector.IsGrounded)
        {
          vectorDirection = Vector3.ProjectOnPlane(vectorDirection, groundDetector.GroundHit.normal);
        }
      }

      physicsController.AddForce(vectorDirection * speed, ForceMode.VelocityChange);

      // Make the player character rotate towards the direction it is moving in
      Quaternion lookRotation = Quaternion.LookRotation(verticalDirectionVector);
      physicsController.MoveRotation(lookRotation);

      // Reset camera and neck rotation as well
      Vector3 camRot = playerCamera.transform.localEulerAngles;
      camRot.y = 0f;
      playerCamera.transform.localEulerAngles = camRot;
      Vector3 neckRot = neck.transform.localEulerAngles;
      neckRot.x = 0f;
      neck.transform.localEulerAngles = neckRot;
    }

    private void SetCrouchCollider()
    {

    }

    public void OnMove( InputValue value )
    {
      // Get keyboard move values
      Vector2 keyboardMoveInput = value.Get<Vector2>();

      moveInput.x = keyboardMoveInput.x;
      moveInput.z = keyboardMoveInput.y;
    }

    public void OnRun( InputValue value )
    {
      isRunning = value.isPressed;

      // Running overrides other actions
      if (isRunning)
      {
        isSneaking = false;
      }
    }

    public void OnCrouch()
    {
      isCrouching = !isCrouching;

      animator.SetBool("Is Crouching", isCrouching);
    }

    public void OnJump()
    {
      if (groundDetector.IsGrounded)
      {
        jumpThisFrame = true;
      }
    }

    public void OnSneak( InputValue value  )
    {
      isSneaking = value.isPressed;

      // Sneaking overrides other actions
      if (isSneaking)
      {
        isRunning = false;
      }
    }
  }
}
