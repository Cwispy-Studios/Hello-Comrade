using System;
using System.Collections;
using System.Collections.Generic;
using CwispyStudios.HelloComrade.Scene_Interactables;
using UnityEngine;

namespace CwispyStudios.HelloComrade.Player
{
    public class InteractionHandler : MonoBehaviour
    {
        [Header("Input Settings")] 
        [SerializeField] private float maxInteractDistance; // Max distance at which the player can interact with an item
        
        private Camera playerCamera;
        private Vector3 centerVector;
        private LayerMask interactablesMask;

        private void Start()
        {
            playerCamera = GetComponentInChildren<Camera>();
            centerVector = new Vector3((int) (Screen.width / 2), (int) (Screen.height / 2), 0);
            interactablesMask = LayerMask.GetMask("Scene Interactables");
        }

        private void OnInteractLMB()
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.ScreenPointToRay(centerVector), out hit, maxInteractDistance,
                interactablesMask))
            {
                hit.collider.gameObject.GetComponent<Interactable>().LeftMouseClick();
            }
        }
    }
}