using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace CwispyStudios.HelloComrade.Scene_Interactables.Lighting
{
  public class LightSwitch : Interactable, IPunObservable
  {
    private static readonly Quaternion SwitchOnPosition = Quaternion.Euler(-65, 0, 0);
    private static readonly Quaternion SwitchOffPosition = Quaternion.Euler(-15, 0, 0);

    private Light[] linkedLights;

    private int id = 0;
    private bool on ;

    [SerializeField] private Transform switchTransform;

    public static void AssignLightIDs()
    {
      LightSwitch[] allSceneLightSwitches = FindObjectsOfType<LightSwitch>();
      int id = 0;
      foreach (var lightSwitch in allSceneLightSwitches)
      {
        lightSwitch.id = id;
        id++;
      }
    }

    private void Awake()
    {
      GetLinkedLights();
    }

    private void GetLinkedLights()
    {
      linkedLights = GetComponentsInChildren<Light>();
    }

    public override void LeftMouseClick()
    {
      photonView.RPC("SwitchLights", RpcTarget.All, id);
    }

    [PunRPC]
    private void SwitchLights(int updateId)
    {
      on = !on;
      SetLights(updateId);
    }

    private void SetLights(int updateId)
    {
      if (updateId != id) return;
      foreach (var linkedLight in linkedLights)
      {
        linkedLight.enabled = on;
        switchTransform.localRotation = on ? SwitchOnPosition : SwitchOffPosition;
      }
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