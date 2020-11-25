using UnityEngine;

using TMPro;

using Photon.Pun;

namespace Networking
{
  [RequireComponent( typeof( TMP_InputField ) )]
  public class PlayerNameInputField : MonoBehaviour
  {
    private const string PlayerNamePrefKey = "PlayerName";

    private void Start()
    {
      string defaultName = string.Empty;

      TMP_InputField inputField = GetComponent<TMP_InputField>();

      if (inputField != null)
      {
        if (PlayerPrefs.HasKey(PlayerNamePrefKey))
        {
          defaultName = PlayerPrefs.GetString(PlayerNamePrefKey);
          inputField.text = defaultName;
        }
      }

      PhotonNetwork.NickName = defaultName;
    }

    public void SetPlayerName( string value )
    {
      if (string.IsNullOrEmpty(value))
      {
        return;
      }

      PhotonNetwork.NickName = value;

      PlayerPrefs.SetString(PlayerNamePrefKey, value);
    }
  }
}
