using System;
using UnityEngine;
using UnityEngine.InputSystem;

using Photon.Pun;
using UnityEngine.Rendering;

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

    private Interactable interactingObject;
    private Vector2 mouseDelta;
    private bool buttonHeldDown;

    private void Start()
    {
      inventory = GetComponent<CharacterInventory>();
      mouseLook = GetComponent<MouseLook>();
    }

    private void Update()
    {
      if (buttonHeldDown && interactingObject != null)
      {
        interactingObject.OnInteractHold(mouseDelta);
      }
      else if (interactingObject != null)
      {
        interactingObject.OnInteractRelease();
      }
    }

    private void OnLook(InputValue value)
    {
      mouseDelta = value.Get<Vector2>();
    }

    private void OnInteract(InputValue value)
    {
      buttonHeldDown = value.isPressed;
      if (buttonHeldDown)
      {
        if (mouseLook.GetLookingAtObject(maxInteractDistance, interactableMask, out RaycastHit hit))
        {
          interactingObject = hit.collider.GetComponent<Interactable>();
          interactingObject.OnInteract();
        }
      }
      else
      {
        interactingObject = null;
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