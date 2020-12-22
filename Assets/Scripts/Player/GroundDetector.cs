using System;

using UnityEngine;

using ExitGames.Client.Photon; // SendOptions
using Photon.Pun; // RaiseEvent
using Photon.Realtime; // RaiseOptions

namespace CwispyStudios.HelloComrade.Player
{
  public class GroundDetector : MonoBehaviourPun
  {
    private Collider playerCollider;

    /// <summary>
    /// Whether the player is on the ground the previous fixed update.
    /// </summary>
    [HideInInspector] public bool WasGrounded { get; private set; } = false;
    /// <summary>
    /// Whether the player is on the ground this current fixed update.
    /// </summary>
    [HideInInspector] public bool IsGrounded { get; private set; } = false;
    /// <summary>
    /// Whether the player should be falling because they are a certain distance
    /// from the ground. Note: A player can both be grounded and falling at the same time.
    /// </summary>
    [HideInInspector] public bool IsFalling { get; private set; } = false;
    /// <summary>
    /// The raycast information of the last detected object below the player.
    /// </summary>
    [HideInInspector] public RaycastHit GroundHit { get; private set; }

    public event Action <float> LandEvent;

    private void Awake()
    {
      playerCollider = GetComponent<Collider>();
    }

    private void Start()
    {
      DetectGround();

      WasGrounded = IsGrounded;
    }

    private void Update()
    {
      DetectGround();
    }

    private void DetectGround()
    {
      float colliderExtent = playerCollider.bounds.extents.y;

      Vector3 rayOrigin = transform.position;
      rayOrigin.y += colliderExtent;

      float rayDistance = colliderExtent + 0.15f;
      float spherecastRadius = 0.2f;

      Ray ray = new Ray(rayOrigin, Vector3.down);
      WasGrounded = IsGrounded;
      bool foundGround = Physics.SphereCast(ray, spherecastRadius, out RaycastHit hit, rayDistance, (1 << 0));

      if (foundGround)
      {
        IsGrounded = true;
        GroundHit = hit;

        // Find the distance to ground
        float distanceToGround = hit.distance - colliderExtent;

        // Distance to ground is miniscule, player is firmly grounded and will not need to fall
        if (distanceToGround <= 0.01f)
        {
          IsFalling = false;
        }

        // Small gaps and minor falls, do not trigger fall animation but should still apply gravity
        // Player is still grounded in such cases, and animation will not trigger
        else
        {
          IsFalling = true;
        }
      }
      
      else
      {
        IsGrounded = false;
        IsFalling = true;
      }

      // Landed on the ground
      if (!WasGrounded && IsGrounded)
      {
        float groundLayerValue = GetGroundLayerValue();

        LandEvent?.Invoke(groundLayerValue);
      }
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