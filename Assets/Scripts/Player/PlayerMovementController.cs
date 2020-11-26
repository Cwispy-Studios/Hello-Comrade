using UnityEngine;
using UnityEngine.InputSystem;

using Photon.Pun;

using FMODUnity;

namespace CwispyStudios.HelloComrade.Player
{
  [RequireComponent(typeof(GroundDetector))]
  public class PlayerMovementController : MonoBehaviourPun
  {
    [Header("Player Objects")]
    [SerializeField] private Camera playerCamera = null;
    [SerializeField] private GameObject neck = null;
    [Header("Movement")]
    [SerializeField, Range(0.1f, 10f)] private float moveSpeed = 0.3f;
    [SerializeField, Range(1f, 3f)] private float runSpeedMultiplier = 2.25f;
    [SerializeField, Range(0.1f, 1f)] private float sneakSpeedMultiplier = 0.5f;
    [Header("Slope and Step")]
    [SerializeField, Range(0f, 0.2f)] private float maxStepDistance = 0.15f;
    [SerializeField, Range(0f, 90f)] private float maxSlopeAngle = 40f;
    [Header("Jumping and Gravity")]
    [SerializeField] private float jumpForce = 750f;
    [SerializeField] private float gravityForce = 9.81f;
    [SerializeField] private float gravityDownwardForceMultiplier = 5f;

    private const float StandingColliderHeight = 1.8f;
    private const float StandingColliderPosition = StandingColliderHeight * 0.5f;
    private const float CrouchingColliderHeight = 1.15f;
    private const float CrouchingColliderPosition = CrouchingColliderHeight * 0.5f;

    private CapsuleCollider playerCollider = null;
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

      playerCollider = GetComponent<CapsuleCollider>();

      if (playerCollider == null)
      {
        Debug.LogError("Error! Player's Collider component is missing!", this);
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

      physicsController.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void ApplyGravity()
    {
      float gravityMultiplier = physicsController.velocity.y < 0f ? gravityDownwardForceMultiplier : 1f;
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
        animator.SetFloat("Speed Multiplier", 0f);
      }
    }

    private void MovePlayer()
    {
      // Get rotation of camera on 2D axis and the right rotation for horizontal movement
      float cameraAngleRad = playerCamera.transform.eulerAngles.y * Mathf.Deg2Rad;
      float cameraRightAngleRad = (playerCamera.transform.eulerAngles.y + 90f) * Mathf.Deg2Rad;

      // Get direction vectors 
      Vector3 verticalDirectionVector = new Vector3(Mathf.Sin(cameraAngleRad), 0f, Mathf.Cos(cameraAngleRad));
      Vector3 horizontalDirectionVector = new Vector3(Mathf.Sin(cameraRightAngleRad), 0f, Mathf.Cos(cameraRightAngleRad));

      float speed = moveSpeed;
      float speedMultiplier = 1f;

      if (isRunning)
      {
        speedMultiplier = runSpeedMultiplier;
      }

      else if (isSneaking)
      {
        speedMultiplier = sneakSpeedMultiplier;
      }

      speed *= speedMultiplier;

      animator.SetFloat("Speed Multiplier", speedMultiplier);

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

    private bool SetCrouch( bool crouch )
    {
      // Sets the new crouch collider, if it is standing and was crouching
      // Check if the player is able to freely stand up
      bool success = CheckAndSetCrouchCollider(crouch);

      if (success)
      {
        isCrouching = crouch;
        animator.SetBool("Is Crouching", isCrouching);
      }

      return success;
    }

    private bool CheckAndSetCrouchCollider( bool crouch )
    {
      // Player is going to crouch and was standing up
      if (crouch && !isCrouching)
      {
        playerCollider.height = CrouchingColliderHeight;
        Vector3 adjustedColliderPosition = playerCollider.center;
        adjustedColliderPosition.y = CrouchingColliderPosition;
        playerCollider.center = adjustedColliderPosition;

        return true;
      }

      // Player is going to stand and was crouching
      else if (!crouch && isCrouching)
      {
        // Do a spherecast upwards to check if player will collide with anything when standing 
        Vector3 rayOrigin = physicsController.position;
        rayOrigin.y += CrouchingColliderPosition;

        float rayDistance = CrouchingColliderPosition + (StandingColliderHeight - CrouchingColliderHeight);
        Ray ray = new Ray(rayOrigin, Vector3.up);

        // If there is collision above do not crouch
        if (Physics.Raycast(ray, rayDistance, ~(1 << 8)))
        {
          return false;
        }
        
        else
        {
          playerCollider.height = StandingColliderHeight;
          Vector3 adjustedColliderPosition = playerCollider.center;
          adjustedColliderPosition.y = StandingColliderPosition;
          playerCollider.center = adjustedColliderPosition;
          return true;
        }
      }

      // No change in status, do nothing
      else return false;
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
      SetCrouch(!isCrouching);
    }

    public void OnJump()
    {
      // Player must be on the ground to jump
      if (groundDetector.IsGrounded)
      {
        // Check if crouching
        if (isCrouching) 
        {
          // Check if player can stand up in its current position
          if (SetCrouch(false))
          {
            jumpThisFrame = true;
          }
        }

        else
        {
          jumpThisFrame = true;
        }
      }
    }

    public void OnSneak( InputValue value )
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
