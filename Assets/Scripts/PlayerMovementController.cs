using UnityEngine;
using UnityEngine.InputSystem;

using Photon.Pun;

public class PlayerMovementController : MonoBehaviourPun
{
  [SerializeField] private Camera playerCamera = null;
  [SerializeField] private GameObject neck = null;
  [SerializeField] private float lookSpeedMultiplier = 1f;
  [SerializeField] private float moveSpeed = 1.5f;
  [SerializeField] private float runSpeedMultiplier = 2f;
  [SerializeField] private float jumpForceMultiplier = 25f;
  [SerializeField] private float gravityForce = 30f;
  [SerializeField] private float gravityForceMultiplier = 2.5f;

  private Rigidbody physicsController = null;
  private Animator animator = null;

  private Vector3 mouseInput = Vector3.zero;
  private Vector3 moveInput = Vector3.zero;
  private bool isRunning = false;
  private bool jumpThisFrame = false;
  private bool isCrouching = false;

  private float colliderExtent = 0f;

  // Max look angles for mouse look, sets the rotational constraints for the neck
  private const float MaxVerticalLookDegrees = 70f;
  private const float MaxHorizontalLookDegrees = 85f;

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

    Collider capsuleCollider = GetComponent<Collider>();

    if (capsuleCollider == null)
    {
      Debug.LogError("Error! Player's Collider component is missing!", this);
    }

    colliderExtent = capsuleCollider.bounds.extents.y;
  }

  public void OnMove(InputValue value)
  {
    // Get keyboard move values
    Vector2 keyboardMoveInput = value.Get<Vector2>();

    moveInput.x = keyboardMoveInput.x;
    moveInput.z = keyboardMoveInput.y;
  }

  public void OnLook(InputValue value)
  {
    // Get mouse movements
    Vector2 mouseDelta = value.Get<Vector2>();

    mouseInput.x = mouseDelta.x;
    mouseInput.y = mouseDelta.y;
  }

  public void OnRun(InputValue value)
  {
    isRunning = value.isPressed;
  }

  public void OnCrouch(InputValue value)
  {
    isCrouching = value.isPressed;
  }

  public void OnJump()
  {
    if (CheckIfGrounded())
    {
      jumpThisFrame = true;
    }
  }

  private void Jump()
  {
    jumpThisFrame = false;

    animator.SetTrigger("Jump");

    physicsController.AddForce(Vector3.up * jumpForceMultiplier, ForceMode.Impulse);
  }

  private bool CheckIfGrounded()
  {
    Vector3 rayOrigin = physicsController.position;
    rayOrigin.y += colliderExtent;

    float rayDistance = colliderExtent + 0.1f;
    float spherecastRadius = 0.1f;

    Ray ray = new Ray(rayOrigin, Vector3.down);
    return Physics.SphereCast(ray, spherecastRadius, rayDistance, ~(1 << 8));
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
      if (!CheckIfGrounded())
      {
        ApplyGravity();
      }
    }

    if (mouseInput != Vector3.zero)
    {
      TrackAndFollowMouseMovements();
    }

    if (moveInput != Vector3.zero)
    {
      MovePlayer();
    }

    else
    {
      animator.SetBool("Is Walking", false);
      animator.SetBool("Is Running", false);
      animator.SetBool("Is Crouching", isCrouching);
    }
  }

  private void TrackAndFollowMouseMovements()
  {
    // Get local rotation of the camera inside body
    Vector3 cameraRot = playerCamera.transform.localEulerAngles;

    // Since the x rotation (looking up and down) goes from 0-360 instead of -180-180 we have to limit it to that
    if (cameraRot.x > 180f)
    {
      cameraRot.x -= 360f;
    }

    // Since the y rotation (looking up and down) goes from 0-360 instead of -180-180 we have to limit it to that
    if (cameraRot.y > 180f)
    {
      cameraRot.y -= 360f;
    }

    // Assign the relevant movements to the camera rotation
    cameraRot.x += mouseInput.y * lookSpeedMultiplier * -1f;
    cameraRot.y += mouseInput.x * lookSpeedMultiplier;

    // Clamp looking up and down
    cameraRot.x = Mathf.Clamp(cameraRot.x, -MaxVerticalLookDegrees, MaxVerticalLookDegrees);

    playerCamera.transform.localEulerAngles = cameraRot;

    // Get the neck's local rotation based on camera movement
    Vector3 neckLocalRot = new Vector3(cameraRot.y * -1f, 0f, cameraRot.x * -1f);

    // Check if neck's horizontal x rotation is above the threshold
    if (Mathf.Abs(neckLocalRot.x) > MaxHorizontalLookDegrees)
    {
      // Get the angle difference between the local rotation and max degrees
      float angleDiff = Mathf.Abs(neckLocalRot.x) - MaxHorizontalLookDegrees;

      angleDiff = neckLocalRot.x < 0f ? angleDiff * -1f : angleDiff;

      // Then clamp the rotation angle
      neckLocalRot.x = Mathf.Clamp(neckLocalRot.x, -MaxHorizontalLookDegrees, MaxHorizontalLookDegrees);

      // Rotate the body by the angle diff
      transform.Rotate(0f, -angleDiff, 0f, Space.World);
      //Quaternion deltaRotation = Quaternion.Euler(0f, -angleDiff, 0f);
      //physicsController.MoveRotation(physicsController.rotation * deltaRotation);

      // Also clamp local camera rotation
      Vector3 localCamRot = playerCamera.transform.localEulerAngles;
      if (localCamRot.y > 180f)
      {
        localCamRot.y -= 360f;
      }
      localCamRot.y = Mathf.Clamp(localCamRot.y, -MaxHorizontalLookDegrees, MaxHorizontalLookDegrees);
      playerCamera.transform.localEulerAngles = localCamRot;
    }

    neck.transform.localEulerAngles = neckLocalRot;
  }

  private void MovePlayer()
  {
    animator.SetFloat("Speed", moveInput.z);
    animator.SetFloat("Direction", moveInput.x);
    animator.SetBool("Is Walking", true);
    animator.SetBool("Is Running", isRunning);
    animator.SetBool("Is Crouching", false);

    // Get rotation of camera on 2D axis and the right rotation for horizontal movement
    float cameraAngleRad = playerCamera.transform.eulerAngles.y * Mathf.Deg2Rad;
    float cameraRightAngleRad = (playerCamera.transform.eulerAngles.y + 90f) * Mathf.Deg2Rad;

    // Get direction vectors 
    Vector3 verticalDirectionVector = new Vector3(Mathf.Sin(cameraAngleRad), 0f, Mathf.Cos(cameraAngleRad));
    Vector3 horizontalDirectionVector = new Vector3(Mathf.Sin(cameraRightAngleRad), 0f, Mathf.Cos(cameraRightAngleRad));

    if (isRunning)
    {
      verticalDirectionVector *= runSpeedMultiplier;
      horizontalDirectionVector *= runSpeedMultiplier;
    }

    Vector3 velocity = ((verticalDirectionVector * moveInput.z) + (horizontalDirectionVector * moveInput.x))
      * moveSpeed;

    physicsController.AddForce(velocity, ForceMode.VelocityChange);

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
}
