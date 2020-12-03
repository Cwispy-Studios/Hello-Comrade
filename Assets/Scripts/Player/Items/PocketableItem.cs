using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items
{
  public class PocketableItem : Item
  {
    [PunRPC]
    public override void OnPickUpItem( int viewID )
    {
      transform.parent = PhotonNetwork.GetPhotonView(viewID).transform;
      transform.localPosition = Vector3.zero;
      transform.localRotation = Quaternion.identity;

      // Set rigidbody to kinematic so physics does not affect it
      GetComponent<Rigidbody>().isKinematic = true;
      // Set collider to trigger so it does not affect the player's rigidbody
      GetComponent<Collider>().isTrigger = true;
    }

    public override void OnDropItem()
    {

    }
  }
}