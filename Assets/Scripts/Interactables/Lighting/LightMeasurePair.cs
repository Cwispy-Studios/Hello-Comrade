using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CwispyStudios.HelloComrade.Interactions.Lighting
{

    [Serializable]
    public class LightMeasurePair
    {
        [SerializeField] public RenderTexture renderTexture = null;
        [SerializeField] public Camera renderCamera = null;
    }
}