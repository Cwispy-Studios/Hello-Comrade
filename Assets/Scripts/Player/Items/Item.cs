using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  public class Item : MonoBehaviourPun
  {
    private float itemMass;
    public float ItemMass { get; private set; }

    private Rigidbody physicsController = null;

    private void Awake()
    {
      physicsController = GetComponent<Rigidbody>();
      itemMass = physicsController.mass;
    }

    /// <summary>Primary code for equipping item (GameObject handling etc)</summary>
    public virtual void EquipItem()
    {
    }

    /// <summary>Primary code for unequipping item (GameObject handling etc)</summary>
    /// <summary>Difference with DropItem is that this is used for when item is still in inventory but active item switches</summary>
    public virtual void UnequipItem()
    {
    }

    /// <summary>Primary code for dropping item (GameObject handling etc)</summary>
    public virtual void DropItem()
    {
    }

    /// <summary>Primary code for execution of item code</summary>
    public virtual void UseItem()
    {
    }

    /// <summary>Primary code for execution of item deletion code</summary>
    public virtual void DestroyItem()
    {
    }

    private void OnCollisionEnter( Collision collision )
    {
      Debug.Log("Collision!: " + Time.time.ToString("F10"), this);

      Debug.Log(collision.contactCount + " " + collision.impulse + " " + collision.relativeVelocity);

    }
  }
}