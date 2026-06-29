using UnityEngine;
using Fungus;

[CommandInfo("Flow", "Load Scene Transition", "Loads a new scene automatically using the Iris Screen Transition Manager.")]
public class LoadSceneTransitionCommand : Command
{
    [Tooltip("Write the scene destination here")]
    public string targetSceneName = "";

    public override void OnEnter()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Target Scene Name empty!");
            Continue();
            return;
        }

        if (ScreenTransitionManager.Instance != null)
        {
            ScreenTransitionManager.Instance.SwitchSceneWithTransition(targetSceneName);
        }
        else
        {
            Debug.LogWarning("ScreenTransitionManager not found in scene! Using normal scene transition");
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        }

        Continue();
    }

    public override string GetSummary()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            return "Error: Scene Name Empty";
        }
        return "To -> " + targetSceneName;
    }
}