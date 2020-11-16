using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class TestConnect : MonoBehaviourPunCallbacks
{
  //[SerializeField] private string gameVersion = "0.0.1";
  private void Start()
  {
    print("Connecting To Server");
    PhotonNetwork.NickName = MasterManager.GameSettings.NickName;
    PhotonNetwork.GameVersion = MasterManager.GameSettings.GameVersion;
    PhotonNetwork.ConnectUsingSettings();
  }

  public override void OnConnectedToMaster()
  {
    print("Connected To Server");
    print(PhotonNetwork.LocalPlayer.NickName);

    PhotonNetwork.JoinLobby();
  }

  public override void OnDisconnected( DisconnectCause cause )
  {
    print("Disconnected From Server for reason " + cause.ToString());
  }
}
