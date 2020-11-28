using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace CwispyStudios.HelloComrade.Scene_Interactables.Lighting
{
    public class LightSwitch : Interactable
    {
        private static Quaternion switchOnPosition = Quaternion.Euler(-65, 0, 0);
        private static Quaternion switchOffPosition = Quaternion.Euler(35, 0, 0);
        
        private Light[] linkedLights;
        private PhotonView photonView;

        [SerializeField] private Transform switchTransform;

        private void Awake()
        {
            GetLinkedLights();
            photonView = GetComponent<PhotonView>();
        }

        private void GetLinkedLights()
        {
            linkedLights = GetComponentsInChildren<Light>();
        }

        public override void LeftMouseClick()
        {
            photonView.RPC("SwitchLights", RpcTarget.All);
            SwitchLights();
        }

        [PunRPC]
        private void SwitchLights()
        {
            foreach (var linkedLight in linkedLights)
            {
                linkedLight.enabled = !linkedLight.enabled;
                switchTransform.localRotation = linkedLight.enabled ? switchOnPosition : switchOffPosition;
            }
        }
    }
}