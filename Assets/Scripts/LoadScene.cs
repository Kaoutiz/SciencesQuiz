using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class LoadScene : MonoBehaviour
{
    public string scene;
    public void OnClicked()
    {
        SceneManager.LoadScene(scene);
    }  
}
