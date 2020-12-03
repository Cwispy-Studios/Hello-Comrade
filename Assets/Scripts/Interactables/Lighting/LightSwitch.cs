using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Interactions.Lighting
{
  using Audio;

  public class LightSwitch : Interactable, IPunObservable
  {
    [SerializeField] private Transform switchTransform = null;
    [SerializeField] private AudioEmitter switchToggleEvent = null;

    private static readonly Quaternion SwitchOnPosition = Quaternion.Euler(-65, 0, 0);
    private static readonly Quaternion SwitchOffPosition = Quaternion.Euler(-15, 0, 0);

    private Light[] linkedLights;

    private bool isOn = false;

    private void Awake()
    {
      linkedLights = GetComponentsInChildren<Light>();

      switchToggleEvent.Initialise(transform);
    }

    private void ToggleLight()
    {
      isOn = !isOn;
      SetLightState();
      PlaySwitchToggleSound();
    }

    private void SetLightState()
    {
      foreach (Light linkedLight in linkedLights)
      {
        linkedLight.enabled = isOn;
        switchTransform.localRotation = isOn ? SwitchOnPosition : SwitchOffPosition;
      }
    }

    private void PlaySwitchToggleSound()
    {
      switchToggleEvent.SetParameter("State", isOn ? 1f : 0f);
      switchToggleEvent.PlaySound();
    }

    public override void OnInteract()
    {
      TransferPhotonOwnership();
      ToggleLight();
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
        bool newState = (bool)stream.ReceiveNext();

        if (newState != isOn)
        {
          isOn = newState;
          SetLightState();
          PlaySwitchToggleSound();
        }

      }
    }
  }
}