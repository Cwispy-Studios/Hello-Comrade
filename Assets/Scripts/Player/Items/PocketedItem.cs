using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  using Networking;

  public class PocketedItem : Item
  {
    private const float DropForce = 2f;

    // Pocketable items should only have 1 collider!
    private Collider itemCollider = null;

    public override void Awake()
    {
      base.Awake();

      itemCollider = GetComponent<Collider>();
    }

    public override void OnPickUpItem()
    {
      transform.localPosition = Vector3.zero;
      transform.localRotation = Quaternion.identity;

      // Unity complains if continuous rigidbody is kinematic
      PhysicsController.collisionDetectionMode = CollisionDetectionMode.Discrete;
      // Set rigidbody to kinematic so physics does not affect it
      PhysicsController.isKinematic = true;
      // Set collider to trigger so it does not affect the player's rigidbody
      itemCollider.isTrigger = true;

      gameObject.layer = 23;
    }

    [PunRPC]
    public override void OnDropItem()
    {
      base.OnDropItem();

      // Return to scene
      transform.parent = null;

      // Reset rigidbody kinematic to allow physics interactions
      PhysicsController.isKinematic = false;
      PhysicsController.collisionDetectionMode = CollisionDetectionMode.Continuous;
      PhysicsController.AddForce(transform.forward * DropForce, ForceMode.VelocityChange);
      // Reset collider trigger
      itemCollider.isTrigger = false;

      gameObject.layer = 0;
      gameObject.SetActive(true);
    }
  }
}