using UnityEngine;

[ExecuteInEditMode]
public class CloudsGen : MonoBehaviour
{
    public int cloudsResolution = 20;
    public float cloudsHeight;
    public Mesh cloudMesh;
    public Material cloudMaterial;
    private float _offset;
   
    private Camera _sceneCamera;
    private Matrix4x4 _cloudsPosMatrix;

    public bool shadowCasting, shadowReceive, useLightProbes;

    private void OnEnable()
    {
        _sceneCamera = Camera.main;
    }

    void Update()
    {
        var currentTransform = transform;
        
        cloudMaterial.SetFloat("CloudsWorldPos", currentTransform.position.y);
        cloudMaterial.SetFloat("CloudHeight", cloudsHeight);
       
        _offset = cloudsHeight / cloudsResolution / 2f;
        
        
        var initPos = transform.position + (Vector3.up * (_offset * cloudsResolution/2f));
        
        for (var i = 0; i < cloudsResolution; i++)
        {
            //Take into consideration translation, rotation, scale of clouds-gen object
            _cloudsPosMatrix = Matrix4x4.TRS(initPos - (Vector3.up * _offset * i), currentTransform.rotation,currentTransform.localScale);
            
            //Push mesh data to render without editor overhead of managing multiple objects
            Graphics.DrawMesh(cloudMesh, _cloudsPosMatrix, cloudMaterial, 0, _sceneCamera, 0, null, shadowCasting, shadowReceive, useLightProbes);
        }
    }
}
