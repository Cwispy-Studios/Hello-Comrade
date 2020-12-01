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
    private Camera playerCamera;

    private Vector3 centerScreenVector;

    private void Start()
    {
      inventory = GetComponent<CharacterInventory>();
      playerCamera = GetComponentInChildren<Camera>();

      centerScreenVector = new Vector3(Screen.width / 2, Screen.height / 2, 0f);
    }

    private void OnInteract()
    {
      Ray ray = playerCamera.ScreenPointToRay(centerScreenVector);

      if (Physics.Raycast(ray, out RaycastHit hit, maxInteractDistance, interactableMask))
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