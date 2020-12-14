using UnityEngine;
using Photon.Pun;

namespace CwispyStudios.HelloComrade.Interactions.Doors
{
    public class DoorInteraction : Interactable, IPunObservable
    {
        [SerializeField] private Rigidbody door;
        [SerializeField] private float forceMultiplier = 0.05f;
        //private Vector3 pointOfInteraction;
        private bool isInUse;

        public override void OnInteract()
        {
            if (!isInUse)
            {
                TransferPhotonOwnership();
                //pointOfInteraction = hit.collider.attachedRigidbody.position; ->>was used but not anymore
                isInUse = true;
            }
        }

        public override void OnInteractHold(Vector2 mouseDelta)
        {
            Vector3 appliedForce = mouseDelta * forceMultiplier;
            door.AddForce(appliedForce, ForceMode.VelocityChange);
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

