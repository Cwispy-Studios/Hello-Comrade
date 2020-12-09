using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  public enum ItemType
  {
    Pocketed = 0,
    Carried = 1,
    Dragged = 2
  }

  [RequireComponent(typeof(Rigidbody))]
  public class Item : MonoBehaviourPunCallbacks
  {
    private ItemType itemType;
    public ItemType Type { get { return itemType; } }

    private float itemMass;
    public float ItemMass { get; private set; }

    private MeshRenderer meshRenderer;

    private Rigidbody physicsController = null;
    public Rigidbody PhysicsController { get { return physicsController; } }

    private Transform sceneParent;

    public virtual void Awake()
    {
      if (GetComponent<PocketedItem>())
      {
        itemType = ItemType.Pocketed;
      }

      else if (GetComponent<CarriedItem>())
      {
        itemType = ItemType.Carried;
      }

      else if (GetComponent<DraggedItem>())
      {
        itemType = ItemType.Dragged;
      }

      physicsController = GetComponent<Rigidbody>();
      itemMass = physicsController.mass;

      meshRenderer = GetComponent<MeshRenderer>();

      sceneParent = transform.parent;
    }

    /// <summary>
    /// 
    /// </summary>
    [PunRPC]
    public virtual void OnEquipItem()
    {
      meshRenderer.enabled = true;
    }

    /// <summary>Primary code for unequipping item (GameObject handling etc)</summary>
    /// <summary>Difference with DropItem is that this is used for when item is still in inventory but active item switches</summary>
    [PunRPC]
    public virtual void OnUnequipItem()
    {
      meshRenderer.enabled = false;
    }

    /// <summary>
    /// When the item gets picked up by a player
    /// </summary>
    public virtual void OnPickUpItem()
    {
    }

    /// <summary>
    /// When the item gets dropped by a player
    /// </summary>
    public virtual void OnDropItem()
    {
      ResetTransformParent();
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
      if (!photonView.IsMine)
      {
        photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
      }
    }

    public void ResetTransformParent()
    {
      transform.parent = sceneParent;
    }

    public void OnCollisionEnter( Collision collision )
    {
      Debug.Log("Collision!: " + Time.time.ToString("F10"), this);

      Debug.Log(collision.contactCount + " " + collision.impulse + " " + collision.relativeVelocity);
    }
  }
}