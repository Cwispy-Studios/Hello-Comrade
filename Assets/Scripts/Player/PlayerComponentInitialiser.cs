using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal; // Universal Additional Camera Data

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player
{
  using Audio;

  public class PlayerComponentInitialiser : MonoBehaviourPun
  {
    private void Awake()
    {
      Camera playerCamera = GetComponentInChildren<Camera>();

      if (!photonView.IsMine)
      {
        Destroy(playerCamera.GetUniversalAdditionalCameraData());
        Destroy(playerCamera.GetComponent<FMODUnity.StudioListener>());
        Destroy(playerCamera);
        Destroy(GetComponent<PlayerInput>());
        Destroy(GetComponent<InteractionHandler>());
        Destroy(GetComponent<MouseLook>());
        Destroy(GetComponent<AmbienceTriggerDetector>());
        Destroy(GetComponent<InteractionHandler>());
      }

      Destroy(this);
    }
  }
}
