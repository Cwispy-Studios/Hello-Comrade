using System;
using System.Collections;
using System.Collections.Generic;
using CwispyStudios.HelloComrade.Interactions.Lighting;
using CwispyStudios.HelloComrade.Player;
using Photon.Pun;
using UnityEngine;

namespace CwispyStudios.HelloComrade.Interactions.Lighting
{
  public class LightMeasurer : MonoBehaviourPun
  {
    private const float MaxRange = 20;
    private int playerMask;

    [SerializeField] private LightMeasurePair lightMeasurePair = null;
    [SerializeField] private Renderer playerMaterial = null;
    private Texture2D bufferTexture;
    private float avg = 0;
    private WaitForEndOfFrame endOfFrame;
    private RaycastHit[] hits;
    private List<PlayerMaterialPair> playerMaterialPairs;
    [SerializeField] private Transform thisPlayerTransform;
    [SerializeField] private Renderer thisPlayerMaterial;

      [SerializeField] private float invisibilityThreshold = .0006f;

    private int alphaId;

    private void Awake()
    {
      bufferTexture = Texture2D.grayTexture;
      endOfFrame = new WaitForEndOfFrame();
      alphaId = Shader.PropertyToID("Vector1_AD4E3E33");
      playerMask = LayerMask.GetMask("Player");
      playerMaterialPairs = new List<PlayerMaterialPair>();
      
      foreach (var measurer in FindObjectsOfType<LightMeasurer>())
      {
        if (measurer != this && measurer.photonView.IsMine)
          measurer.AddNewPlayer(new PlayerMaterialPair(thisPlayerTransform, thisPlayerMaterial.material));
      }

      if (photonView.IsMine) return;
      
      Destroy(this);
    }

    private void Update()
    {
      if (photonView.IsMine)
        StartCoroutine(CalculateTransparency());
    }

    private IEnumerator CalculateTransparency()
    {
      yield return endOfFrame;

      foreach (var player in playerMaterialPairs)
      {
        // ray cast players, if something is in the way set player material at normal alpha
        // else render camera and set alpha from that
        int a = Physics.RaycastNonAlloc(new Ray(transform.position, player.playerTransform.position), hits, MaxRange);
        if (a > 0)
          if (hits[0].transform == player.playerTransform)
            RenderLightCamera();
          else
            avg = 1;
        player.playerMaterial.SetFloat(alphaId, avg);
      }
    }

    private void RenderLightCamera()
    {
      avg = 0;

      lightMeasurePair.renderCamera.Render();
      RenderTexture.active = lightMeasurePair.renderTexture;
      bufferTexture.ReadPixels(
        new Rect(0, 0, lightMeasurePair.renderTexture.width, lightMeasurePair.renderTexture.height), 0, 0);
      bufferTexture.Apply();
      avg += bufferTexture.GetPixel(0, 0).grayscale;


      RenderTexture.active = null;

      avg = avg < invisibilityThreshold ? 0 : 1;
    }

    // TODO REMOVE
    private void SetShaderTransparency()
    {
      playerMaterial.material.SetFloat(alphaId, avg);
    }
    
    private void AddNewPlayer(PlayerMaterialPair newPlayer)
    {
      playerMaterialPairs.Add(newPlayer);
    }
  }
}