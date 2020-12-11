using UnityEngine;
using UnityEngine.InputSystem;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  public class CharacterItemHandler : MonoBehaviourPun, IPunObservable
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
      if (activeJoint && mouseDelta != Vector2.zero)
      {
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

          Debug.Log(amountToRotate);

          Quaternion deltaRotation = Quaternion.Euler(0f, amountToRotate, 0f);
          physicsController.MoveRotation(physicsController.rotation * deltaRotation);
        }
        //Debug.Log(mouseDelta.x);
        //Vector3 connectedAnchorWorldPosition = activeJoint.connectedAnchor + activeItem.transform.position;

        //Vector3 force = (transform.right + -transform.forward).normalized * mouseDelta.x;
        //activeItem.PhysicsController.AddForceAtPosition(force, connectedAnchorWorldPosition);
        //Debug.Log(Vector3.Dot(forward, anchor));

        //Debug.Log(forward + " " + anchor + " " + Vector3.Angle(forward, anchor));



        // Local forward of the player
        //Vector3 forward = transform.forward;
        //forward.y = 0f;
        //// Local position of the dragged item
        //Vector3 anchor = (activeJoint.connectedAnchor + activeJoint.connectedBody.position) - transform.position;
        //anchor.y = 0f;

        //float angle = Vector3.SignedAngle(forward, anchor, Vector3.up);

        ////if (Vector3.Dot(forward, anchor) < 0) angle *= -1f;

        //Debug.Log(angle);
        //Debug.DrawRay(transform.position, forward, Color.blue);
        //Debug.DrawRay(transform.position, anchor, Color.yellow);

        //Quaternion deltaRotation = Quaternion.Euler(0f, angle, 0f);


        //Debug.Log(activeJoint.currentForce);
        //float multiplier = activeItem.PhysicsController.mass / physicsController.mass;
        //float speedMultiplier = multiplier >= 1f ? 0.5f / multiplier : 1f - (multiplier * 0.5f);

        //Vector3 playerGroundVelocity = physicsController.velocity;
        //playerGroundVelocity.y = 0f;
        //playerGroundVelocity *= speedMultiplier;

        //Vector3 desiredPlayerVelocity = playerGroundVelocity;
        //desiredPlayerVelocity.y = physicsController.velocity.y;
        //physicsController.velocity = desiredPlayerVelocity;

        //Vector3 worldAnchorPosition = activeJoint.connectedAnchor + activeItem.PhysicsController.position;
        //activeItem.PhysicsController.velocity = playerGroundVelocity;

        // Push back against the player

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
      //configurableJoint.anchor = transform.InverseTransformPoint(carriedSlot.transform.position);
      configurableJoint.connectedAnchor = localAnchorPosition;

      //SoftJointLimit jointLimit = new SoftJointLimit();
      //jointLimit.limit = 2f;

      //configurableJoint.linearLimit = jointLimit;

      configurableJoint.xDrive = AddJointDrive(1000f, 10f);
      configurableJoint.yDrive = AddJointDrive(750f, 10f);
      configurableJoint.zDrive = AddJointDrive(1000f, 10f);
      configurableJoint.slerpDrive = AddJointDrive(1000f, 10f);
      configurableJoint.rotationDriveMode = RotationDriveMode.Slerp;

      //configurableJoint.linearLimit = new SoftJointLimit();

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

      //activeItem.PhysicsController.centerOfMass = transform.position - activeItem.transform.position;
    }

    public void DestroyJoint()
    {
      if (activeJoint)
      {
        Destroy(activeJoint);
        activeJoint = null;
      }

      if (activeItem)
      {
        //activeItem.PhysicsController.ResetCenterOfMass();
        activeItem = null;
      }

      LockRotation = false;
    }

    public void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
      if (stream.IsWriting)
      {

      }

      else
      {

      }
    }

    public void OnLook( InputValue value )
    {
      mouseDelta = value.Get<Vector2>();
    }
  }
}
