using UnityEngine;
using UnityEngine.UI;

using Photon.Realtime;

public class RoomListing : MonoBehaviour
{
  [SerializeField] private Text m_text = null;

  public RoomInfo RoomInfo { get; private set; }

  public void SetRoomInfo(RoomInfo roomInfo)
  {
    RoomInfo = roomInfo;
    m_text.text = roomInfo.MaxPlayers + ", " + roomInfo.Name;
  }
}
