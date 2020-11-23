using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VersionScript : MonoBehaviour
{
    public TextMeshProUGUI versionDisplay;
    
    // Start is called before the first frame update
    void Start()
    {
        versionDisplay = GameObject.Find("VersionText").GetComponent<TextMeshProUGUI>();
        if (versionDisplay != null)
        {
            versionDisplay.text = "Version: " + Application.version;
        }
        else
        {
            Debug.Log("Cant find gameobject of type TextMeshPro with name VersionText");
        }
    }
}
