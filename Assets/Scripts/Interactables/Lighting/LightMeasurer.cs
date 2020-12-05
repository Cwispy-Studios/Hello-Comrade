using System;
using System.Collections;
using System.Collections.Generic;
using CwispyStudios.HelloComrade.Interactions.Lighting;
using Photon.Pun;
using UnityEngine;

namespace CwispyStudios.HelloComrade.Interactions.Lighting
{

    public class LightMeasurer : MonoBehaviourPun, IPunObservable
    {

        [SerializeField] private LightMeasurePair[] lightMeasurePairs = null;
        [SerializeField] private Renderer playerMaterial = null;
        private Texture2D bufferTexture;
        private float avg = 0;
        private WaitForEndOfFrame endOfFrame;

        [SerializeField] private Vector2 lightRange = new Vector2(0.05f, .3f);
        private float lightRangeRange; // Sorry bout that

        private void Awake()
        {
            bufferTexture = Texture2D.grayTexture;
            endOfFrame = new WaitForEndOfFrame();
            lightRangeRange = lightRange.y - lightRange.x;
        }

        private void Update()
        {
            if (photonView.IsMine)
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

            if (avg < lightRange.y)
            {
                if (avg < lightRange.x)
                {
                    avg = 0;
                }
                else
                {
                    avg -= lightRange.x;
                    avg /= lightRangeRange;
                }
            }

            SetShaderTransparency();
        }
        
        private void SetShaderTransparency()
        {
            playerMaterial.material.SetFloat(4, avg);
        }
        
        public void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(avg);
            }

            else
            {
                // Network player, receive data
                avg = (float)stream.ReceiveNext();
                SetShaderTransparency();
            }
        }
    }
}