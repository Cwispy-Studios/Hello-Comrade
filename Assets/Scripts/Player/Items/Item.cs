using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  public enum ItemType
  {
    Pocketable = 0,
    Carried = 1,
    Dragged = 2
  }

  [RequireComponent(typeof(Rigidbody))]
  public class Item : MonoBehaviourPun
  {
    [SerializeField] private ItemType itemType = ItemType.Pocketable;
    public ItemType Type { get { return itemType; } }

    private float itemMass;
    public float ItemMass { get; private set; }

    private Rigidbody physicsController = null;

    private void Awake()
    {
      physicsController = GetComponent<Rigidbody>();
      itemMass = physicsController.mass;
    }

    /// <summary>
    /// 
    /// </summary>
    [PunRPC]
    public virtual void OnEquipItem()
    {
      gameObject.SetActive(true);
    }

    /// <summary>Primary code for unequipping item (GameObject handling etc)</summary>
    /// <summary>Difference with DropItem is that this is used for when item is still in inventory but active item switches</summary>
    [PunRPC]
    public virtual void OnUnequipItem()
    {
      gameObject.SetActive(false);
    }

    /// <summary>
    /// When the item gets picked up by a player
    /// </summary>
    public virtual void OnPickUpItem( int viewID )
    {
    }

    /// <summary>
    /// When the item gets dropped by a player
    /// </summary>
    public virtual void OnDropItem()
    {
    }

    /// <summary>
    /// When the item gets used or interacted by a player
    /// </summary>
    public virtual void OnUseItem()
    {
    }

    /// <summary>Primary code for execution of item deletion code</summary>
    public virtual void DestroyItem()
    {
    }

    public void TransferPhotonOwnership()
    {
      if (!photonView.IsMine || !photonView.AmOwner)
      {
        photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
      }
    }

    private void OnCollisionEnter( Collision collision )
    {
      Debug.Log("Collision!: " + Time.time.ToString("F10"), this);

      Debug.Log(collision.contactCount + " " + collision.impulse + " " + collision.relativeVelocity);

    }
  }
}