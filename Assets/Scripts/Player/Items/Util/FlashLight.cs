using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player.Items.Util
{
  public class FlashLight : PocketedItem, IPunObservable
  {
    private bool isOn;
    private bool lastOnState = false;

    /// <summary>
    /// If flashlight is equipped (object is on the player), light source is in the player's camera (head).
    /// Otherwise it is in the prefab itself.
    /// </summary>
    private Light lightSource;

    public override void Awake()
    {
      base.Awake();

      lightSource = GetComponentInChildren<Light>();
    }

    private void GetLightSourceInPlayer()
    {
      PlayerMovementController player = GetComponentInParent<PlayerMovementController>();
      lightSource = player.GetComponentInChildren<Light>();
    }

    private void SetLights()
    {
      lightSource.enabled = isOn;
    }

    [PunRPC]
    public override void OnEquipItem()
    {
      base.OnEquipItem();

      isOn = lastOnState;
      SetLights();
      // Play audio
    }

    [PunRPC]
    public override void OnUnequipItem()
    {
      lastOnState = isOn;
      isOn = false;
      SetLights();
      // Play audio

      base.OnUnequipItem();
    }

    /// <summary>Primary code for execution of item code</summary>
    public override void OnUseItem()
    {
      isOn = !isOn;
      lastOnState = isOn;
      SetLights();
      // Play audio
    }

    [PunRPC]
    public override void OnPickUpItem( int viewID )
    {
      base.OnPickUpItem(viewID);

      // Turn off original light source
      lightSource.enabled = false;
      GetLightSourceInPlayer();

      SetLights();
    }

    [PunRPC]
    public override void OnDropItem()
    {
      lightSource.enabled = false;
      isOn = lastOnState;

      base.OnDropItem();

      lightSource = GetComponentInChildren<Light>();

      SetLights();
    }

    public void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
      if (stream.IsWriting)
      {
        stream.SendNext(isOn);
        stream.SendNext(lastOnState);
      }

      else
      {
        bool newState = (bool)stream.ReceiveNext();

        if (newState != isOn)
        {
          isOn = newState;
          SetLights();
          // Play audio
        }

        lastOnState = (bool)stream.ReceiveNext();
      }
    }
  }
}