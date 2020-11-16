using UnityEngine;

using Photon.Pun;

public class PlayerAnimatorManager : MonoBehaviourPun
{
  [SerializeField] private float directionDampTime = 0.25f;

  private Animator m_animator = null;

  private void Start()
  {
    m_animator = GetComponent<Animator>();

    if (!m_animator)
    {
      Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
    }
  }

  private void Update()
  {
    if (!photonView.IsMine && PhotonNetwork.IsConnected) return;
    if (!m_animator) return;

    AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);

    if (stateInfo.IsName("Base Layer.Run"))
    {
      if (Input.GetButtonDown("Fire2"))
      {
        m_animator.SetTrigger("Jump");
      }
    }

    float h = Input.GetAxis("Horizontal");
    float v = Input.GetAxis("Vertical");

    if (v < 0) v = 0;

    m_animator.SetFloat("Speed", h * h + v * v);
    m_animator.SetFloat("Direction", h, directionDampTime, Time.deltaTime);
  }
}
