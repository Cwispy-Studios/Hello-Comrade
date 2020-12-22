using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

namespace CwispyStudios.HelloComrade.Player
{
  [RequireComponent(typeof(GroundDetector))]
  public class PlayerMovementController : MonoBehaviourPun
  {
    [Header("Player Objects")]
    [SerializeField] private GameObject crouchCameraPositionObject = null;
    [Header("Movement")]
    [SerializeField, Range(0.01f, 1f)] private float walkSpeed = 0.27f;
    [SerializeField, Range(1f, 3f)] private float runSpeedMultiplier = 1.9f;
    [SerializeField, Range(0.1f, 1f)] private float sneakSpeedMultiplier = 0.45f;
    [SerializeField, Range(0.5f, 1f)] private float crouchSpeedMultiplier = 0.8f;
    [Header("Slope and Step")]
    [SerializeField, Range(0f, 0.2f)] private float maxStepDistance = 0.15f;
    [SerializeField, Range(0f, 90f)] private float maxSlopeAngle = 50f;
    [Header("Jumping and Gravity")]
    [SerializeField, Range(100f, 2000f)] private float jumpForce = 720f;
    [SerializeField, Range(1f, 100f)] private float gravityForce = 9.81f;
    [SerializeField, Range(2f, 15f)] private float baseGravityDownwardForceMultiplier = 3f;
    [SerializeField, Range(5f, 250f)] private float increasingDownwardForceMultiplier = 100f;
    [SerializeField, Range(10f, 100f)] private float terminalVelocity = 53f;

    private const float StandingColliderHeight = 1.8f;
    private const float StandingColliderPosition = StandingColliderHeight * 0.5f;
    private const float CrouchingColliderHeight = 1.15f;
    private const float CrouchingColliderPosition = CrouchingColliderHeight * 0.5f;

    /// <summary>
    /// If you change this value in runtime you are a bad person.
    /// </summary>
    private Vector3 standingCameraLocalPosition;
    /// <summary>
    /// If you change this value in runtime you are a bad person.
    /// </summary>
    private Vector3 crouchingCameraLocalPosition;
    private float timeSpentFalling = 0f;

    private Camera playerCamera;
    private CapsuleCollider playerCollider;
    private GroundDetector groundDetector;
    private Rigidbody physicsController;
    private Animator animator;

    private float moveSpeedMultiplier = 1f;

    private Vector3 moveInput = Vector3.zero;
    private PlayerMovementState playerMovementState = PlayerMovementState.Walking;
    private bool jumpThisFrame = false;
    private bool isCrouching = false;
    private bool runningButtonHeld = false;
    private bool sneakingButtonHeld = false;

    private RaiseEventOptions jumpRaiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };

    private void Awake()
    {
      playerCamera = GetComponentInChildren<Camera>();
      playerCollider = GetComponent<CapsuleCollider>();
      groundDetector = GetComponent<GroundDetector>();
      physicsController = GetComponent<Rigidbody>();
      animator = GetComponent<Animator>();

      standingCameraLocalPosition = playerCamera.transform.localPosition;
      crouchingCameraLocalPosition = crouchCameraPositionObject.transform.localPosition;
    }

    private void FixedUpdate()
    {
      if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

      animator.SetBool("On Ground", groundDetector.IsGrounded);

      // Already means player is grounded and can jump
      if (jumpThisFrame)
      {
        Jump();
      }

      // Check if player is falling and gravity should be applied
      else if (groundDetector.IsFalling)
      {
        ApplyGravity();
      }

      else
      {
        timeSpentFalling = 0f;
      }

      if (moveInput != Vector3.zero)
      {
        animator.SetFloat("Speed Multiplier", moveSpeedMultiplier);
        MovePlayer();
      }

      else
      {
        animator.SetFloat("Speed Multiplier", 0f);
      }
    }

