using Photon.Pun;

namespace CwispyStudios.HelloComrade.Interactions
{
  public class Interactable : MonoBehaviourPun
  {
    public virtual void OnInteract()
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