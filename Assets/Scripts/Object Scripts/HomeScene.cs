using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeScene : MonoBehaviour
{
    public string SceneToLoad;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadSceneAfterDelay(5f));
    }

    private IEnumerator LoadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        //Wait for the seconds to go by

        SceneManager.LoadScene(SceneToLoad); //Load assigned scene
    }
}