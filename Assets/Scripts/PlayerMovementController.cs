using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
  private Rigidbody m_physicsController = null;
  private Animator m_animator = null;

  private Vector3 m_input = Vector3.zero;

  private void Awake()
  {
    m_physicsController = GetComponent<Rigidbody>();
    m_animator = GetComponent<Animator>();

    if (m_physicsController == null || m_animator == null)
    {
      Debug.LogError("Error! Rigidbody or Animator component does not exist on player object!", this);
    }
  }

  private void Update()
  {
    // Get player input
    float h = Input.GetAxis("Horizontal");
    float v = Input.GetAxis("Vertical");
  }

  private void FixedUpdate()
  {
    
  }
}
