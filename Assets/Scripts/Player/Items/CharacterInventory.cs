using UnityEngine;
using UnityEngine.InputSystem;

namespace CwispyStudios.HelloComrade.Player.Items
{
  public class CharacterInventory : MonoBehaviour
  {
    [Tooltip("The wrist that will hold active pocketable items. Should be the right wrist.")]
    [SerializeField] private ActiveHand activeHand = null;

    private const int MaxInventorySize = 1;

    // Inventory
    public Item[] inventoryList = new Item[MaxInventorySize];
    private Item activeItem = null;
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

      SwitchActiveItem();
    }

    /// <summary>Add Item to inventory slot for non-pocketable items</summary>
    public void CarryItem( Item newItem )
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
      inventoryList[currentIndex].UseItem();
    }

    /// <summary>Drop Item from inventory slot into the world</summary>
    public void DropItem(int id)
    {
      // Get item and execute use code
      if (inventoryList[id] == null) return;
      inventoryList[id].DropItem();
      RemoveItem(id);
    }

    /// <summary>Remove Item from inventory slot and delete item</summary>
    public void ConsumeItem(int id)
    {
      // Get item and execute use code
      if (inventoryList[id] == null) return;
      inventoryList[id].UseItem();
      RemoveItem(id);
    }

    private void AddItem(Item newItem)
    {
      int freeId = ReturnFreeSlot();
      if (freeId == MaxInventorySize) return;

      currentIndex = freeId;
      
      itemsInInventory++;

      SetWeight(newItem.ItemMass);

      inventoryList[currentIndex] = newItem;
      
      newItem.EquipItem();
      
      // TODO Add call later to UI for updating inventory
    }

    private void RemoveItem(int removeId)
    {
      lockInventory = false;
      
      itemsInInventory--;

      SetWeight(-inventoryList[removeId].ItemMass);

      inventoryList[removeId] = null;
      
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

      for (int i = 0; i < inventoryList.Length; i++)
      {
        if (inventoryList[i] != null) continue;
        freeId = i;
        break;
      }

      return freeId;
    }

    /// <summary>Deactivate old GameObject and Activate new GameObject</summary>
    private void SwitchActiveItem()
    {
    }

    /// <summary>
    /// Input System callback when mouse wheel is scrolled.
    /// </summary>
    /// <param name="value"></param>
    private void OnScrollInventory( InputValue value )
    {
      ScrollInventorySlots(value.Get<float>() > 0f);
    }
  }
}