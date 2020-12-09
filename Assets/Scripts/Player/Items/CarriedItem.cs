using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  using Networking;

  public class CarriedItem : Item
  {
    private Rigidbody physicsController = null;

    public override void Awake()
    {
      base.Awake();

      physicsController = GetComponent<Rigidbody>();
    }

    public override void OnPickUpItem()
    {
      // Set rigidbody to kinematic so physics does not affect it
      physicsController.isKinematic = true;

      foreach (Transform objectTransform in GetComponentsInChildren<Transform>())
      {
        objectTransform.gameObject.layer = 23;
      }
    }

    public override void OnDropItem()
    {
      base.OnDropItem();

      physicsController.isKinematic = false;
      physicsController.AddForce(transform.forward, ForceMode.VelocityChange);

      foreach (Transform objectTransform in GetComponentsInChildren<Transform>())
      {
        objectTransform.gameObject.layer = 0;
      }
    }
  }
}