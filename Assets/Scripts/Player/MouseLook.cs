using UnityEngine;

using UnityEngine.InputSystem;

namespace CwispyStudios.HelloComrade.Player
{
  public class MouseLook : MonoBehaviour
  {
    [Tooltip("Player Objects")]
    [SerializeField] private Camera playerCamera = null;
    [SerializeField] private GameObject neck = null;
    [Tooltip("Look Properties")]
    [SerializeField, Range(0.01f, 1f)] private float lookSpeed = 0.2f;
    // Max look angles for mouse look, sets the rotational constraints for the neck
    [SerializeField, Range(45f, 90f)] private float maxVerticalLookDegrees = 70f;
    [SerializeField, Range(45f, 90f)] private float maxHorizontalLookDegrees = 90f;

    private Rigidbody physicsController = null;

    private Vector3 mouseInput = Vector3.zero;

    private void Awake()
    {
      physicsController = GetComponent<Rigidbody>();

      if (physicsController == null)
      {
        Debug.LogError("Error! Player's Rigidbody component is missing!", this);
      }
    }

    private void Update()
    {
      if (mouseInput != Vector3.zero)
      {
        TrackAndFollowMouseMovements();
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
      cameraRot.x += mouseInput.y * lookSpeed * -1f;
      cameraRot.y += mouseInput.x * lookSpeed;

      // Clamp looking up and down
      cameraRot.x = Mathf.Clamp(cameraRot.x, -maxVerticalLookDegrees, maxVerticalLookDegrees);

      playerCamera.transform.localEulerAngles = cameraRot;

      // Get the neck's local rotation based on camera movement
      Vector3 neckLocalRot = new Vector3(cameraRot.y * -1f, 0f, cameraRot.x * -1f);

      // Check if neck's horizontal x rotation is above the threshold
      if (Mathf.Abs(neckLocalRot.x) > maxHorizontalLookDegrees)
      {
        // Get the angle difference between the local rotation and max degrees
        float angleDiff = Mathf.Abs(neckLocalRot.x) - maxHorizontalLookDegrees;

        angleDiff = neckLocalRot.x < 0f ? angleDiff * -1f : angleDiff;

        // Then clamp the rotation angle
        neckLocalRot.x = Mathf.Clamp(neckLocalRot.x, -maxHorizontalLookDegrees, maxHorizontalLookDegrees);

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
        localCamRot.y = Mathf.Clamp(localCamRot.y, -maxHorizontalLookDegrees, maxHorizontalLookDegrees);
        playerCamera.transform.localEulerAngles = localCamRot;
      }

      neck.transform.localEulerAngles = neckLocalRot;
    }

    private void OnLook( InputValue value )
    {
      // Get mouse movements
      Vector2 mouseDelta = value.Get<Vector2>();

      mouseInput.x = mouseDelta.x;
      mouseInput.y = mouseDelta.y;
    }
  }
}

