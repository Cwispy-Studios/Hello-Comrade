using System;
using System.Collections;
using System.Collections.Generic;
using CwispyStudios.HelloComrade.Interactions.Lighting;
using UnityEngine;

namespace CwispyStudios.HelloComrade.Interactions.Lighting
{

    public class LightMeasurer : MonoBehaviour
    {

        [SerializeField] private LightMeasurePair[] lightMeasurePairs = null;
        private Texture2D bufferTexture;
        private float avg = 0;
        private WaitForEndOfFrame endOfFrame;

        [SerializeField] private Vector2 lightRange = new Vector2(0, 1);

        private void Awake()
        {
            bufferTexture = Texture2D.grayTexture;
            endOfFrame = new WaitForEndOfFrame();
        }

        private void Update()
        {
            StartCoroutine(CalculateTransparency());
        }

        private IEnumerator CalculateTransparency()
        {
            yield return endOfFrame;



            avg = 0;
            foreach (var lightMeasurer in lightMeasurePairs)
            {
                lightMeasurer.renderCamera.Render();
                RenderTexture.active = lightMeasurer.renderTexture;
                bufferTexture.ReadPixels(new Rect(0, 0, lightMeasurer.renderTexture.width, lightMeasurer.renderTexture.height), 0, 0);
                bufferTexture.Apply();
                avg += bufferTexture.GetPixel(0, 0).grayscale;
            }

            RenderTexture.active = null;

            avg /= lightMeasurePairs.Length;
            print(avg.ToString());
        }
    }
}