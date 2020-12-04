using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player
{
  using Interactions;
  using Items;

  public class InteractionHandler : MonoBehaviour
  {
    [Header("Input Settings")]
    [SerializeField] private float maxInteractDistance = 1f; // Max distance at which the player can interact with an item
    [SerializeField] private LayerMask interactableMask = (1 << 15);
    [SerializeField] private LayerMask itemMask = 1;

    // Cache components
    private CharacterInventory inventory;
    private MouseLook mouseLook;

    private void Start()
    {
      inventory = GetComponent<CharacterInventory>();
      mouseLook = GetComponent<MouseLook>();
    }

    private void OnInteract()
    {
      if (mouseLook.GetLookingAtObject(maxInteractDistance, interactableMask, out RaycastHit hit))
      {
        hit.collider.GetComponent<Interactable>().OnInteract();
      }
    }
    
    private void OnInteractHold()
    {
      if (mouseLook.GetLookingAtObject(maxInteractDistance, interactableMask, out RaycastHit hit))
      {
        hit.collider.GetComponent<Interactable>().OnInteract(hit);
      }
    }

    private void OnPickUp()
    {
      // Find the object player is looking at 
      if (mouseLook.GetLookingAtObject(maxInteractDistance, itemMask, out RaycastHit hit))
      {
        // Check if the object is an item
        Item item = hit.collider.GetComponent<Item>();

        if (item)
        {
          ItemType itemType = item.Type;

          switch (itemType)
          {
            case ItemType.Pocketed:
              inventory.PocketItem(item);
              break;

            case ItemType.Carried:
              inventory.CarryItem(item);
              break;

            case ItemType.Dragged:
              break;
          }
        }
      }
    }
  }
}