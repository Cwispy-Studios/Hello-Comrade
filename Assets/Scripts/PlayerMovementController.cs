using UnityEngine;

using Photon.Pun;

public class PlayerMovementController : MonoBehaviourPun
{
  [SerializeField] private Camera m_camera = null;
  [SerializeField] private GameObject m_neck = null;
  [SerializeField] private float m_lookSpeedMultiplier = 5f;
  [SerializeField] private float m_moveSpeed = 1.5f;

  private Rigidbody m_physicsController = null;
  private Animator m_animator = null;

  private Vector3 m_mouseInput = Vector3.zero;
  private Vector3 m_moveInput = Vector3.zero;

  // Max look angles for mouse look, sets the rotational constraints for the neck
  private const float MaxVerticalLookDegrees = 70f;
  private const float MaxHorizontalLookDegrees = 85f;

  private void Awake()
  {
    if (!photonView.IsMine && PhotonNetwork.IsConnected)
    {
      Destroy(m_camera.gameObject);
      return;
    }

    // Account for delta time
    m_lookSpeedMultiplier *= 60f;

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
      Debug.LogError("Error! Player's Rigidbody component is missing!", this);
    }

    m_animator = GetComponent<Animator>();

    if (m_animator == null)
    {
      Debug.LogError("Error! Player's Animator component is missing!", this);
    }
  }

  private void Update()
  {
    if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

    // Get player input
    float h = Input.GetAxisRaw("Horizontal");
    float v = Input.GetAxisRaw("Vertical");

    m_moveInput.x = h;
    m_moveInput.z = v;

    m_moveInput.Normalize();

    // Get multiplied mouse movements 
    float yaw = Input.GetAxis("Mouse X");
    float pitch = Input.GetAxis("Mouse Y");

    m_mouseInput.x = yaw;
    m_mouseInput.y = pitch;
  }

  private void FixedUpdate()
  {
    if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

    if (m_mouseInput != Vector3.zero)
    {
      TrackAndFollowMouseMovements();
    }

    if (m_moveInput != Vector3.zero)
    {
      MovePlayer();
    }

    else
    {
      m_animator.SetBool("Is Walking", false);
    }
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

    // Assign the relevant movements to the camera rotation
    cameraRot.x += m_mouseInput.y * m_lookSpeedMultiplier * -1f * Time.deltaTime;
    cameraRot.y += m_mouseInput.x * m_lookSpeedMultiplier * Time.deltaTime;

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

  private void MovePlayer()
  {
    //int animatorSpeed = m_moveInput.z < 0 ? -1 : 1;
    m_animator.SetFloat("Speed", m_moveInput.z);
    m_animator.SetFloat("Direction", m_moveInput.x);
    m_animator.SetBool("Is Walking", true);

    // Get rotation of camera on 2D axis and the right rotation for horizontal movement
    float cameraAngleRad = m_camera.transform.eulerAngles.y * Mathf.Deg2Rad;
    float cameraRightAngleRad = (m_camera.transform.eulerAngles.y + 90f) * Mathf.Deg2Rad;

    // Get direction vectors 
    Vector3 verticalDirectionVector = new Vector3(Mathf.Sin(cameraAngleRad), 0f, Mathf.Cos(cameraAngleRad));
    Vector3 horizontalDirectionVector = new Vector3(Mathf.Sin(cameraRightAngleRad), 0f, Mathf.Cos(cameraRightAngleRad));

    Vector3 velocity = ((verticalDirectionVector * m_moveInput.z) + (horizontalDirectionVector * m_moveInput.x))
      * m_moveSpeed * Time.deltaTime;

    m_physicsController.MovePosition(m_physicsController.position + velocity);

    // Make the player character rotate towards the direction it is moving in
    Quaternion lookRotation = Quaternion.LookRotation(verticalDirectionVector);
    m_physicsController.MoveRotation(lookRotation);

    // Reset camera and neck rotation as well
    Vector3 camRot = m_camera.transform.localEulerAngles;
    camRot.y = 0f;
    m_camera.transform.localEulerAngles = camRot;
    Vector3 neckRot = m_neck.transform.localEulerAngles;
    neckRot.x = 0f;
    m_neck.transform.localEulerAngles = neckRot;
  }
}
