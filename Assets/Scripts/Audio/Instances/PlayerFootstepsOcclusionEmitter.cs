using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Audio.Instances
{
  public class PlayerFootstepsOcclusionEmitter : Occlusion
  {
    [Header("IMPORTANT!!!!")]
    [SerializeField] private string parameterName = "Ground Type";
    [SerializeField] private Player.GroundDetector groundDetector = null;

    [TextArea]
    public string Notes = "0: Walk, 1: Run, 2: Sneak";

    public void PlayFootstepsWalk()
    {
      if (!photonView.IsMine) return;

      photonView.RPC("PlayEvent", RpcTarget.AllViaServer, 0);
    }

    public void PlayFootstepsRun()
    {
      if (!photonView.IsMine) return;

      photonView.RPC("PlayEvent", RpcTarget.AllViaServer, 1);
    }

    public void PlayFootstepsSneak()
    {
      if (!photonView.IsMine) return;

      photonView.RPC("PlayEvent", RpcTarget.AllViaServer, 2);
    }

    [PunRPC]
    private void PlayEvent( int index )
    {
      if (SetEmitterAndCheckWithinListeningDistance(index))
      {
        float value = ConvertGroundTagToValue(groundDetector.GetGroundTag());

        // Set parameters
        SetEmitterParameter(parameterName, value);
        OccludeEventAndStartEmitter();
      }
    }

    private float ConvertGroundTagToValue( string layerName )
    {
      switch (layerName)
      {
        case "Hardwood":
          return 0f;

        case "Grass":
          return 1f;

        case "Asphalt":
          return 0f;

        case "Concrete":
          return 0f;

        default: return 0f;
      }
    }
  }
}

