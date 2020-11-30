using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Player.Items;
using UnityEngine;

namespace CwispyStudios.HelloComrade.Player.Items.Util
{
  public class FlashLight : PocketItem, IPunObservable
  {
    private bool on;
    private int id;
    private Light flashLight;
    /// <summary>Primary code for execution of item code</summary>
    public override void UseItem()
    {
      on = !on;
      SetLights(id);
    }

    private void Awake()
    {
      flashLight = GetComponent<Light>();
      AssignId();
    }

    private void AssignId()
    {
      id = FindObjectsOfType<FlashLight>().Length;
    }

    [PunRPC]
    private void SetLights(int updateId)
    {
      if (updateId != id) return;
      flashLight.enabled = on;
    }
    
    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
      if (stream.IsWriting)
      {
        // We own this player: send the others our data
        stream.SendNext(on);
      }
      else
      {
        // Network player, receive data
        on = (bool)stream.ReceiveNext();
        SetLights(id);
      }
    }

    #endregion
  }
}