using System.Collections;

using UnityEngine;

namespace CwispyStudios.HelloComrade.Player.Items
{
  using Audio;

  public class DraggedItem : Item
  {
    [SerializeField] private AudioEmitter dragEvent = null;

    // Used to keep track of how long the dragging sound has been playing
    private float dragSoundCounter = 0f;
    // Max time the dragging sound should keep playing for before it is stopped if the object is not being dragged
    private float dragSoundMaxIdlePlaytime = 0.25f;
    // Whether the dragging sound is playing
    private bool dragSoundIsPlaying = false;
    // Current intensity of the dragging sound
    private float currentDraggingIntensity = 0f;
    // Target intensity of the dragging sound
    private float targetDraggingIntensity = 0f;
    // Smoothing multiplier to lerp from current to target dragging intensity
    private float draggingIntensitySmooth = 10f;

    public override void Start()
    {
      base.Start();

      dragEvent.Initialise(transform);
    }

    private void OnCollisionStay( Collision collision )
    {
      float timeOfCollision = Time.time;

      // Collisions should not happen at the start of the game
      if (timeOfCollision <= AwakeTime + 1f) return;

      // Check if object is being moved or is moving
      if (!PhysicsController.IsSleeping())
      {
        // Check if object is on the ground while it is moving
        if (Mathf.Abs(PhysicsController.velocity.y) < 0.1f)
        {
          targetDraggingIntensity = PhysicsController.velocity.magnitude;

          if (!dragSoundIsPlaying)
          {
            dragSoundIsPlaying = true;
            StartCoroutine(WaitToStopSound());

            dragEvent.PlaySound();
          }

          else
          {
            dragSoundCounter = 0f;
          }

          //Debug.Log("Dragged: " + PhysicsController.velocity.magnitude.ToString("F5") + " " + PhysicsController.angularVelocity.magnitude.ToString("F5"));
        }
      }
    }

    private IEnumerator WaitToStopSound()
    {
      dragSoundCounter = 0f;

      while (dragSoundCounter <= dragSoundMaxIdlePlaytime)
      {
        float deltaTime = Time.deltaTime;

        dragSoundCounter += deltaTime;

        float draggingIntensitySmoothing = deltaTime * draggingIntensitySmooth;

        // Sound should reach higher intensity faster than dropping intensity
        if (targetDraggingIntensity > currentDraggingIntensity)
        {
          draggingIntensitySmoothing *= 2f;
        }

        currentDraggingIntensity = Mathf.SmoothStep(currentDraggingIntensity, targetDraggingIntensity, draggingIntensitySmoothing);
        dragEvent.SetParameter("Drag Intensity", currentDraggingIntensity);

        yield return null;
      }

      dragSoundIsPlaying = false;
      dragEvent.StopSound();
    }
  }
}
