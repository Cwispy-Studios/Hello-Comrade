using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  public class CarriedItem : Item
  {
    private Rigidbody physicsController = null;

    public override void Awake()
    {
      base.Awake();

      physicsController = GetComponent<Rigidbody>();
    }

    [PunRPC]
    public override void OnPickUpItem( int playerPhotonActorNumber )
    {
      GameObject playerObject = (GameObject)PhotonNetwork.CurrentRoom.GetPlayer(playerPhotonActorNumber).TagObject;

      transform.parent = playerObject.GetComponentInChildren<CarriedSlot>().transform;
      transform.localPosition = Vector3.zero;
      transform.localRotation = Quaternion.identity;

      // Set rigidbody to kinematic so physics does not affect it
      physicsController.isKinematic = true;

      gameObject.layer = 23;
    }

    [PunRPC]
    public override void OnDropItem()
    {
      // Return to scene
      transform.parent = null;

      // Reset rigidbody kinematic to allow physics interactions
      physicsController.isKinematic = false;
      physicsController.AddForce(transform.forward, ForceMode.VelocityChange);

      gameObject.layer = 0;
    }
  }
}