using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartHandler : MonoBehaviour
{

    public void SceneLoad(string sceneName)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Exit the game. In the Editor this stops play mode; in a build it quits the application.
    /// Call this from UI button or any exit logic.
    /// </summary>
    public void ExitGame()
    {
#if UNITY_EDITOR
        // Stop play mode when running inside the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the application in builds
        Application.Quit();
#endif
    }
}
