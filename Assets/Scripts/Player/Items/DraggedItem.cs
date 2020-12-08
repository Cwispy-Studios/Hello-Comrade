using System.Collections.Generic;

using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  using Networking;

  public class DraggedItem : Item
  {
    private Dictionary<int, ConfigurableJoint> attachedJoints = new Dictionary<int, ConfigurableJoint>();

    [PunRPC]
    public override void OnPickUpItem( Vector3 grabPoint, PhotonMessageInfo info )
    {
      int actorNumber = info.Sender.ActorNumber;
      int playerObjectViewID = (int)PhotonNetwork.CurrentRoom.GetPlayer(actorNumber).CustomProperties[PlayerCustomProperties.PlayerObjectViewID];
      GameObject playerObject = PhotonNetwork.GetPhotonView(playerObjectViewID).gameObject;

      ConfigurableJoint joint = playerObject.AddComponent<ConfigurableJoint>();
      joint.autoConfigureConnectedAnchor = false;
      joint.connectedBody = PhysicsController;
      joint.connectedAnchor = grabPoint;
      joint.anchor = Vector3.up;
      joint.xDrive = AddJointDrive(1000f, 10f);
      joint.yDrive = AddJointDrive(750f, 10f);
      joint.zDrive = AddJointDrive(1000f, 10f);
      joint.slerpDrive = AddJointDrive(1000f, 10f);
      joint.rotationDriveMode = RotationDriveMode.Slerp;
      joint.enableCollision = true;
      joint.breakForce = 10f;
      joint.breakTorque = 10f;
      joint.connectedMassScale = 1.25f;

      attachedJoints.Add(actorNumber, joint);
    }

    [PunRPC]
    public override void OnDropItem()
    {
      //int actorNumber = info.Sender.ActorNumber;

      //Destroy(attachedJoints[actorNumber]);
      //attachedJoints.Remove(actorNumber);
    }

    private JointDrive AddJointDrive( float force, float damping )
    {
      JointDrive drive = new JointDrive();

      drive.positionSpring = force;
      drive.positionDamper = damping;
      drive.maximumForce = Mathf.Infinity;

      return drive;
    }
  }
}
