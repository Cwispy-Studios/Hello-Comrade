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
    private bool isDraggingItem = false;
    private Vector3 centerScreenVector;
    private Vector2 mouseInput = Vector2.zero;

    private void Awake()
    {
      playerCamera = GetComponentInChildren<Camera>();
      physicsController = GetComponent<Rigidbody>();
      physicsController.maxAngularVelocity = 50f;
      itemHandler = GetComponent<CharacterItemHandler>();

      centerScreenVector = new Vector3(Screen.width / 2, Screen.height / 2, 0f);
    }

    private void OnEnable()
    {
      itemHandler.DragItemEvent += OnDragItem;
    }

    private void OnDisable()
    {
      itemHandler.DragItemEvent -= OnDragItem;
    }

    private void FixedUpdate()
    {
      if (mouseInput != Vector2.zero)
      {
        if (inFreeLook) FreeLook();
        else if (!isDraggingItem) RotatePlayer();
        else RotateDraggedItem();

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

    private void RotateDraggedItem()
    {
      if (mouseInput != Vector2.zero)
      {
        // Max force allowed from raw delta movement
        float maxMovement = 500f;
        // Clamp the horizontal movement to the max force
        float horizontalMovement = Mathf.Clamp(mouseInput.x, -maxMovement, maxMovement);
        // The minimum movement required to move the object based on its mass
        float massThresholdMultiplier = 2f;
        float massThreshold = itemHandler.ActiveItem.ItemMass * massThresholdMultiplier;

        if (Mathf.Abs(horizontalMovement) >= massThreshold)
        {
          // How much the object should be rotated based on how fast the mouse is being moved
          float rotationStrength = Mathf.Abs(horizontalMovement) - massThreshold;
          float x = rotationStrength / maxMovement;
          float maxRotation = 5f;
          // https://easings.net/#easeOutQuad
          float amountToRotate = (1 - (1 - x) * (1 - x)) * maxRotation;
          if (horizontalMovement < 0f) amountToRotate *= -1f;

          Quaternion deltaRotation = Quaternion.Euler(0f, amountToRotate, 0f);
          physicsController.MoveRotation(physicsController.rotation * deltaRotation);
        }
      }
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

    private void OnDragItem( bool isDragging )
    {
      isDraggingItem = isDragging;
    }

    /// <summary>
    /// Input System callback when moving the mouse
    /// </summary>
    /// <param name="value"></param>
    private void OnLook( InputValue value )
    {
      // Get mouse movements
      mouseInput = value.Get<Vector2>();
    }

    /// <summary>
    /// Input System callback when pressing L ALT
    /// </summary>
    /// <param name="value"></param>
    private void OnFreeLook( InputValue value )
    {
      StopAllCoroutines();
      inFreeLook = value.isPressed;

      if (!inFreeLook)
      {
        StartCoroutine(LerpCameraToDefaultRotation());
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

