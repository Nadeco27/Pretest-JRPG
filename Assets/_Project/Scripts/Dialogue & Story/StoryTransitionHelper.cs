using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryTransitionHelper : MonoBehaviour
{
    // Called by Fungus when scene transitionin and need to save position
    public void MoveToSceneAndSavePosition(string targetScene)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            PlayerPositionRestorer.SavePositionForScene(currentScene, player.transform.position);
        }

        if (ScreenTransitionManager.Instance != null)
        {
            ScreenTransitionManager.Instance.SwitchSceneWithTransition(targetScene);
        }
    }

    // Function to mark story progression
    public void MarkStoryProgress(string storyKey)
    {
        PlayerPrefs.SetInt(storyKey, 1);
        PlayerPrefs.Save();
        Debug.Log($"[Story] Progress recorded: {storyKey} = 1");
    }
}