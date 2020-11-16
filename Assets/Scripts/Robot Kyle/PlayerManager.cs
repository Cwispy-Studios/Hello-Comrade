using UnityEngine;

using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks
{
  private void Start()
  {
    Photon.Pun.Demo.PunBasics.CameraWork cameraWork = GetComponent<Photon.Pun.Demo.PunBasics.CameraWork>();

    if (cameraWork != null)
    {
      if (photonView.IsMine)
      {
        cameraWork.OnStartFollowing();
      }
    }

    else
    {
      Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
    }
  }
}
