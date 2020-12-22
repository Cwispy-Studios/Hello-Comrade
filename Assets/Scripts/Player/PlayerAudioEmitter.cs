using UnityEngine;

using ExitGames.Client.Photon;
using Photon.Pun;

namespace CwispyStudios.HelloComrade.Player
{
  using Audio;

  public class PlayerAudioEmitter : MonoBehaviourPun
  {
    [Header("FMOD Events")]
    [SerializeField] private AudioEmitter footstepsWalkEvent = null;
    [SerializeField] private AudioEmitter footstepsRunEvent = null;
    [SerializeField] private AudioEmitter footstepsSneakEvent = null;
    [SerializeField] private AudioEmitter jumpEvent = null;
    [SerializeField] private AudioEmitter landEvent = null;

    private Animator animator;
    private GroundDetector groundDetector;

    private int animatorSpeedMultiplierHash = 0;

    private void Awake()
    {
      bool isMine = photonView.IsMine;

      footstepsWalkEvent.Initialise(transform, isMine);
      footstepsRunEvent.Initialise(transform, isMine);
      footstepsSneakEvent.Initialise(transform, isMine);
      jumpEvent.Initialise(transform, isMine);
      landEvent.Initialise(transform, isMine);

      animator = GetComponent<Animator>();
      groundDetector = GetComponent<GroundDetector>();

      animatorSpeedMultiplierHash = Animator.StringToHash("Speed Multiplier");
    }

    private void OnEnable()
    {
      groundDetector.LandEvent += OnLand;
      PhotonNetwork.NetworkingClient.EventReceived += JumpEvent;
    }

    private void OnDisable()
    {
      groundDetector.LandEvent -= OnLand;
      PhotonNetwork.NetworkingClient.EventReceived -= JumpEvent;
    }

    /// <summary>
    /// Called by animator event
    /// </summary>
    private void PlayFootsteps()
    {
      float speedMultiplier = animator.GetFloat(animatorSpeedMultiplierHash);

      AudioEmitter eventToPlay;

      if (speedMultiplier > 1.1f) eventToPlay = footstepsRunEvent;
      else if (speedMultiplier < 0.9f) eventToPlay = footstepsSneakEvent;
      else eventToPlay = footstepsWalkEvent;

      eventToPlay.SetParameter("Ground Type", groundDetector.GetGroundLayerValue());
      eventToPlay.PlaySound();
    }

    private void OnLand( float groundLayerValue )
    {
      landEvent.SetParameter("Ground Type", groundLayerValue);
      landEvent.PlaySound();
    }

    private void JumpEvent( EventData photonEvent )
    {
      byte eventCode = photonEvent.Code;
      bool objectIsSender = photonEvent.Sender == photonView.OwnerActorNr;

      if (eventCode == PhotonEvents.OnJumpEventCode && objectIsSender)
      {
        jumpEvent.SetParameter("Ground Type", groundDetector.GetGroundLayerValue());
        jumpEvent.PlaySound();
      }
    }
  }
}
