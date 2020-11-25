using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Audio.Instances
{
  public class PlayerFootstepsOcclusionEmitter : Occlusion
  {
    [TextArea]
    public string Notes = "0: Walk, 1: Run, 2: Sneak";

    public void PlayFootstepsWalk()
    {
      if (!photonView.IsMine) return;

      photonView.RPC("RpcPlayEvent", RpcTarget.AllViaServer, 0);
    }

    public void PlayFootstepsRun()
    {
      if (!photonView.IsMine) return;

      photonView.RPC("RpcPlayEvent", RpcTarget.AllViaServer, 1);
    }

    public void PlayFootstepsSneak()
    {
      if (!photonView.IsMine) return;

      photonView.RPC("RpcPlayEvent", RpcTarget.AllViaServer, 2);
    }
  }
}

