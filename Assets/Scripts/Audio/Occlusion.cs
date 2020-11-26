using UnityEngine;

using Photon.Pun;

using FMODUnity;

namespace CwispyStudios.HelloComrade.Audio
{
  public class Occlusion : MonoBehaviourPun
  {
    [SerializeField] private OcclusionEmitter[] occlusionEmitters;

    private GameObject listener;

    private void Awake()
    {
      foreach (OcclusionEmitter emitter in occlusionEmitters)
      {
        emitter.Initialise(gameObject);
      }
    }

    private void Start()
    {
      // There should only be one by the time this code runs, since every other player will have their camera deleted.
      var allListeners = FindObjectsOfType<StudioListener>();

      foreach (StudioListener studioListener in allListeners)
      {
        GameObject listenerObject = studioListener.gameObject;
        GameObject parentObject = listenerObject.transform.parent.gameObject;

        if (parentObject.GetPhotonView().IsMine)
        {
          listener = listenerObject;
          break;
        }
      }
    }

    private void OccludeBetween( OcclusionEmitter emitter )
    {
      float soundOcclusionWidening = emitter.SoundOcclusionWidening;
      float playerOcclusionWidening = emitter.PlayerOcclusionWidening;
      LayerMask occlusionLayer = emitter.OcclusionLayer;

      Vector3 soundLeft = transform.position + (-transform.right * soundOcclusionWidening);
      Vector3 soundRight = transform.position + (transform.right * soundOcclusionWidening);

      Vector3 soundAbove = transform.position + (transform.up * soundOcclusionWidening);

      Vector3 listenerLeft = listener.transform.position + (-listener.transform.right * playerOcclusionWidening);
      Vector3 listenerRight = listener.transform.position + (listener.transform.right * playerOcclusionWidening);

      Vector3 listenerAbove = listener.transform.position + (listener.transform.up * playerOcclusionWidening);

      int totalNumHits = 0;

      totalNumHits += CastLines(soundLeft, listenerLeft, occlusionLayer);
      totalNumHits += CastLines(soundLeft, listener.transform.position, occlusionLayer);
      totalNumHits += CastLines(soundLeft, listenerRight, occlusionLayer);

      totalNumHits += CastLines(transform.position, listenerLeft, occlusionLayer);
      totalNumHits += CastLines(transform.position, listener.transform.position, occlusionLayer);
      totalNumHits += CastLines(transform.position, listenerRight, occlusionLayer);

      totalNumHits += CastLines(soundRight, listenerLeft, occlusionLayer);
      totalNumHits += CastLines(soundRight, listener.transform.position, occlusionLayer);
      totalNumHits += CastLines(soundRight, listenerRight, occlusionLayer);

      totalNumHits += CastLines(soundAbove, listenerAbove, occlusionLayer);

      emitter.PlaySound(transform.position, totalNumHits);
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

    private void OccludeEvent(OcclusionEmitter emitter)
    {
      float listenerDistance = Vector3.Distance(transform.position, listener.transform.position);

      if (listenerDistance <= emitter.MaxDistanceAudible)
      {
        //if (emitter.PlayerOcclusionWidening != 0 )
        OccludeBetween(emitter);
      }
    }

    [PunRPC]
    public void RpcPlayEvent( int eventIndex )
    {
      OccludeEvent(occlusionEmitters[eventIndex]);
    }
  }
}
