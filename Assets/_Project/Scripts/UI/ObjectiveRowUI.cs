using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectiveRowUI : MonoBehaviour
{
    [Header("Row Components")]
    [SerializeField] private Image statusImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    private ObjectiveData currentData;

    // Populates the row with data upon instantiation
    public void Initialize(ObjectiveData data)
    {
        currentData = data;
        statusImage.sprite = data.emptyCheckboxSprite;
        descriptionText.text = data.description;
    }

    // Flips the visual state when the specific objective is completed
    public void MarkComplete()
    {
        statusImage.sprite = currentData.checkedCheckboxSprite;
    }
}