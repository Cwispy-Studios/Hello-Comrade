﻿using UnityEngine;

public class PlayerLookController : MonoBehaviour
{
  [SerializeField] private Camera m_camera = null;
  [SerializeField] private GameObject m_neck = null;
  [SerializeField] private float m_speedMultiplier = 2f;

  private Rigidbody m_physicsController = null;

  private const float MaxVerticalLookDegrees = 70f;
  private const float MaxHorizontalLookDegrees = 85f;

  private void Awake()
  {
    // Account for delta time
    m_speedMultiplier *= 60f;

    if (m_camera == null)
    {
      Debug.LogError("Error! Player's camera object is missing or not assigned!", this);
    }

    if (m_neck == null)
    {
      Debug.LogError("Error! Player's neck object is missing or not assigned!", this);
    }

    m_physicsController = GetComponent<Rigidbody>();

    if (m_physicsController == null)
    {
      Debug.LogError("Error! Player's rigidbody object is missing or not assigned!", this);
    }
  }

  private void FixedUpdate()
  {
    TrackAndFollowMouseMovements();
  }

  private void TrackAndFollowMouseMovements()
  {
    // Get local rotation of the camera inside body
    Vector3 cameraRot = m_camera.transform.localEulerAngles;
    // Since the x rotation (looking up and down) goes from 0-360 instead of -180-180 we have to limit it to that
    if (cameraRot.x > 180f)
    {
      cameraRot.x -= 360f;
    }

    // Since the y rotation (looking up and down) goes from 0-360 instead of -180-180 we have to limit it to that
    if (cameraRot.y > 180f)
    {
      cameraRot.y -= 360f;
    }

    // Get multiplied mouse movements 
    float yaw = m_speedMultiplier * Input.GetAxis("Mouse X") * Time.deltaTime;
    float pitch = m_speedMultiplier * Input.GetAxis("Mouse Y") * Time.deltaTime;

    // Assign the relevant movements to the camera rotation
    cameraRot.x += pitch * -1f;
    cameraRot.y += yaw;

    // Clamp looking up and down
    cameraRot.x = Mathf.Clamp(cameraRot.x, -MaxVerticalLookDegrees, MaxVerticalLookDegrees);

    m_camera.transform.localEulerAngles = cameraRot;

    // Get the neck's local rotation based on camera movement
    Vector3 neckLocalRot = new Vector3(cameraRot.y * -1f, 0f, cameraRot.x * -1f);

    // Check if neck's horizontal x rotation is above the threshold
    if (Mathf.Abs(neckLocalRot.x) > MaxHorizontalLookDegrees)
    {
      // Get the angle difference between the local rotation and max degrees
      float angleDiff = Mathf.Abs(neckLocalRot.x) - MaxHorizontalLookDegrees;

      angleDiff = neckLocalRot.x < 0f ? angleDiff * -1f : angleDiff;

      // Then clamp the rotation angle
      neckLocalRot.x = Mathf.Clamp(neckLocalRot.x, -MaxHorizontalLookDegrees, MaxHorizontalLookDegrees);

      // Rotate the body by the angle diff
      Quaternion deltaRotation = Quaternion.Euler(0f, -angleDiff, 0f);
      m_physicsController.MoveRotation(m_physicsController.rotation * deltaRotation);

      // Also clamp local camera rotation
      Vector3 localCamRot = m_camera.transform.localEulerAngles;
      if (localCamRot.y > 180f)
      {
        localCamRot.y -= 360f;
      }
      localCamRot.y = Mathf.Clamp(localCamRot.y, -MaxHorizontalLookDegrees, MaxHorizontalLookDegrees);
      m_camera.transform.localEulerAngles = localCamRot;
    }

    m_neck.transform.localEulerAngles = neckLocalRot;
  }
}
