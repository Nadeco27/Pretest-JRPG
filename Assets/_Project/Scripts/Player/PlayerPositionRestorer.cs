using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPositionRestorer : MonoBehaviour
{
    private void Awake()
    {
        PlayerStateManager.isMovementAllowed = true;

        string currentScene = SceneManager.GetActiveScene().name;
        string saveKey = "HasSavedPosition_" + currentScene;

        // Check if there is any saved position in said scene
        if (PlayerPrefs.GetInt(saveKey, 0) == 1)
        {
            float savedX = PlayerPrefs.GetFloat("ReturnPosX_" + currentScene);
            float savedY = PlayerPrefs.GetFloat("ReturnPosY_" + currentScene);
            float savedZ = PlayerPrefs.GetFloat("ReturnPosZ_" + currentScene);

            transform.position = new Vector3(savedX, savedY, savedZ);

            // Delete key after returning to saved position
            PlayerPrefs.DeleteKey(saveKey);
            
            Debug.Log($"[PlayerPosition] Player Moved to saved position in {currentScene}.");
        }
    }

    public static void SavePositionForScene(string sceneName, Vector3 position)
    {
        PlayerPrefs.SetFloat("ReturnPosX_" + sceneName, position.x);
        PlayerPrefs.SetFloat("ReturnPosY_" + sceneName, position.y);
        PlayerPrefs.SetFloat("ReturnPosZ_" + sceneName, position.z);
        PlayerPrefs.SetInt("HasSavedPosition_" + sceneName, 1);
        PlayerPrefs.Save();
        
        Debug.Log($"[PlayerPosition] Player positon saved in: {sceneName}");
    }
}