using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CwispyStudios.HelloComrade.Interactions.Lighting
{
  public class PlayerMaterialPair
  {
    public readonly Transform playerTransform;
    public readonly Material playerMaterial;

    public PlayerMaterialPair(Transform playerTransform, Material playerMaterial)
    {
      this.playerTransform = playerTransform;
      this.playerMaterial = playerMaterial;
    }
  }
}