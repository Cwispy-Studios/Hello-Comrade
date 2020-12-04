using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  public class PocketedItem : Item
  {
    private Rigidbody physicsController = null;
    // Pocketable items should only have 1 collider!
    private Collider itemCollider = null;

    public override void Awake()
    {
      base.Awake();

      physicsController = GetComponent<Rigidbody>();
      itemCollider = GetComponent<Collider>();
    }

    [PunRPC]
    public override void OnPickUpItem( int playerPhotonActorNumber )
    {
      GameObject playerObject = (GameObject)PhotonNetwork.CurrentRoom.GetPlayer(playerPhotonActorNumber).TagObject;

      transform.parent = playerObject.GetComponentInChildren<ActiveItemSlot>().transform;
      transform.localPosition = Vector3.zero;
      transform.localRotation = Quaternion.identity;

      // Unity complains if continuous rigidbody is kinematic
      physicsController.collisionDetectionMode = CollisionDetectionMode.Discrete;
      // Set rigidbody to kinematic so physics does not affect it
      physicsController.isKinematic = true;
      // Set collider to trigger so it does not affect the player's rigidbody
      itemCollider.isTrigger = true;

      gameObject.layer = 23;
    }

    [PunRPC]
    public override void OnDropItem()
    {
      // Return to scene
      transform.parent = null;

      // Reset rigidbody kinematic to allow physics interactions
      physicsController.isKinematic = false;
      physicsController.collisionDetectionMode = CollisionDetectionMode.Continuous;
      // Reset collider trigger
      itemCollider.isTrigger = false;

      gameObject.layer = 0;
    }
  }
}