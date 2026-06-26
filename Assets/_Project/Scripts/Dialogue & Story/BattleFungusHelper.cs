using UnityEngine;

public class BattleFungusHelper : MonoBehaviour
{
    public static bool isMidBattleDialogueActive { get; private set; } = false;

    public void StartMidBattleDialogue()
    {
        isMidBattleDialogueActive = true;

        if (BattleUIController.Instance != null)
        {
            BattleUIController.Instance.HideBattleUI();
        }
    }

    public void EndMidBattleDialogue()
    {
        isMidBattleDialogueActive = false;

        if (BattleUIController.Instance != null)
        {
            BattleUIController.Instance.ShowBattleUI();
        }
    }

    private void OnDestroy()
    {
        isMidBattleDialogueActive = false;
    }
}