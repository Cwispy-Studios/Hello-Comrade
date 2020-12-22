using UnityEngine;

using Photon.Pun;

public class PlayerAnimatorController : MonoBehaviourPun
{
  private Animator animator;

  private void Awake()
  {
    animator = GetComponent<Animator>();
  }

  private void FixedUpdate()
  {
    if (!photonView.IsMine) return;
  }
}
