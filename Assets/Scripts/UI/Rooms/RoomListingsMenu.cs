using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

using System.Collections.Generic;

public class RoomListingsMenu : MonoBehaviourPunCallbacks
{
  [SerializeField] private Transform m_content = null;
  [SerializeField] private RoomListing m_roomListing = null;

  private List<RoomListing> m_listings = new List<RoomListing>();

  public override void OnRoomListUpdate( List<RoomInfo> roomList )
  {
    foreach (RoomInfo roomInfo in roomList)
    {
      // Removed from room list
      if (roomInfo.RemovedFromList)
      {
        // Have to search by room name as details may have changed
        int index = m_listings.FindIndex(x => x.RoomInfo.Name == roomInfo.Name);

        if (index != -1)
        {
          Destroy(m_listings[index].gameObject);
          m_listings.RemoveAt(index);
        }
      }

      // Added to room list
      else
      {
        RoomListing listing = Instantiate(m_roomListing, m_content);

        if (listing != null)
        {
          listing.SetRoomInfo(roomInfo);

          m_listings.Add(listing);
        }
      }
    }
  }
}
