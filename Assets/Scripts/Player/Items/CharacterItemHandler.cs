using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  public class CharacterItemHandler : MonoBehaviourPun
  {
    [Tooltip("Attached to player's right wrist")]
    [SerializeField] private PocketedSlot pocketedSlot = null;
    [Tooltip("Attached to player's root around the hip")]
    [SerializeField] private CarriedSlot carriedSlot = null;

    private Joint activeJoint = null;

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
    }

    public void CreateDragJoint( Item item, Vector3 localAnchorPosition )
    {
      ConfigurableJoint configurableJoint = carriedSlot.gameObject.AddComponent<ConfigurableJoint>();
      configurableJoint.autoConfigureConnectedAnchor = false;
      configurableJoint.connectedBody = item.PhysicsController;
      configurableJoint.anchor = Vector3.zero;
      configurableJoint.connectedAnchor = localAnchorPosition;
      configurableJoint.xDrive = AddJointDrive(1000f, 10f);
      configurableJoint.yDrive = AddJointDrive(750f, 10f);
      configurableJoint.zDrive = AddJointDrive(1000f, 10f);
      configurableJoint.slerpDrive = AddJointDrive(1000f, 10f);
      configurableJoint.rotationDriveMode = RotationDriveMode.Slerp;
      configurableJoint.enableCollision = true;
      configurableJoint.breakForce = 10f;
      configurableJoint.breakTorque = 10f;
      configurableJoint.connectedMassScale = 1.25f;

      activeJoint = configurableJoint;
    }

    public void DestroyJoint()
    {
      if (activeJoint)
      {
        Destroy(activeJoint);
      }
    }
  }
}
