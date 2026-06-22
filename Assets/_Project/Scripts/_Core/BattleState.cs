// Tidak perlu inherit dari MonoBehaviour
public enum BattleState
{
    START,          // Spawn character, initialize UI
    PLAYERTURN,     // Player action
    ENEMYTURN,      // Enemy action
    BUSY_CUTSCENE,  // Stop character action for dialogue cutscene
    WON,            // Player won battle
    LOST            // Player lost battle
}