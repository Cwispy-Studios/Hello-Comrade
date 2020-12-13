using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  using GamePhysics;
  using Audio;

  public enum ItemType
  {
    Pocketed = 0,
    Carried = 1,
    Dragged = 2
  }

  [RequireComponent(typeof(Rigidbody))]
  public class Item : MonoBehaviourPunCallbacks
  {
    [SerializeField] private AudioEmitter collisionSounds = null;

    private ItemType itemType;
    public ItemType Type { get { return itemType; } }

    // Cache the mass of the item
    private float itemMass;
    public float ItemMass { get { return itemMass; } }

    // Makes the item invisible
    private MeshRenderer meshRenderer;

    // Cache rigidbody and make it accessible
    private Rigidbody physicsController = null;
    public Rigidbody PhysicsController { get { return physicsController; } }

    private Transform sceneParent;

    private Dictionary<float, CollisionInformation> collisionInformations = new Dictionary<float, CollisionInformation>();

    public float AwakeTime { get; private set; }
    
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

      AwakeTime = Time.time;
    }

    public virtual void Start()
    {
      collisionSounds.Initialise(transform);
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

    private void CollisionResponse( float timeOfCollision )
    {
      CollisionInformation collisionInformation = collisionInformations[timeOfCollision];

      float normalisedForce = collisionInformation.Impulse.magnitude / itemMass;

      // Ignore collisions if the force is too small
      if (normalisedForce < 0.01f)
      {
        Debug.Log("Total impulse too small: " + normalisedForce.ToString("F7"));
        return;
      }

      //Vector3 normalisedImpulse = collisionInformation.Impulse / itemMass;

      // Play audio
      collisionSounds.SetParameter("Intensity", normalisedForce);
      collisionSounds.PlaySound();
      Debug.Log("Collision with " + collisionInformation.CollisionObject + " with normalised force of "/* + normalisedImpulse + ", "*/ + normalisedForce, this);
      collisionInformations.Remove(timeOfCollision);
    }

    public void OnCollisionEnter( Collision collision )
    {
      float timeOfCollision = Time.time;

      // Collisions should not happen at the start of the game
      if (timeOfCollision <= AwakeTime + 1f) return;

      Vector3 impulse = collision.impulse;

      // New collision
      if (!collisionInformations.ContainsKey(timeOfCollision))
      {
        CollisionInformation collisionInformation = new CollisionInformation(collision.gameObject, impulse);
        collisionInformations.Add(timeOfCollision, collisionInformation);

        StartCoroutine(WaitForCollisionResponse(timeOfCollision));
      }

      // Existing collision, add up impulse
      else
      {
        collisionInformations[timeOfCollision].AddImpulse(impulse);
      }
      //Debug.Log("Collision!: " + Time.time.ToString("F10"), this);

      //Debug.Log(collision.contactCount + " " + collision.impulse + " " + collision.relativeVelocity);
    }

    private IEnumerator WaitForCollisionResponse( float timeOfCollision )
    {
      // Wait for next frame
      yield return null;

      CollisionResponse(timeOfCollision);
    }
  }
}