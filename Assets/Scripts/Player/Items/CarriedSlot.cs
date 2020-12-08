using UnityEngine;

public class CarriedSlot : MonoBehaviour
{
  private void FixedUpdate()
  {
    Joint joint = GetComponent<Joint>();
    if (joint)
    {
      Debug.Log(joint.currentForce + " " + joint.currentTorque);
    }
  }
}