    private void Jump()
    {
      jumpThisFrame = false;
      physicsController.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void OnLand()
    {
      animator.SetTrigger("Land");
    }

    private void ApplyGravity()
    {
      float downwardForce = gravityForce;

      if (physicsController.velocity.y < 0f)
      {
        timeSpentFalling += Time.fixedDeltaTime;
        downwardForce *= baseGravityDownwardForceMultiplier;
        downwardForce += timeSpentFalling * increasingDownwardForceMultiplier;

        if (physicsController.velocity.y > terminalVelocity)
        {
          Vector3 limitedVelocity = physicsController.velocity;
          limitedVelocity.y = terminalVelocity;
          physicsController.velocity = limitedVelocity;
        }
      }

      physicsController.AddForce(Vector3.down * downwardForce, ForceMode.Acceleration);
    }

    private void MovePlayer()
    {    
      Vector3 vectorDirection = ((transform.forward * moveInput.z) + (transform.right * moveInput.x));

      float finalSpeed = walkSpeed * moveSpeedMultiplier;

      // Crouch speed multiplier only affects when on the ground
      if (isCrouching && groundDetector.IsGrounded)
      {
        finalSpeed *= crouchSpeedMultiplier;
      }

      // Check in the direction of movement of the rigidbody to see if it will collide with anything
      // This is mainly to ensure that the rigidbody is able to move up and down slopes properly
      // without walking into or over them
      if (physicsController.SweepTest(vectorDirection, out RaycastHit hit, finalSpeed, QueryTriggerInteraction.Ignore))
      {
        // Player is able to step over objects
        if (hit.point.y - physicsController.position.y <= maxStepDistance)
        {
          // Find the angle of the object we hit
          float hitAngle = Vector3.Angle(hit.normal, Vector3.up);

          // Player can walk up this angle, adjust the movement angle 
          if (hitAngle <= maxSlopeAngle)
          {
            vectorDirection = Vector3.ProjectOnPlane(vectorDirection, hit.normal);
          }

          // Step up the object
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

      physicsController.AddForce(vectorDirection * finalSpeed, ForceMode.VelocityChange);
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

        ChangeCameraPosition();
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

    private void ChangeCameraPosition()
    {
      StopAllCoroutines();

      if (isCrouching)
      {
        StartCoroutine(LerpCameraToPosition(crouchingCameraLocalPosition));
      }

      else
      {
        StartCoroutine(LerpCameraToPosition(standingCameraLocalPosition));
      }
    }

    private IEnumerator LerpCameraToPosition( Vector3 targetPosition )
    {
      Vector3 playerCameraPosition = playerCamera.transform.localPosition;

      while (Vector3.Distance(playerCameraPosition, targetPosition) > 0.05f)
      {
        playerCameraPosition = Vector3.MoveTowards(playerCameraPosition, targetPosition, 2f * Time.deltaTime);
        playerCamera.transform.localPosition = playerCameraPosition;

        yield return null;
      }

      playerCamera.transform.localPosition = targetPosition;
    }

    private void OnMove( InputValue value )
    {
      // Get keyboard move values
      Vector2 keyboardMoveInput = value.Get<Vector2>();

      moveInput.x = keyboardMoveInput.x;
      moveInput.z = keyboardMoveInput.y;
    }

    private void OnRun( InputValue value )
    {
      runningButtonHeld = value.isPressed;

      // Running overrides other actions
      if (runningButtonHeld)
      {
        playerMovementState = PlayerMovementState.Running;
        moveSpeedMultiplier = runSpeedMultiplier;
      }

      // Stop running
      else
      {
        // Check if sneak button is still held down, if it is then player moves to sneaking
        if (sneakingButtonHeld)
        {
          playerMovementState = PlayerMovementState.Sneaking;
          moveSpeedMultiplier = sneakSpeedMultiplier;
        }

        // Player may be already sneaking (sneaking overrode running and both keys still held down
        // but run key was let go off)
        else if (playerMovementState != PlayerMovementState.Sneaking)
        {
          playerMovementState = PlayerMovementState.Walking;
          moveSpeedMultiplier = 1f;
        }
      }
    }

    /// <summary>
    /// Input system callback when pressing L CTRL
    /// </summary>
    /// <param name="value"></param>
    private void OnSneak( InputValue value )
    {
      sneakingButtonHeld = value.isPressed;

      // Sneaking overrides other actions
      if (sneakingButtonHeld)
      {
        playerMovementState = PlayerMovementState.Sneaking;
        moveSpeedMultiplier = sneakSpeedMultiplier;
      }

      // Stop running, but player may also be running so check that they are not so it does not override that
      else 
      {
        // Check if run button is still held down, if it is then player moves to running
        if (runningButtonHeld)
        {
          playerMovementState = PlayerMovementState.Running;
          moveSpeedMultiplier = runSpeedMultiplier;
        }

        // Player may be already running (running overrode sneaking and both keys still held down
        // but sneak key was let go off)
        else if (playerMovementState != PlayerMovementState.Running)
        {
          playerMovementState = PlayerMovementState.Walking;
          moveSpeedMultiplier = 1f;
        }
      }
    }

    /// <summary>
    /// Input system callback when pressing C
    /// </summary>
    private void OnCrouch()
    {
      SetCrouch(!isCrouching);
    }

    /// <summary>
    /// Input system callback when pressing SPACEBAR
    /// </summary>
    private void OnJump()
    {
      // Player must be on the ground to jump
      if (groundDetector.IsGrounded)
      {
        // Check if crouching AND if player can stand up in its current position
        // OR player is not crouching
        if ( (isCrouching && SetCrouch(false)) || !isCrouching) 
        {
          jumpThisFrame = true;

          PhotonNetwork.RaiseEvent(PhotonEvents.OnJumpEventCode, null, jumpRaiseEventOptions, SendOptions.SendUnreliable);
        }
      }
    }
  }
}
