using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenScript : MonoBehaviour
{
    [SerializeField] private Image progressBar = null;
    [SerializeField] private TextMeshProUGUI loadingText = null;
    [SerializeField] private TextMeshProUGUI progressText = null;
    [SerializeField] private TextMeshProUGUI levelText = null;
    [SerializeField] private TextMeshProUGUI descriptionText = null;
    [SerializeField] private Image backgroundDisplay = null;

    [SerializeField] private Sprite backgroundImage = null;
    [SerializeField] private string levelName = null;
    [SerializeField] [TextArea] private string levelDescription = null;
    
    private int _sceneToLoad = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        //_sceneToLoad = PlayerPrefs.GetInt("sceneToLoad");

        levelText.text = levelName;
        descriptionText.text = levelDescription;
        backgroundDisplay.sprite = backgroundImage;

        //start async operation
        StartCoroutine(LoadScene());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator LoadScene()
    {
        //create async operation
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_sceneToLoad);
        asyncOperation.allowSceneActivation = false;
        
        while (!asyncOperation.isDone)
        {
            //Output the current progress
            loadingText.text = "Loading: ";
            progressText.text = (asyncOperation.progress * 100) + "%";
            progressBar.fillAmount = asyncOperation.progress;

            // Check if the load has finished
            if (asyncOperation.progress >= 0.9f)
            {
                //Change the Text to show the Scene is ready
                progressBar.fillAmount = 1;
                loadingText.text = "Press any key to continue";
                progressText.text = "100%";
                //Wait to you press any key to activate the Scene
                if (Input.anyKey)
                    //Activate the Scene
                    asyncOperation.allowSceneActivation = true;
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
