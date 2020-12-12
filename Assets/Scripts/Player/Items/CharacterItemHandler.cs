using UnityEngine;
using UnityEngine.InputSystem;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  public class CharacterItemHandler : MonoBehaviourPun
  {
    [Tooltip("Attached to player's right wrist")]
    [SerializeField] private PocketedSlot pocketedSlot = null;
    [Tooltip("Attached to player's root around the hip")]
    [SerializeField] private CarriedSlot carriedSlot = null;

    private Vector2 mouseDelta;

    private Joint activeJoint = null;
    private Item activeItem = null;
    private Rigidbody physicsController = null;

    public bool LockRotation = false;

    private void Awake()
    {
      physicsController = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
      if (!photonView.IsMine) return;

      if (activeJoint)
      {
        if (mouseDelta != Vector2.zero)
        {
          // TODO: Shift to MouseLook, come up with better curve
          float maxMovement = 500f;
          float horizontalMovement = Mathf.Clamp(mouseDelta.x, -maxMovement, maxMovement);
          float massThresholdMultiplier = 2f;
          float massThreshold = activeItem.ItemMass * massThresholdMultiplier;

          if (Mathf.Abs(horizontalMovement) >= massThreshold)
          {
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
    }

    private JointDrive AddJointDrive( float force, float damping )
    {
      JointDrive drive = new JointDrive();

      drive.positionSpring = force;
      drive.positionDamper = damping;
      drive.maximumForce = Mathf.Infinity;

      return drive;
    }

    public void ConnectPocketedItemToSlot( Item item )
    {
      // Pocketed items should be in the player's transform
      item.transform.parent = pocketedSlot.transform;
    }

    public void ConnectCarriedItemToSlot( Item item )
    {
      item.transform.parent = carriedSlot.transform;
      item.transform.localPosition = Vector3.zero;
      item.transform.localRotation = Quaternion.identity;

      activeItem = item;
    }

    public void CreateDragJointWithItem( Item item, Vector3 localAnchorPosition )
    {
      ConfigurableJoint configurableJoint = gameObject.AddComponent<ConfigurableJoint>();
      configurableJoint.autoConfigureConnectedAnchor = false;
      configurableJoint.connectedBody = item.PhysicsController;
      configurableJoint.anchor = transform.InverseTransformPoint(item.transform.TransformPoint(localAnchorPosition));
      configurableJoint.connectedAnchor = localAnchorPosition;

      configurableJoint.xDrive = AddJointDrive(1000f, 10f);
      configurableJoint.yDrive = AddJointDrive(750f, 10f);
      configurableJoint.zDrive = AddJointDrive(1000f, 10f);
      configurableJoint.slerpDrive = AddJointDrive(1000f, 10f);
      configurableJoint.rotationDriveMode = RotationDriveMode.Slerp;

      configurableJoint.enableCollision = true;
      //configurableJoint.breakForce = 10f;
      //configurableJoint.breakTorque = 10f;
      configurableJoint.connectedMassScale = 1.5f;
      configurableJoint.xMotion = ConfigurableJointMotion.Limited;
      configurableJoint.yMotion = ConfigurableJointMotion.Limited;
      configurableJoint.zMotion = ConfigurableJointMotion.Limited;
      //configurableJoint.angularXMotion = ConfigurableJointMotion.Limited;
      //configurableJoint.angularYMotion = ConfigurableJointMotion.Limited;
      //configurableJoint.angularZMotion = ConfigurableJointMotion.Limited;

      activeJoint = configurableJoint;

      activeItem = item;

      LockRotation = true;
    }

    public void DestroyJoint()
    {
      if (activeJoint)
      {
        Destroy(activeJoint);
        activeJoint = null;
      }

      LockRotation = false;
    }

    public void OnLook( InputValue value )
    {
      mouseDelta = value.Get<Vector2>();
    }
  }
}
