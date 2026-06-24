using UnityEngine;

public class PlayerPositionRestorer : MonoBehaviour
{
    private void Awake()
    {
        PlayerStateManager.isMovementAllowed = true;

        // Teleport player to saved position
        if (PlayerPrefs.GetInt("HasSavedPosition", 0) == 1)
        {
            float savedX = PlayerPrefs.GetFloat("ReturnPosX");
            float savedY = PlayerPrefs.GetFloat("ReturnPosY");
            float savedZ = PlayerPrefs.GetFloat("ReturnPosZ");

            transform.position = new Vector3(savedX, savedY, savedZ);

            // Delete key to serve for new return save positon
            PlayerPrefs.DeleteKey("HasSavedPosition");
            
            Debug.Log("[PlayerPosition] Player berhasil dikembalikan ke lokasi sisa pertarungan.");
        }
    }
}