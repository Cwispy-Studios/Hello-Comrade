using Photon.Pun;
using UnityEngine;

namespace CwispyStudios.HelloComrade.Interactions
{
  public class Interactable : MonoBehaviourPun
  {
    public virtual void OnInteract()
    {
    }
    
    public virtual void OnInteract(RaycastHit hit)
    {
    }

    public virtual void OnInteractHold(Vector2 mouseDelta)
    {
    }

    public virtual void OnInteractRelease()
    {
    }
      public void TransferPhotonOwnership()
    {
      if (!photonView.IsMine)
      {
        photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
      }
    }
  }
}