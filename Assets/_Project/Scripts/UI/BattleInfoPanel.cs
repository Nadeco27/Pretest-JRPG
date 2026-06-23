using UnityEngine;
using TMPro;

public class BattleInfoPanel : MonoBehaviour
{
    public static BattleInfoPanel Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI cycleText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI locationText;

    [Header("Battle Context")]
    [SerializeField] private string encounterName = "Realm: Nightmare";

    private float battleTimer = 0f;
    private bool isTimerRunning = false;
    private int currentCycle = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (locationText != null) locationText.text = encounterName;
        UpdateCycle(1);
        SetTurnText("Player Turn", Color.cyan);
        StartTimer();
    }

    private void Update()
    {
        // Handle battle timer logic
        if (isTimerRunning)
        {
            battleTimer += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    public void SetTurnText(string text, Color textColor)
    {
        if (turnText != null)
        {
            turnText.text = text;
            turnText.color = textColor;
        }
    }

    public void AdvanceCycle()
    {
        currentCycle++;
        UpdateCycle(currentCycle);
        Debug.Log($"[Battle Info] Cycle advanced to {currentCycle}");
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    private void StartTimer()
    {
        battleTimer = 0f;
        isTimerRunning = true;
    }

    private void UpdateCycle(int cycle)
    {
        if (cycleText != null)
        {
            cycleText.text = $"Cycle: {cycle}";
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        // Calculate minutes and seconds
        int minutes = Mathf.FloorToInt(battleTimer / 60f);
        int seconds = Mathf.FloorToInt(battleTimer % 60f);

        // Format string to mm:ss
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}