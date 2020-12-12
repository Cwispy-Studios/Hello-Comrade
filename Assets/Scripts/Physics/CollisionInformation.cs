using UnityEngine;

namespace CwispyStudios.HelloComrade.GamePhysics
{
  public struct CollisionInformation
  {
    public GameObject CollisionObject;
    public Vector3 Impulse;
    public float TotalImpulseAcrossAxes;

    private void AddImpulseAcrossAxes( Vector3 impulse )
    {
      TotalImpulseAcrossAxes += Mathf.Abs(impulse.x);
      TotalImpulseAcrossAxes += Mathf.Abs(impulse.y);
      TotalImpulseAcrossAxes += Mathf.Abs(impulse.z);
    }

    public CollisionInformation( GameObject collisionObject, Vector3 impulse )
    {
      CollisionObject = collisionObject;
      Impulse = Vector3.zero;
      TotalImpulseAcrossAxes = 0f;

      AddImpulse(impulse);
    }

    public void AddImpulse( Vector3 impulse )
    {
      Impulse += impulse;

      AddImpulseAcrossAxes(impulse);
    }
  }
}
