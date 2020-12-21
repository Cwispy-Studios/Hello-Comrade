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
    private const float LateAwakeSeconds = 5;

    [SerializeField] private LightMeasurePair lightMeasurePair = null;
    private Texture2D bufferTexture;
    private bool invisible = false;
    private int invisibilityValueLocation;
    private WaitForEndOfFrame endOfFrame;
    //private RaycastHit[] hits = new RaycastHit[1];
    public List<PlayerMeshColliderPair> playerMeshColliderPairs;
    [SerializeField] private PlayerMeshColliderPair thisPlayerMeshColliderPairs;

    [SerializeField] private Vector3 measurePointOffset = new Vector3();
    [SerializeField] private float invisibilityThreshold = .0006f;

    private Vector3 directionBuffer;
    private Quaternion standardRotation;

    private void Awake()
    {
      bufferTexture = Texture2D.grayTexture;
      endOfFrame = new WaitForEndOfFrame();
      playerMeshColliderPairs = new List<PlayerMeshColliderPair>();
      invisibilityValueLocation = Shader.PropertyToID("Vector1_AD4E3E33");

      if (photonView.IsMine) return;
      SendAddThisPlayer();
    }

    private void OnDestroy()
    {
      if (photonView.IsMine) return;
      SendDeleteThisPlayer();
    }

    private void Update()
    {
      if (photonView.IsMine)
        StartCoroutine(CalculateTransparency());
    }

    private IEnumerator CalculateTransparency()
    {
      yield return endOfFrame;

      foreach (var player in playerMeshColliderPairs)
      {
        // ray cast players, if something is in the way set player material at normal alpha
        // else render camera and set alpha from that
        
        directionBuffer = player.playerMeasurePoint.position - lightMeasurePair.renderCamera.transform.position;

        RaycastHit hit;
        bool hasHit = Physics.Raycast(transform.position, directionBuffer, out hit, MaxRange);
        if (hasHit)
        {
          if (hit.transform == player.playerColliderTransform)
          {
            RenderLightCamera(player.playerMeasurePoint.position);
            Debug.DrawRay(transform.position, directionBuffer, Color.green, .1f);
          }
          else
          {
            invisible = false;
            Debug.DrawRay(transform.position, directionBuffer, Color.red, .1f);
          }
        }

        //RenderLightCamera(player.playerMeasurePoint.position);

        player.playerMesh.material.SetFloat(invisibilityValueLocation, invisible ? 0.1f : 1f);
      }
    }

    private void RenderLightCamera(Vector3 measurePosition)
    {
      invisible = false;

      float avg = 0;

      // look once at top of sphere
      avg += ReturnGrayScale(measurePosition + measurePointOffset, Vector3.zero);
      // look once at sphere from player perspective
      avg += ReturnGrayScale(transform.position, measurePosition);

      avg /= 2;
      
      print(avg);
      
      invisible = avg < invisibilityThreshold;
    }

    private float ReturnGrayScale(Vector3 measurePosition, Vector3 measureDirection)
    {
      lightMeasurePair.renderCamera.transform.position = measurePosition;
      
      if (measureDirection.Equals(Vector3.zero))
        lightMeasurePair.renderCamera.transform.rotation = standardRotation;
      else
        lightMeasurePair.renderCamera.transform.LookAt(measureDirection);
      
      lightMeasurePair.renderCamera.Render();
      RenderTexture.active = lightMeasurePair.renderTexture;
      bufferTexture.ReadPixels(
        new Rect(0, 0, lightMeasurePair.renderTexture.width, lightMeasurePair.renderTexture.height), 0, 0);
      bufferTexture.Apply();

      float returnVal = bufferTexture.GetPixel(0, 0).grayscale;

      RenderTexture.active = null;
      
      return returnVal;
    }

    private void AddNewPlayer(PlayerMeshColliderPair newPair)
    {
      playerMeshColliderPairs.Add(newPair);
    }

    private void RemoveOldPlayer(PlayerMeshColliderPair newPair)
    {
      playerMeshColliderPairs.Remove(newPair);
    }
    
    private void SendAddThisPlayer()
    {
      foreach (var measurer in FindObjectsOfType<LightMeasurer>())
      {
        // Find local measurer
        if (measurer != this && measurer.photonView.IsMine)
        {
          // Add non-local measurer
          measurer.AddNewPlayer(thisPlayerMeshColliderPairs);
        }
      }
    }

    private void SendDeleteThisPlayer()
    {
      foreach (var measurer in FindObjectsOfType<LightMeasurer>())
      {
        // Find local measurer
        if (measurer != this && measurer.photonView.IsMine)
        {
          // remove non-local measurer
          measurer.RemoveOldPlayer(thisPlayerMeshColliderPairs);
        }
      }
    }
  }
}