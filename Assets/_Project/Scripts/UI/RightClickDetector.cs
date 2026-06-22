using UnityEngine;
using UnityEngine.EventSystems;

public class RightClickDetector : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Call the public close method from the main action menu manager
            if (BattleActionMenu.Instance != null)
            {
                BattleActionMenu.Instance.CloseCurrentSubMenu();
            }
        }
    }
}