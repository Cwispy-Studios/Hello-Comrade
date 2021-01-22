using System;
using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  public class CharacterItemHandler : MonoBehaviourPunCallbacks
  {
    [Tooltip("Attached to player's right wrist")]
    [SerializeField] private PocketedSlot pocketedSlot = null;
    [Tooltip("Attached to player's root around the hip")]
    [SerializeField] private CarriedSlot carriedSlot = null;

    private Joint activeJoint = null;
    private Item activeItem = null;
    public Item ActiveItem { get { return activeItem; } }

    public Action<bool> DragItemEvent;

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

      DragItemEvent?.Invoke(true);
    }

    public void DestroyJoint()
    {
      if (activeJoint)
      {
        Destroy(activeJoint);
        activeJoint = null;
      }

      DragItemEvent?.Invoke(false);
    }

    public override void OnPlayerEnteredRoom( Photon.Realtime.Player newPlayer )
    {
      if (activeJoint)
      {
        photonView.RPC("SyncDragJoint", newPlayer, activeItem.photonView.ViewID, activeJoint.connectedAnchor);
      }
    }

    [PunRPC]
    private IEnumerator SyncDragJoint( int itemView, Vector3 anchorPosition )
    {
      // Wait for transform values to sync up
      yield return null;

      Item item = PhotonNetwork.GetPhotonView(itemView).GetComponent<Item>();

      CreateDragJointWithItem(item, anchorPosition);
    }
  }
}
