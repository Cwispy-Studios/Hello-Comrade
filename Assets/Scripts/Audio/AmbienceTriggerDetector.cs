using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Audio
{
  public class AmbienceTriggerDetector : MonoBehaviourPun
  {
    [SerializeField] private AudioEmitter outdoorAmbience = null;
    [SerializeField] private AudioEmitter indoorAmbience = null;

    private AmbienceType currentAmbienceType = AmbienceType.None;
    private AudioEmitter currentEmitter = null;

    private void Awake()
    {
      if (!photonView.IsMine) return;

      outdoorAmbience.Initialise(transform);
      indoorAmbience.Initialise(transform);
    }

    private void OnTriggerEnter( Collider other )
    {
      if (!photonView.IsMine) return;

      // Ambience Trigger layer
      if (other.gameObject.layer == 14)
      {
        AmbienceType triggeredAmbienceType = other.GetComponent<AmbienceTrigger>().AmbienceTypeTrigger;

        Debug.Log(triggeredAmbienceType);

        switch (triggeredAmbienceType)
        {
          case AmbienceType.Outdoor:

            if (currentAmbienceType != triggeredAmbienceType)
            {
              if (currentEmitter != null)
              {
                currentEmitter.StopSound();
              }

              currentEmitter = outdoorAmbience;
              currentEmitter.PlaySound();
            }

            break;

          case AmbienceType.Indoor:

            if (currentAmbienceType != triggeredAmbienceType)
            {
              if (currentEmitter != null)
              {
                currentEmitter.StopSound();
              }

              currentEmitter = indoorAmbience;
              currentEmitter.PlaySound();
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
