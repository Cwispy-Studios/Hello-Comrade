using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Items
{
  public class CharacterInventory : MonoBehaviour
  {
    private const int InventorySize = 4;

    // Inventory
    private Item[] inventoryArray = new Item[InventorySize];
    private int itemsInInventory = 0;
    private int currentIndex = 0;
    private bool lockInventory = false; // true if current item picked up is a non-pocketable item
    
    // Interaction with other components
    [Header("Linked Components")]
    [SerializeField] private Rigidbody characterRigidbody = null;

    [Header("Inventory Attributes")] 
    public float TotalWeight = 0;

    private void Start()
    {
      if (characterRigidbody == null) return;
      TotalWeight = characterRigidbody.mass;
    }

    /// <summary>Scroll through items in inventory</summary>
    public void ScrollItem(bool positiveIncrement)
    {
      if (lockInventory)
        DropItem(currentIndex);

      // Switch currentIndex and use SwitchActiveGameObject to physically change object 
      currentIndex += positiveIncrement ? 1 : -1;
      SwitchActiveGameObject();
    }

    /// <summary>Add Item to inventory slot for non-pocketable items</summary>
    public void HoldItem(Item newItem)
    {
      AddItem(newItem);

      lockInventory = true;
    }

    /// <summary>Add Item to inventory slot for pocketable items</summary>
    public void PocketItem(Item newItem)
    {
      AddItem(newItem);
    }

    /// <summary>Drop Item from inventory slot into the world</summary>
    public void DropItem(int id)
    {
      // Get item and execute use code
      if (inventoryArray[id] == null) return;
      inventoryArray[id].DropItem();
      RemoveItem(id);
    }

    /// <summary>Remove Item from inventory slot and delete item</summary>
    public void ConsumeItem(int id)
    {
      // Get item and execute use code
      if (inventoryArray[id] == null) return;
      inventoryArray[id].UseItem();
      RemoveItem(id);
    }

    private void AddItem(Item newItem)
    {
      int freeId = ReturnFreeSlot();
      if (freeId == InventorySize) return;

      currentIndex = freeId;
      
      itemsInInventory++;

      SetWeight(newItem.Weight);

      inventoryArray[currentIndex] = newItem;
      
      newItem.EquipItem();
      
      // TODO Add call later to UI for updating inventory
    }

    private void RemoveItem(int removeId)
    {
      lockInventory = false;
      
      itemsInInventory--;

      SetWeight(-inventoryArray[removeId].Weight);

      inventoryArray[removeId] = null;
      
      // TODO Add call later to UI for updating inventory
    }

    private void SetWeight(int value)
    {
      TotalWeight += value;
      characterRigidbody.mass = TotalWeight;
    }

    private int ReturnFreeSlot()
    {
      int freeId = InventorySize; // default return is the size of the inventory

      for (int i = 0; i < inventoryArray.Length; i++)
      {
        if (inventoryArray[i] != null) continue;
        freeId = i;
        break;
      }

      return freeId;
    }

    /// <summary>Deactivate old GameObject and Activate new GameObject</summary>
    private void SwitchActiveGameObject()
    {
    }
  }
}