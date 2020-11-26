using UnityEngine;

using Photon.Pun;

using FMODUnity;
using FMOD.Studio;

namespace CwispyStudios.HelloComrade.Audio
{
  [System.Serializable]
  public class AudioEmitter
  {
    [Header("FMOD Event")]
    [SerializeField, EventRef] public string audioEvent;
    private EventInstance audioEventInstance;
    private EventDescription audioEventDescription;

    [Header("Occlusion Options")]
    [SerializeField, Range(0f, 10f)] private float soundOcclusionWidening = 1f;
    [SerializeField, Range(0f, 10f)] private float playerOcclusionWidening = 1f;
    [SerializeField] private LayerMask occlusionLayer = 0;

    private float maxDistanceAudible = 0f;

    // Transform of the object that will emit the sound
    private Transform sourceObject;
    // The local player who will hear the sound
    private GameObject listener;

    public void Initialise( Transform source, bool sourceIsLocalPlayer = false )
    {
      sourceObject = source;

      // If this audio emitter emits from the local player, occlusion should be disabled
      // So that sounds are not accidentally occluded to the player itself
      if (sourceIsLocalPlayer)
      {
        soundOcclusionWidening = 0f;
        playerOcclusionWidening = 0f;
      }

      audioEventInstance = RuntimeManager.CreateInstance(audioEvent);

      audioEventDescription = RuntimeManager.GetEventDescription(audioEvent);
      audioEventDescription.getMaximumDistance(out maxDistanceAudible);

      listener = FindLocalListener();
    }

    private GameObject FindLocalListener()
    {
      var allListeners = Object.FindObjectsOfType<StudioListener>();

      foreach (StudioListener studioListener in allListeners)
      {
        GameObject listenerObject = studioListener.gameObject;
        GameObject parentObject = listenerObject.transform.parent.gameObject;

        // Find the camera that belongs to the local player
        if (parentObject.GetPhotonView().IsMine)
        {
          return listenerObject;
        }
      }

      return null;
    }

    private int GetOcclusionAmount()
    {
      Vector3 soundLeft = sourceObject.position + (-sourceObject.right * soundOcclusionWidening);
      Vector3 soundRight = sourceObject.position + (sourceObject.right * soundOcclusionWidening);

      Vector3 soundAbove = sourceObject.position + (sourceObject.up * soundOcclusionWidening);

      Vector3 listenerLeft = listener.transform.position + (-listener.transform.right * playerOcclusionWidening);
      Vector3 listenerRight = listener.transform.position + (listener.transform.right * playerOcclusionWidening);

      Vector3 listenerAbove = listener.transform.position + (listener.transform.up * playerOcclusionWidening);

      int totalNumHits = 0;

      totalNumHits += CastLines(soundLeft, listenerLeft, occlusionLayer);
      totalNumHits += CastLines(soundLeft, listener.transform.position, occlusionLayer);
      totalNumHits += CastLines(soundLeft, listenerRight, occlusionLayer);

      totalNumHits += CastLines(sourceObject.position, listenerLeft, occlusionLayer);
      totalNumHits += CastLines(sourceObject.position, listener.transform.position, occlusionLayer);
      totalNumHits += CastLines(sourceObject.position, listenerRight, occlusionLayer);

      totalNumHits += CastLines(soundRight, listenerLeft, occlusionLayer);
      totalNumHits += CastLines(soundRight, listener.transform.position, occlusionLayer);
      totalNumHits += CastLines(soundRight, listenerRight, occlusionLayer);

      totalNumHits += CastLines(soundAbove, listenerAbove, occlusionLayer);

      Debug.Log(totalNumHits);

      return totalNumHits;
    }

    private int CastLines( Vector3 start, Vector3 end, LayerMask occlusionLayer )
    {
      Vector3 startPoint = start;
      Vector3 endPoint = end;

      int numHits = 0;

      while (Physics.Linecast(startPoint, endPoint, out RaycastHit hit, occlusionLayer))
      {
        Debug.DrawLine(startPoint, endPoint, Color.red, 1f);

        startPoint = Vector3.MoveTowards(hit.point, endPoint, 0.1f);
        ++numHits;
      }

      return numHits;
    }

    public void SetParameter( string paramName, float paramValue )
    {
      audioEventInstance.setParameterByName(paramName, paramValue);
    }

    public void PlaySound()
    {
      // Other parameters should have already been set
      if (soundOcclusionWidening > 0f && playerOcclusionWidening > 0f)
      {
        SetParameter("Occlusion", GetOcclusionAmount());
      }

      audioEventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(sourceObject.position));
      audioEventInstance.start();
    }
  }
}

