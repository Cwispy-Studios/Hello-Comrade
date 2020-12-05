using UnityEngine;
using UnityEngine.SceneManagement;

using ExitGames.Client.Photon;
using Photon.Pun;

namespace CwispyStudios.HelloComrade.Managers
{
  using Networking;

  public class GameManager : MonoBehaviourPunCallbacks
  {
    [Tooltip("Prefab for the player character model")]
    [SerializeField] private GameObject playerPrefab = null;

    private void Awake()
    {
      if (playerPrefab == null)
      {
        Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
      }

      Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManager.GetActiveScene().name);
      GameObject playerObject = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(1f, 2f, 10f), Quaternion.identity, 0);
      //PhotonNetwork.LocalPlayer.TagObject = playerObject;

      Hashtable customProperties = new Hashtable();
      customProperties[PlayerCustomProperties.PlayerObjectViewID] = playerObject.GetPhotonView().ViewID;
      PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);

      Cursor.visible = false;
      Cursor.lockState = CursorLockMode.Locked;
    }

    private void LoadPlaygroundLevel()
    {
      if (!PhotonNetwork.IsMasterClient)
      {
        Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
      }

      Debug.LogFormat("PhotonNetwork : Loading Playground Level");
      PhotonNetwork.LoadLevel(1);
    }

    public override void OnLeftRoom()
    {
      SceneManager.LoadScene(0);
    }

    public void LeaveRoom()
    {
      PhotonNetwork.LeaveRoom();
    }
  }

}
