using UnityEngine;

namespace CwispyStudios.HelloComrade.Audio
{
  public class AmbienceTrigger : MonoBehaviour
  {
    [SerializeField] private AmbienceType ambienceTypeTrigger = AmbienceType.Outdoor;
    public AmbienceType AmbienceTypeTrigger { get { return ambienceTypeTrigger; } }
  }
}
