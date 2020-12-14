using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  [RequireComponent(typeof(CharacterItemHandler))]
  public class CharacterInventory : MonoBehaviourPunCallbacks
  {
    private const int MaxInventorySize = 4;

    // Inventory
    private Item[] inventoryList;
    private Item activeItem = null;
    private int currentIndex = 0;
    private bool lockInventory = false; // true if current item picked up is a non-pocketable item

    private CharacterItemHandler itemHandler = null;
    
    private void Awake()
    {
      itemHandler = GetComponent<CharacterItemHandler>();
      inventoryList = new Item[MaxInventorySize];
    }

    /// <summary>
    /// Finds and returns the first free slot index. Returns -1 if inventory is full.
    /// </summary>
    /// <returns></returns>
    private int ReturnFreeSlot()
    {
      for (int i = 0; i < inventoryList.Length; i++)
      {
        if (inventoryList[i] == null)
        {
          return i;
        }
      }

      return -1;
    }

    private bool AddItemToInventory( Item newItem )
    {
      // Find the index of a free item slot
      // If no free slot (-1), return false
      int freeIndex = ReturnFreeSlot();
      if (freeIndex == -1) return false;

      return AddItemToInventory(newItem, freeIndex);
    }

    private bool AddItemToInventory( Item newItem, int atIndex )
    {
      if (atIndex < 0 || atIndex >= inventoryList.Length) return false;
      if (inventoryList[atIndex] != null) return false;

      // Character gets heavier from weight of item
      AddItemMassToCharacterMass(newItem.ItemMass);

      photonView.RPC("SyncInventorySlot", RpcTarget.All, atIndex, newItem.photonView.ViewID);

      // Assign the item to the found index of the inventory list
      //inventoryList[atIndex] = newItem;

      // TODO Add call later to UI for updating inventory

      return true;
    }

    [PunRPC]
    private void SyncInventorySlot( int inventoryIndex, int viewID )
    {
      if (viewID < 0)
      {
        inventoryList[inventoryIndex] = null;
      }

      else
      {
        inventoryList[inventoryIndex] = PhotonNetwork.GetPhotonView(viewID).GetComponent<Item>();
      }
    }

    private void RemoveCurrentItem()
    {
      lockInventory = false;

      AdjustCharacterMass(-inventoryList[currentIndex].ItemMass);

      photonView.RPC("SyncInventorySlot", RpcTarget.All, currentIndex, -1);

      // TODO Add call later to UI for updating inventory
    }

    private void AddItemMassToCharacterMass( float mass )
    {
      AdjustCharacterMass(mass);
    }

    private void SubtractItemMassToCharacterMass( float mass )
    {
      AdjustCharacterMass(-mass);
    }

    private void AdjustCharacterMass( float mass )
    {
      //TotalWeight += value;
      //characterRigidbody.mass = TotalWeight;
    }

    /// <summary>
    /// Used when scrolling through inventory, can only scroll through pocketed items
    /// </summary>
    private void SwitchActiveItem()
    {
      if (activeItem != null)
      {
        activeItem.OnUnequipItem();
      }

      activeItem = inventoryList[currentIndex];

      if (activeItem != null)
      {
        activeItem.OnEquipItem();
      }
    }

    [PunRPC]
    /// <summary>Scroll through items in inventory</summary>
    private void ScrollInventorySlots( bool positiveIncrement )
    {
      // Disable scrolling if player is holding carried item
      if (lockInventory) return;

      // Switch currentIndex and use SwitchActiveGameObject to physically change object 
      currentIndex += positiveIncrement ? 1 : -1;
      currentIndex %= MaxInventorySize;

      if (currentIndex < 0)
      {
        currentIndex += MaxInventorySize;
      }

      SwitchActiveItem();
    }

    [PunRPC]
    /// <summary>Add Item to inventory slot for pocketable items</summary>
    private void PocketItem( int itemViewID )
    {
      Item newItem = PhotonNetwork.GetPhotonView(itemViewID).GetComponent<Item>();

      // Cannot pocket items if holding a carried item
      if (!lockInventory && AddItemToInventory(newItem))
      {
        // Transfer ownership to player who picked it up
        if (photonView.IsMine)
        {
          newItem.TransferPhotonOwnership();
        }

        itemHandler.ConnectPocketedItemToSlot(newItem);
        newItem.OnPickUpItem();
        newItem.OnUnequipItem();
        SwitchActiveItem();
      }
    }

    private void PocketItemAtIndex( int itemViewID, int index )
    {
      Item newItem = PhotonNetwork.GetPhotonView(itemViewID).GetComponent<Item>();

      // Cannot pocket items if holding a carried item
      if (!lockInventory && AddItemToInventory(newItem, index))
      {
        itemHandler.ConnectPocketedItemToSlot(newItem);
        newItem.OnPickUpItem();
        newItem.OnUnequipItem();
        SwitchActiveItem();
      }
    }

    [PunRPC]
    /// <summary>Add Item to inventory slot for non-pocketable items</summary>
    private void CarryItem( int itemViewID )
    {
      Item newItem = PhotonNetwork.GetPhotonView(itemViewID).GetComponent<Item>();

      // Can only carry item if hand is free
      if (AddItemToInventory(newItem, currentIndex))
      {
        lockInventory = true;

        // Transfer ownership to player who picked it up
        if (photonView.IsMine)
        {
          newItem.TransferPhotonOwnership();
        }

        itemHandler.ConnectCarriedItemToSlot(newItem);
        newItem.OnPickUpItem();

        activeItem = inventoryList[currentIndex];
      }
    }

    [PunRPC]
    private void DragItem( int itemViewID, Vector3 hitPoint )
    {
      Item newItem = PhotonNetwork.GetPhotonView(itemViewID).GetComponent<Item>();

      // Can only carry item if hand is free
      if (AddItemToInventory(newItem, currentIndex))
      {
        lockInventory = true;

        // Zero vector indicates syncing inventory when new player joins
        // Joints will be created by the item handler then
        if (hitPoint != Vector3.zero)
        {
          itemHandler.CreateDragJointWithItem(newItem, hitPoint);
        }

        newItem.OnPickUpItem();

        activeItem = inventoryList[currentIndex];
      }
    }

    /// <summary>
    /// Input System callback when RMB is clicked, to use held item
    /// </summary>
    private void OnUse()
    {
      if (activeItem != null)
      {
        activeItem.OnUseItem();
      }
    }

    /// <summary>
    /// Input System callback when G is pressed, to drop equipped item
    /// </summary>
    private void OnDropItem()
    {
      if (activeItem != null)
      {
        photonView.RPC("DropCurrentItem", RpcTarget.All);
      }
    }

    [PunRPC]
    private void DropCurrentItem()
    {
      itemHandler.DestroyJoint();
      activeItem.OnDropItem();
      activeItem = null;

      RemoveCurrentItem();

      lockInventory = false;
    }

    /// <summary>
    /// Input System callback when mouse wheel is scrolled.
    /// </summary>
    /// <param name="value"></param>
    private void OnScrollInventory( InputValue value )
    {
      float scrollDelta = value.Get<float>();

      if (scrollDelta > 10f) photonView.RPC("ScrollInventorySlots", RpcTarget.All, true);
      else if (scrollDelta < -10f) photonView.RPC("ScrollInventorySlots", RpcTarget.All, false);
    }

    public override void OnPlayerEnteredRoom( Photon.Realtime.Player newPlayer )
    {
      // Sync up the items in the inventory for this new player
      // Loop through inventory and get photon ids of every item 
      List<int> viewIds = new List<int>();
      List<int> indexOfItems = new List<int>();

      for (int i = 0; i < inventoryList.Length; ++i)
      {
        if (inventoryList[i] != null)
        {
          int viewId = inventoryList[i].photonView.ViewID;
          viewIds.Add(viewId);
          indexOfItems.Add(i);
        }
      }

      photonView.RPC("SyncInventoryForNewPlayer", newPlayer, indexOfItems.ToArray(), viewIds.ToArray(), currentIndex);
    }

    [PunRPC]
    private void SyncInventoryForNewPlayer( int[] indexOfItems, int[] viewIds, int activeIndex )
    {
      currentIndex = activeIndex;

      for (int i = 0; i < indexOfItems.Length; ++i)
      {
        int index = indexOfItems[i];
        int viewId = viewIds[i];

        PhotonView itemView = PhotonNetwork.GetPhotonView(viewId);
        Item item = itemView.GetComponent<Item>();

        switch (item.Type)
        {
          case ItemType.Pocketed:
            // TODO: How to sync index of pocketed items?
            // Hacky solution
            PocketItemAtIndex(viewId, index);
            break;

          case ItemType.Carried:
            CarryItem(viewId);
            break;

          case ItemType.Dragged:
            DragItem(viewId, Vector3.zero);
            break;
        }
      }
    }

    public override void OnPlayerLeftRoom( Photon.Realtime.Player otherPlayer )
    {
      // Find the inventory of the player who left
      if (photonView.Owner != otherPlayer) return;

      photonView.RPC("DropAllItemsInInventory", RpcTarget.All);
    }

    [PunRPC]
    private void DropAllItemsInInventory()
    {
      itemHandler.DestroyJoint();

      for (int i = 0; i < inventoryList.Length; ++i)
      {
        if (inventoryList[i] != null)
        {
          inventoryList[i].OnDropItem();
          inventoryList[i].OnEquipItem();
        }
      }
    }
  } // class
}