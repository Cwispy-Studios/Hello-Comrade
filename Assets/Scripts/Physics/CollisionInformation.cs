using UnityEngine;

namespace CwispyStudios.HelloComrade.GamePhysics
{
  public struct CollisionInformation
  {
    public GameObject CollisionObject;
    public Vector3 Impulse;

    public CollisionInformation( GameObject collisionObject, Vector3 impulse )
    {
      CollisionObject = collisionObject;
      Impulse = impulse;
    }

    public void AddImpulse( Vector3 impulse )
    {
      Impulse += impulse;
    }
  }
}
