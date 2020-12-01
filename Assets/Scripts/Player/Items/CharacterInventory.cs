using UnityEngine;
using UnityEngine.InputSystem;

namespace CwispyStudios.HelloComrade.Player.Items
{
  public class CharacterInventory : MonoBehaviour
  {
    private const int MaxInventorySize = 1;

    // Inventory
    public Item[] inventoryArray = new Item[MaxInventorySize];
    private int itemsInInventory = 0;
    private int currentIndex = 0;
    private bool lockInventory = false; // true if current item picked up is a non-pocketable item
    
    // Interaction with other components
    //[Header("Linked Components")]
    //[SerializeField] private Rigidbody characterRigidbody = null;

    //[Header("Inventory Attributes")] 
    //public float TotalWeight = 0;

    private void Awake()
    {
      //if (characterRigidbody == null) return;
      //TotalWeight = characterRigidbody.mass;
    }

    /// <summary>Scroll through items in inventory</summary>
    public void ScrollInventorySlots( bool positiveIncrement )
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

      SwitchActiveGameObject();
    }

    /// <summary>Add Item to inventory slot for non-pocketable items</summary>
    public void HoldItem( Item newItem )
    {
      AddItem(newItem);

      lockInventory = true;
    }

    /// <summary>Add Item to inventory slot for pocketable items</summary>
    public void PocketItem( Item newItem )
    {
      AddItem(newItem);
    }

    public void InteractHeldItem()
    {
      inventoryArray[currentIndex].UseItem();
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
      if (freeId == MaxInventorySize) return;

      currentIndex = freeId;
      
      itemsInInventory++;

      SetWeight(newItem.ItemMass);

      inventoryArray[currentIndex] = newItem;
      
      newItem.EquipItem();
      
      // TODO Add call later to UI for updating inventory
    }

    private void RemoveItem(int removeId)
    {
      lockInventory = false;
      
      itemsInInventory--;

      SetWeight(-inventoryArray[removeId].ItemMass);

      inventoryArray[removeId] = null;
      
      // TODO Add call later to UI for updating inventory
    }

    private void SetWeight(float value)
    {
      //TotalWeight += value;
      //characterRigidbody.mass = TotalWeight;
    }

    private int ReturnFreeSlot()
    {
      int freeId = MaxInventorySize; // default return is the size of the inventory

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

    private void OnScrollInventory( InputValue value )
    {
      ScrollInventorySlots(value.Get<float>() >= 0f);
    }
  }
}