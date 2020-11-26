using UnityEngine;

using FMODUnity;
using FMOD.Studio;

[System.Serializable]
public class OcclusionEmitter
{
  [Header("FMOD Event")]
  [SerializeField, EventRef] private string audioEvent = "";
  private EventInstance audioEventInstance;
  private EventDescription audioEventDescription;

  [Header("Occlusion Options")]
  [SerializeField, Range(0f, 10f)] private float soundOcclusionWidening = 1f;
  public float SoundOcclusionWidening { get { return soundOcclusionWidening; } }
  [SerializeField, Range(0f, 10f)] private float playerOcclusionWidening = 1f;
  public float PlayerOcclusionWidening { get { return playerOcclusionWidening; } }
  [SerializeField] private LayerMask occlusionLayer;
  public LayerMask OcclusionLayer { get { return occlusionLayer; } }

  private bool isInitialised = false;

  private float maxDistanceAudible = 0f;
  public float MaxDistanceAudible { get { return maxDistanceAudible; } }

  public void Initialise()
  {
    if (!isInitialised)
    {
      isInitialised = true;

      audioEventInstance = RuntimeManager.CreateInstance(audioEvent);

      audioEventDescription = RuntimeManager.GetEventDescription(audioEvent);
      audioEventDescription.getMaximumDistance(out maxDistanceAudible);
      audioEventDescription.getParameterDescriptionByIndex(0, out PARAMETER_DESCRIPTION paramDesc);

      
    }
  }

  public void SetParameter( string paramName, float paramValue )
  {
    audioEventInstance.setParameterByName(paramName, paramValue);
  }

  public void PlaySound( Vector3 position, int occlusionLevel )
  {
    audioEventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
    audioEventInstance.setParameterByName("Occlusion", occlusionLevel);
    audioEventInstance.start();
  }
}
