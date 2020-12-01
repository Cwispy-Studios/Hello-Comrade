using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items.Util
{
  public class FlashLight : PocketableItem, IPunObservable
  {
    private bool isOn;

    private Light flashLight;

    private void Awake()
    {
      flashLight = GetComponent<Light>();
    }

    /// <summary>Primary code for execution of item code</summary>
    public override void UseItem()
    {
      isOn = !isOn;
      photonView.RPC("SetLights", RpcTarget.All);
    }

    public void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
      if (stream.IsWriting)
      {
        // We own this player: send the others our data
        stream.SendNext(isOn);
      }
      else
      {
        // Network player, receive data
        isOn = (bool)stream.ReceiveNext();
        SetLights();
      }
    }

    [PunRPC]
    private void SetLights()
    {
      flashLight.enabled = isOn;
    }
  }
}