using UnityEngine;

public class BattleFungusHelper : MonoBehaviour
{
    public void StartMidBattleDialogue()
    {
        if (BattleUIController.Instance != null)
        {
            BattleUIController.Instance.HideBattleUI();
        }
    }

    public void EndMidBattleDialogue()
    {
        if (BattleUIController.Instance != null)
        {
            BattleUIController.Instance.ShowBattleUI();
        }
    }
}