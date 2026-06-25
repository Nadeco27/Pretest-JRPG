using UnityEngine;

public class StoryObjectState : MonoBehaviour
{
    [Tooltip("Required PlayerPrefs story key")]
    public string requiredStoryKey;
    [Tooltip("Required value for effect to trigger")]
    public int requiredValue = 1;

    [Header("Changes when requirement fulfilled")]
    [Tooltip("Disabled object")]
    public GameObject[] objectsToDisable;
    
    [Tooltip("Enabled object")]
    public GameObject[] objectsToEnable;

    [Tooltip("Disable component")]
    public Collider2D componentToDisable;

    private void Start()
    {
        // Check if story progression fulfilled
        if (PlayerPrefs.GetInt(requiredStoryKey, 0) >= requiredValue)
        {
            foreach (GameObject obj in objectsToDisable)
                if (obj != null) obj.SetActive(false);

            foreach (GameObject obj in objectsToEnable)
                if (obj != null) obj.SetActive(true);

            if (componentToDisable != null) 
                componentToDisable.enabled = false;
        }
    }
}