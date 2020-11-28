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
        private static Quaternion switchOffPosition = Quaternion.Euler(-15, 0, 0);
        
        private Light[] linkedLights;
        private PhotonView photonView;

        private int id = 0;

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
            photonView = GetComponent<PhotonView>();
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
            if (updateId != id) return;
            foreach (var linkedLight in linkedLights)
            {
                linkedLight.enabled = !linkedLight.enabled;
                switchTransform.localRotation = linkedLight.enabled ? switchOnPosition : switchOffPosition;
            }
        }
    }
}