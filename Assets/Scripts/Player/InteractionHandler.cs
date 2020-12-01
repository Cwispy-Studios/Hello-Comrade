using UnityEngine;

using CwispyStudios.HelloComrade.Interactions;
using CwispyStudios.HelloComrade.Player.Items;

namespace CwispyStudios.HelloComrade.Player
{
  public class InteractionHandler : MonoBehaviour
  {
    [Header("Input Settings")]
    [SerializeField] private float maxInteractDistance = 1f; // Max distance at which the player can interact with an item
    [SerializeField] private LayerMask interactableMask = (1 << 15);

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

    private void OnUse()
    {
      inventory.InteractHeldItem();
    }
  }
}