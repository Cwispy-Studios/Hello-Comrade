using UnityEngine;

using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player
{
  public class ActiveHand : MonoBehaviourPun
  {
    [SerializeField] private LayerMask lookAtMask = 0;

    private MouseLook mouseLook = null;

    [HideInInspector] public bool IsPointing = true;

    private void Awake()
    {
      mouseLook = GetComponentInParent<MouseLook>();
    }

    private void LateUpdate()
    {
      if (!photonView.IsMine) return;

      if (IsPointing)
      {
        // Default look position is looking in front of player
        Vector3 lookAtPosition = mouseLook.transform.forward;

        if (IsPointing && mouseLook.GetLookingAtObject(100f, lookAtMask, out RaycastHit hit))
        {
          lookAtPosition = hit.point;
        }

        transform.LookAt(lookAtPosition);
      }
    }
  }
}
