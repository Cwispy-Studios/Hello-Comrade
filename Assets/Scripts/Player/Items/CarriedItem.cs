using UnityEngine;

namespace CwispyStudios.HelloComrade.Player.Items
{
  public class CarriedItem : Item
  {
    public override void Awake()
    {
      base.Awake();
    }

    public override void OnPickUpItem()
    {
      // Set rigidbody to kinematic so physics does not affect it
      PhysicsController.isKinematic = true;

      foreach (Transform objectTransform in GetComponentsInChildren<Transform>())
      {
        objectTransform.gameObject.layer = 23;
      }
    }

    public override void OnDropItem()
    {
      base.OnDropItem();

      PhysicsController.isKinematic = false;
      PhysicsController.AddForce(transform.forward, ForceMode.VelocityChange);

      foreach (Transform objectTransform in GetComponentsInChildren<Transform>())
      {
        objectTransform.gameObject.layer = 0;
      }
    }
  }
}