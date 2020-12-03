using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

namespace CwispyStudios.HelloComrade.Interactions.Doors
{
    public class DoorInteraction : Interactable, IPunObservable
    {
        [SerializeField] private Rigidbody Door;
        [SerializeField] private float forceMultiplier = 10f;
        void Awake()
        {
        
        }

        public override void OnInteract(RaycastHit hit)
        {
            print("Hello there");
            TransferPhotonOwnership();
            MoveDoor(hit);
        }

        private void MoveDoor(RaycastHit hit)
        {
            Vector3 appliedForce = (hit.point - Door.transform.position) * forceMultiplier;
            Door.AddForce(appliedForce);
            //Door.AddForceAtPosition(appliedForce);
        }

        public void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
        {
           
        }
        
        //Save raycast hit position when trying to first interact with object as reference
        //Use the reference as an offset for calculating the force.
        // link: https://www.youtube.com/watch?v=254geR2-w5c&feature=emb_logo
    }
}

