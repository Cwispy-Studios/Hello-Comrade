using UnityEngine;

using TMPro;

using Photon.Pun;
using Photon.Realtime;

public class CreateRoomMenu : MonoBehaviourPunCallbacks
{
  [SerializeField] private TextMeshProUGUI m_roomName = null;

  public void OnClickCreateRoom()
  {
    if (!PhotonNetwork.IsConnected)
    {
      return;
    }
    
    RoomOptions options = new RoomOptions();
    options.MaxPlayers = 4;

    string roomName = m_roomName.text;

    if (string.IsNullOrEmpty(roomName)) roomName = "Just Another Room";

    PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
  }

  public override void OnCreatedRoom()
  {
    Debug.Log("Created room successfully.", this);
  }

  public override void OnCreateRoomFailed( short returnCode, string message )
  {
    Debug.Log("Room creation failed: " + message, this);
  }
}
