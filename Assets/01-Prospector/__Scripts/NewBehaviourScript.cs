using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void LoadScene(string __Prospector_Scene_0)
    {
        SceneManager.LoadScene(__Prospector_Scene_0);
    }
}