using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class EnemyEncounter : MonoBehaviour
{
    [Header("Persistent Data")]
    [Tooltip("Unique enemy ID, make sure every enemy have different ID")]
    [SerializeField] private string encounterID = "Dream_Enemy_1";

    [Header("Encounter Settings")]
    [Tooltip("The exact name of the battle scene to load.")]
    [SerializeField] private string battleSceneName = "Battle Scene";
    
    [Tooltip("The exclamation mark object placed above this enemy's head.")]
    [SerializeField] private GameObject exclamationMark;
    
    [Header("Animation Settings")]
    [Tooltip("Duration of the overshoot animation.")]
    [SerializeField] private float animationDuration = 0.4f;

    [Tooltip("Maximum overshoot scale")]
    [SerializeField] private Vector3 overshootScale = new Vector3(1.1f, 1.1f, 1.1f);

    [Tooltip("Final animation scale")]
    [SerializeField] private Vector3 finalScale = Vector3.one;

    private bool hasTriggered = false;

    private void Start()
    {
        // Check if this enemy has been defeated
        if (PlayerPrefs.GetString("LastEncounterID", "") == encounterID && 
            PlayerPrefs.GetInt("DestroyLastEnemyTrigger", 0) == 1)
        {
            PlayerPrefs.SetInt("DestroyLastEnemyTrigger", 0);
            PlayerPrefs.SetString("LastEncounterID", "");
            
            Debug.Log($"[EnemyEncounter] {encounterID} sudah dikalahkan. Menghapus dari map.");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(TriggerBattleSequence());
        }
    }

    private IEnumerator TriggerBattleSequence()
    {
        // Remember enemy ID before changing scene
        PlayerPrefs.SetString("LastEncounterID", encounterID);

        PlayerStateManager.isMovementAllowed = false;

        // Force stop player physics momentum
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) 
        {
            Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
            if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
            string currentScene = SceneManager.GetActiveScene().name;
            PlayerPositionRestorer.SavePositionForScene(currentScene, playerObj.transform.position);
        }

        // Animate exclamation mark
        if (exclamationMark != null)
        {
            AudioManager.Instance.Play("EnemyTrigger");

            exclamationMark.SetActive(true);
            Vector3 startScale = Vector3.zero;

            float timeElapsed = 0f;
            
            // Scale from 0% to overshoot scale
            float phase1Duration = animationDuration * 0.7f;
            while (timeElapsed < phase1Duration)
            {
                timeElapsed += Time.deltaTime;
                exclamationMark.transform.localScale = Vector3.Lerp(startScale, overshootScale, timeElapsed / phase1Duration);
                yield return null;
            }

            // Scale back from overshoot scale to final scale 
            timeElapsed = 0f;
            float phase2Duration = animationDuration * 0.3f;
            while (timeElapsed < phase2Duration)
            {
                timeElapsed += Time.deltaTime;
                exclamationMark.transform.localScale = Vector3.Lerp(overshootScale, finalScale, timeElapsed / phase2Duration);
                yield return null;
            }

            exclamationMark.transform.localScale = finalScale;
        }

        yield return new WaitForSeconds(0.6f);

        // Screen transition
        if (ScreenTransitionManager.Instance != null)
        {
            ScreenTransitionManager.Instance.SwitchSceneWithTransition(battleSceneName);
        }
        else
        {
            Debug.LogWarning("[EnemyEncounter] Transition Manager missing! Loading scene directly.");
            SceneManager.LoadScene(battleSceneName);
        }
    }
}