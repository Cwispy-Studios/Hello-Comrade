using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using TMPro;

namespace CwispyStudios.HelloComrade.Interactions.Doors
{
    public class DoorInteraction : Interactable, IPunObservable
    {
        [SerializeField] private Rigidbody door;
        [SerializeField] private float forceMultiplier = 0.2f;
        private Vector3 pointOfInteraction;
        private bool isInUse;

        public override void OnInteract(RaycastHit hit)
        {
            if (!isInUse)
            {
                TransferPhotonOwnership();
                pointOfInteraction = hit.collider.attachedRigidbody.position;
                isInUse = true;
            }
        }

        public override void OnInteractHold(Vector2 mouseDelta)
        {
                Vector2 appliedForce = mouseDelta * forceMultiplier;
                door.AddForceAtPosition(appliedForce, pointOfInteraction, ForceMode.VelocityChange);
        }

        public override void OnInteractRelease()
        {
            isInUse = false;
        }

        public void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
        {
            if (stream.IsWriting)
            {
                stream.SendNext(isInUse);
            }
            else
            {
                isInUse = (bool) stream.ReceiveNext();
            }
        }
    }
}

