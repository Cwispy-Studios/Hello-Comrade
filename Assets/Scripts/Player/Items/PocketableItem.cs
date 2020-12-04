using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  public class PocketableItem : Item
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
    public override void OnPickUpItem( int viewID )
    {
      transform.parent = PhotonNetwork.GetPhotonView(viewID).transform;
      transform.localPosition = Vector3.zero;
      transform.localRotation = Quaternion.identity;

      // Unity complains if continuous rigidbody is kinematic
      physicsController.collisionDetectionMode = CollisionDetectionMode.Discrete;
      // Set rigidbody to kinematic so physics does not affect it
      physicsController.isKinematic = true;
      // Set collider to trigger so it does not affect the player's rigidbody
      GetComponent<Collider>().isTrigger = true;
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
    }
  }
}