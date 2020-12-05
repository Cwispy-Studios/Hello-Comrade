using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

namespace CwispyStudios.HelloComrade.Interactions.Doors
{
    public class DoorInteraction : Interactable, IPunObservable
    {
        [SerializeField] private Rigidbody door;
        [SerializeField] private float forceMultiplier = 1f;
        private Vector3 pointOfInteraction;

        public override void OnInteract(RaycastHit hit)
        {
            print("Hello there");
            TransferPhotonOwnership();
            pointOfInteraction = hit.collider.attachedRigidbody.position;
        }

        public override void OnInteractHold(Vector2 mouseDelta)
        {
            Vector2 appliedForce = mouseDelta * forceMultiplier;
            door.AddForceAtPosition(appliedForce, pointOfInteraction);
        }

        public void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
        {
           
        }
    }
}

