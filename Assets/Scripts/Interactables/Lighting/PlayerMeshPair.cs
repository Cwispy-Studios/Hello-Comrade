using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CwispyStudios.HelloComrade.Interactions.Lighting
{
  [Serializable]
  public class PlayerMeshColliderPair
  {
    public Renderer playerMesh;
    public Transform playerColliderTransform;
    public Transform playerMeasurePoint;
  }
}