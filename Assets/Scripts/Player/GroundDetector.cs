using UnityEngine;

using ExitGames.Client.Photon; // SendOptions
using Photon.Pun; // RaiseEvent
using Photon.Realtime; // RaiseOptions

namespace CwispyStudios.HelloComrade.Player
{
  public class GroundDetector : MonoBehaviourPun
  {
    private Collider playerCollider;

    [HideInInspector] public bool WasGrounded { get; private set; } = false;
    [HideInInspector] public bool IsGrounded { get; private set; } = false;
    [HideInInspector] public float GroundAngle { get; private set; }
    [HideInInspector] public RaycastHit GroundHit { get; private set; }

    private void Awake()
    {
      playerCollider = GetComponent<Collider>();

      if (playerCollider == null)
      {
        Debug.LogError("Error! Player's Collider component is missing!", this);
      }
    }

    private void Start()
    {
      DetectGround();

      WasGrounded = IsGrounded;
    }

    private void Update()
    {
      if (!photonView.IsMine) return;

      DetectGround();
    }

    private void DetectGround()
    {
      float colliderExtent = playerCollider.bounds.extents.y;

      Vector3 rayOrigin = transform.position;
      rayOrigin.y += colliderExtent;

      float rayDistance = colliderExtent + 0.01f;
      float spherecastRadius = 0.1f;

      Ray ray = new Ray(rayOrigin, Vector3.down);
      WasGrounded = IsGrounded;
      IsGrounded = Physics.SphereCast(ray, spherecastRadius, out RaycastHit hit, rayDistance, (1 << 0));

      // Land on the ground
      if (!WasGrounded && IsGrounded)
      {
        PhotonNetwork.RaiseEvent(
          PhotonEvents.GroundDetectorOnLandEventCode, photonView.ViewID, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
      }

      if (IsGrounded)
        GroundHit = hit;
      GroundAngle = Vector3.Angle(GroundHit.normal, transform.up);
    }

    public float GetGroundLayerValue()
    {
      if (GroundHit.transform)
        return ConvertGroundTagToValue(GroundHit.collider.tag);

      else return 0f;
    }

    private float ConvertGroundTagToValue( string tagName )
    {
      switch (tagName)
      {
        case "Hardwood":
          return 0f;

        case "Grass":
          return 1f;

        case "Asphalt":
          return 2f;

        case "Concrete":
          return 0f;

        default: return 0f;
      }
    }
  }
}