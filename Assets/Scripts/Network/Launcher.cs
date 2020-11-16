using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Networking
{
  public class Launcher : MonoBehaviourPunCallbacks
  {
    [SerializeField] private byte maxPlayersPerRoom = 4;

    // Client version number. Users are seperated from each other by this (allows making breaking changes)
    private string m_gameVersion = "p1";
    private bool m_isConnecting = false;
    [Tooltip("The Ui Panel to let the user enter name, connect and play")]
    [SerializeField] private GameObject controlPanel = null;
    [Tooltip("The UI Label to inform the user that the connection is in progress")]
    [SerializeField] private GameObject progressLabel = null;

    private void Awake()
    {
      progressLabel.SetActive(false);
      controlPanel.SetActive(true);
    }

    public void Connect()
    {
      progressLabel.SetActive(true);
      controlPanel.SetActive(false);

      // Check if user already connected, then join randon room
      if (PhotonNetwork.IsConnected)
      {
        PhotonNetwork.JoinRandomRoom();
      }

      // Not connected, connect to Photon Online Server and set game version
      else
      {
        m_isConnecting = PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = m_gameVersion;
      }
    } 

    public override void OnConnectedToMaster()
    {
      Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN.");

      if (m_isConnecting)
      {
        // Try to join a potential existing room. If fail, OnJoinRandomFailed() will be calledback.
        PhotonNetwork.JoinRandomRoom();
      }
    }

    public override void OnDisconnected( DisconnectCause cause )
    {
      Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);

      m_isConnecting = false;
    }

    public override void OnJoinRandomFailed( short returnCode, string message )
    {
      Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. " +
        "No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

      // Failed to join room, maybe none exists yet?
      PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
      Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");

      PhotonNetwork.LoadLevel(1);
    }
  }

}
