using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;

namespace CwispyStudios.HelloComrade.Player
{
  using Items;

  public class MouseLook : MonoBehaviour
  {
    [Tooltip("Player Objects")]
    [SerializeField] private GameObject neck = null;
    [Tooltip("Look Properties")]
    [SerializeField, Range(0.01f, 1f)] private float horizontalLookSensitivity = 0.15f;
    [SerializeField, Range(0.01f, 1f)] private float verticalLookSensitivity = 0.15f;
    // Max look angles for mouse look, sets the rotational constraints for the neck
    [SerializeField, Range(45f, 90f)] private float maxVerticalLookDegrees = 70f;
    [SerializeField, Range(45f, 90f)] private float maxHorizontalLookDegrees = 90f;

    private Camera playerCamera;
    private Rigidbody physicsController;
    private CharacterItemHandler itemHandler;

    private bool inFreeLook = false;
    private Vector3 centerScreenVector;
    private Vector3 mouseInput = Vector3.zero;

    private void Awake()
    {
      playerCamera = GetComponentInChildren<Camera>();
      physicsController = GetComponent<Rigidbody>();
      physicsController.maxAngularVelocity = 50f;
      itemHandler = GetComponent<CharacterItemHandler>();

      centerScreenVector = new Vector3(Screen.width / 2, Screen.height / 2, 0f);
    }

    private void FixedUpdate()
    {
      if (mouseInput != Vector3.zero)
      {
        if (inFreeLook) FreeLook();
        else if (!itemHandler.LockRotation) RotatePlayer();

        LookVertically();
      }
    }

    private void FreeLook()
    {
      // Get local rotation of the camera inside body
      Vector3 cameraRot = playerCamera.transform.localEulerAngles;

      // Since the y rotation (looking horizontally) goes from 0-360 instead of -180-180 we have to limit it to that
      if (cameraRot.y > 180f)
      {
        cameraRot.y -= 360f;
      }

      cameraRot.y += mouseInput.x * horizontalLookSensitivity;
      // Clamp look angle
      cameraRot.y = Mathf.Clamp(cameraRot.y, -maxHorizontalLookDegrees, maxHorizontalLookDegrees);

      playerCamera.transform.localRotation = Quaternion.Euler(cameraRot);
    }

    private void RotatePlayer()
    {
      Quaternion deltaRotation = Quaternion.Euler(0f, mouseInput.x * horizontalLookSensitivity, 0f);
      physicsController.MoveRotation(physicsController.rotation * deltaRotation);
    }

    private void LookVertically()
    {
      // Get local rotation of the camera inside body
      Vector3 cameraRot = playerCamera.transform.localEulerAngles;

      // Since the x rotation (looking vertically) goes from 0-360 instead of -180-180 we have to limit it to that
      if (cameraRot.x > 180f)
      {
        cameraRot.x -= 360f;
      }

      // Assign the relevant movements to the camera rotation
      cameraRot.x += mouseInput.y * verticalLookSensitivity * -1f;
      // Clamp looking up and down
      cameraRot.x = Mathf.Clamp(cameraRot.x, -maxVerticalLookDegrees, maxVerticalLookDegrees);

      playerCamera.transform.localEulerAngles = cameraRot;

      // Get the neck's local rotation based on camera movement
      Vector3 neckLocalRot = new Vector3(-cameraRot.y, 0f, -cameraRot.x);
      neck.transform.localEulerAngles = neckLocalRot;
    }

    private void OnLook( InputValue value )
    {
      // Get mouse movements
      Vector2 mouseDelta = value.Get<Vector2>();

      mouseInput.x = mouseDelta.x;
      mouseInput.y = mouseDelta.y;
    }

    private void OnFreeLook( InputValue value )
    {
      StopAllCoroutines();
      inFreeLook = value.isPressed;

      if (!inFreeLook)
      {
        StartCoroutine("LerpCameraToDefaultRotation");
      }
    }

    public bool GetLookingAtObject( float distance, int layerMask, out RaycastHit hit, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore )
    {
      Ray ray = playerCamera.ScreenPointToRay(centerScreenVector);

      return Physics.Raycast(ray, out hit, distance, layerMask, queryTriggerInteraction);
    }

    private IEnumerator LerpCameraToDefaultRotation()
    {
      Vector3 currentCamLocalRot = playerCamera.transform.localRotation.eulerAngles;
      if (currentCamLocalRot.y > 180f) currentCamLocalRot.y -= 360f;

      Vector3 targetCamLocalRot = currentCamLocalRot;
      targetCamLocalRot.y = 0f;

      Vector3 neckLocalRot = new Vector3(-currentCamLocalRot.y, 0f, -currentCamLocalRot.x);

      while (currentCamLocalRot.y != 0f)
      {
        targetCamLocalRot.x = currentCamLocalRot.x = playerCamera.transform.localRotation.eulerAngles.x;
        currentCamLocalRot.y = Mathf.MoveTowards(currentCamLocalRot.y, targetCamLocalRot.y, 1000f * Time.deltaTime);

        neckLocalRot.z = -currentCamLocalRot.x;
        neckLocalRot.x = -currentCamLocalRot.y;

        playerCamera.transform.localRotation = Quaternion.Euler(currentCamLocalRot);
        neck.transform.localRotation = Quaternion.Euler(neckLocalRot);

        yield return null;
      }
    }
  }
}

