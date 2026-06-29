using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenTransitionManager : MonoBehaviour
{
    // Singleton instance for global access
    public static ScreenTransitionManager Instance { get; private set; }

    [Header("Transition Settings")]
    [SerializeField] private Material transitionMaterial;
    [SerializeField] private float transitionDuration = 1.5f;

    [Header("UI References")]
    [Tooltip("Reference to the Canvas holding the transition image.")]
    [SerializeField] private Canvas transitionCanvas;

    // Shader Graph property identifier
    private readonly int radiusPropertyID = Shader.PropertyToID("_Radius");
    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Execute Iris-In immediately for the very first scene
        TriggerIrisIn();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Assign the new scene's camera before playing the animation
        AssignCameraToCanvas();

        // Automatically play Iris-In transition whenever any new scene loads
        TriggerIrisIn();
    }

    private void AssignCameraToCanvas()
    {
        if (transitionCanvas != null)
        {
            // Automatically finds the first enabled camera tagged "MainCamera"
            transitionCanvas.worldCamera = Camera.main;
        }
    }

    public void TriggerIrisIn()
    {
        if (transitionMaterial != null)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateIrisRoutine(0f, 1f));
        }
    }

    public void SwitchSceneWithTransition(string targetSceneName)
    {
        if (isTransitioning) return;
        
        StopAllCoroutines();
        StartCoroutine(AnimateIrisOutAndLoad(targetSceneName));
    }

    private IEnumerator AnimateIrisOutAndLoad(string sceneName)
    {
        isTransitioning = true;

        // Iris-Out: Smoothly animate from clear (1) closing down to fully black (0)
        yield return StartCoroutine(AnimateIrisRoutine(1f, 0f));
        SceneManager.LoadScene(sceneName);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateMusicForScene(sceneName);
        }

        isTransitioning = false;
    }

    private IEnumerator AnimateIrisRoutine(float startRadius, float endRadius)
    {
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float currentRadius = Mathf.Lerp(startRadius, endRadius, elapsedTime / transitionDuration);
            
            transitionMaterial.SetFloat(radiusPropertyID, currentRadius);
            yield return null;
        }

        // Snap to precise target value to avoid floating-point errors
        transitionMaterial.SetFloat(radiusPropertyID, endRadius);
    }

    private void OnApplicationQuit()
    {
        if (transitionMaterial != null)
        {
            transitionMaterial.SetFloat(radiusPropertyID, 1f);
        }
    }

    public void QuitGameWithTransition()
    {
        if (isTransitioning) return;
        
        StopAllCoroutines();
        StartCoroutine(QuitRoutine());
    }

    private IEnumerator QuitRoutine()
    {
        isTransitioning = true;

        // Animate Iris-Out to black
        yield return StartCoroutine(AnimateIrisRoutine(1f, 0f));
        
        Debug.Log("[ScreenTransitionManager] Application Quit Executed.");
        Application.Quit();

        // Force stop if running inside Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}