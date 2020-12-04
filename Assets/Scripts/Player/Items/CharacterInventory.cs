using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  public class CharacterInventory : MonoBehaviourPunCallbacks
  {
    [Tooltip("The active item slot where active items will become a parent of. This is in the right wrist of the skeleton.")]
    [SerializeField] private GameObject activeSlot = null;

    private const int MaxInventorySize = 4;

    // Inventory
    private Item[] inventoryList;
    private Item activeItem = null;
    private int itemsInInventory = 0;
    private int currentIndex = 0;
    private bool lockInventory = false; // true if current item picked up is a non-pocketable item
    

    private void Awake()
    {
      inventoryList = new Item[MaxInventorySize];
      //if (characterRigidbody == null) return;
      //TotalWeight = characterRigidbody.mass;
    }

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

    /// <summary>Add Item to inventory slot for pocketable items</summary>
    public void PocketItem( Item newItem )
    {
      if (AddItemToInventory(newItem))
      {
        // Transfer ownership to local player who picked it up
        newItem.TransferPhotonOwnership();
        newItem.photonView.RPC("OnPickUpItem", RpcTarget.All, activeSlot.GetPhotonView().ViewID);
      }
    }

    /// <summary>Add Item to inventory slot for non-pocketable items</summary>
    public void CarryItem( Item newItem )
    {
      // Can only carry item if hand is free
      //if ()
      //AddItemToInventory(newItem);

      //lockInventory = true;
    }

    /// <summary>Drop Item from inventory slot into the world</summary>
    public void DropItem(int id)
    {
      // Get item and execute use code
      if (inventoryList[id] == null) return;
      inventoryList[id].OnDropItem();
      RemoveItem(id);
    }

    /// <summary>Remove Item from inventory slot and delete item</summary>
    public void ConsumeItem(int id)
    {
      // Get item and execute use code
      if (inventoryList[id] == null) return;
      inventoryList[id].OnUseItem();
      RemoveItem(id);
    }

    private bool AddItemToInventory( Item newItem )
    {
      // Find the index of a free item slot
      // If no free slot (-1), return false
      int freeIndex = ReturnFreeSlot();
      if (freeIndex == -1) return false;

      // Character gets heavier from weight of item
      AddItemMassToCharacterMass(newItem.ItemMass);

      // Assign the item to the found index of the inventory list
      inventoryList[freeIndex] = newItem;

      // If new item is in the active slot
      if (freeIndex == currentIndex)
      {
        SwitchActiveItem();
      }

      else
      {
        newItem.OnUnequipItem();
      }

      // TODO Add call later to UI for updating inventory
      
      return true;
    }

    private void RemoveItem(int removeId)
    {
      lockInventory = false;
      
      itemsInInventory--;

      AdjustCharacterMass(-inventoryList[removeId].ItemMass);

      inventoryList[removeId] = null;
      
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

    /// <summary>Deactivate old GameObject and Activate new GameObject</summary>
    private void SwitchActiveItem()
    {
      if (activeItem != null)
      {
        activeItem.photonView.RPC("OnUnequipItem", RpcTarget.All);
      }

      activeItem = inventoryList[currentIndex];

      if (activeItem != null)
      {
        activeItem.photonView.RPC("OnEquipItem", RpcTarget.All);
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
    /// Input System callback when mouse wheel is scrolled.
    /// </summary>
    /// <param name="value"></param>
    private void OnScrollInventory( InputValue value )
    {
      float scrollDelta = value.Get<float>();

      if (scrollDelta > 10f) ScrollInventorySlots(true);
      else if (scrollDelta < -10f) ScrollInventorySlots(false);
    }

    public override void OnPlayerEnteredRoom( Photon.Realtime.Player newPlayer )
    {
      // Sync up the items in the inventory for this new player
      // Loop through inventory and get photon ids of every item 
      List<int> viewIds = new List<int>();

      int viewIdOfActiveItem = -1;

      for (int i = 0; i < inventoryList.Length; ++i)
      {
        if (inventoryList[i] != null)
        {
          int viewId = inventoryList[i].photonView.ViewID;
          viewIds.Add(viewId);

          // Get the view id of the active item, this will be active and all the others are inactive
          if (currentIndex == i)
          {
            viewIdOfActiveItem = viewId;
          }
        }
      }

      photonView.RPC("SyncInventoryItemsForNewPlayer", RpcTarget.AllViaServer, newPlayer, viewIds.ToArray(), viewIdOfActiveItem);
    }

    [PunRPC]
    private void SyncInventoryItemsForNewPlayer( Photon.Realtime.Player newPlayer, int[] viewIds, int viewIdOfActiveItem )
    {
      // Only sync for the new player that joined
      if (PhotonNetwork.LocalPlayer != newPlayer) return;

      for (int i = 0; i < viewIds.Length; ++i)
      {
        int viewId = viewIds[i];

        PhotonView itemView = PhotonNetwork.GetPhotonView(viewId);
        Item item = itemView.GetComponent<Item>();
        item.OnPickUpItem(activeSlot.GetPhotonView().ViewID);

        if (viewIdOfActiveItem == viewId)
        {
          item.OnEquipItem();
        }

        else
        {
          item.OnUnequipItem();
        }
      }
    }
  }
}