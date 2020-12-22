using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Audio
{
  public class AmbienceTriggerDetector : MonoBehaviourPun
  {
    [SerializeField] private AudioEmitter outdoorAmbience = null;
    [SerializeField] private AudioEmitter indoorAmbience = null;

    [SerializeField] private AudioEmitter outdoorReverb = null;
    [SerializeField] private AudioEmitter indoorReverb = null;

    private AmbienceType currentAmbienceType = AmbienceType.None;
    private AudioEmitter currentEmitter = null;
    private AudioEmitter currentReverb = null;

    private void Awake()
    {
      if (!photonView.IsMine) return;

      outdoorAmbience.Initialise(transform);
      indoorAmbience.Initialise(transform);
      outdoorReverb.Initialise(transform);
      indoorReverb.Initialise(transform);
    }

    private void OnTriggerEnter( Collider other )
    {
      if (!photonView.IsMine) return;

      // Ambience Trigger layer
      if (other.gameObject.layer == 14)
      {
        AmbienceType triggeredAmbienceType = other.GetComponent<AmbienceTrigger>().AmbienceTypeTrigger;

        switch (triggeredAmbienceType)
        {
          case AmbienceType.Outdoor:

            if (currentAmbienceType != triggeredAmbienceType)
            {
              if (currentEmitter != null)
              {
                currentEmitter.StopSound();
                currentReverb.StopSound(false);
              }

              currentEmitter = outdoorAmbience;
              currentReverb = outdoorReverb;
              currentEmitter.PlaySound();
              currentReverb.PlaySound();
            }

            break;

          case AmbienceType.Indoor:

            if (currentAmbienceType != triggeredAmbienceType)
            {
              if (currentEmitter != null)
              {
                currentEmitter.StopSound();
                currentReverb.StopSound(false);
              }

              currentEmitter = indoorAmbience;
              currentReverb = indoorReverb;
              currentEmitter.PlaySound();
              currentReverb.PlaySound();
            }

            break;

          default:
            break;
        }

        currentAmbienceType = triggeredAmbienceType;
      }
    }
  }
}
