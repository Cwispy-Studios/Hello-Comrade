using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Interactions.Lighting
{
  public class LightSwitch : Interactable, IPunObservable
  {
    [SerializeField] private Transform switchTransform = null;

    private static readonly Quaternion SwitchOnPosition = Quaternion.Euler(-65, 0, 0);
    private static readonly Quaternion SwitchOffPosition = Quaternion.Euler(-15, 0, 0);

    private Light[] linkedLights;

    private bool isOn = false;

    private void Awake()
    {
      linkedLights = GetComponentsInChildren<Light>();
    }

    private void ToggleLight()
    {
      isOn = !isOn;
      SetLightState();
    }

    private void SetLightState()
    {
      foreach (Light linkedLight in linkedLights)
      {
        linkedLight.enabled = isOn;
        switchTransform.localRotation = isOn ? SwitchOnPosition : SwitchOffPosition;
      }
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
        isOn = (bool)stream.ReceiveNext();
        SetLightState();
      }
    }
  }
}