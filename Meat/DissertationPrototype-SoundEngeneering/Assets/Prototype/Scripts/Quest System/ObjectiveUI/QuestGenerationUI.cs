using System;
using TMPro;
using UnityEngine;

public class QuestGenerationUI : MonoBehaviour
{
    [SerializeField] private GameObject autoGenRoot;
    [SerializeField] private GameObject postInstructionsRoot;
    [SerializeField] private GameObject preInstructionsRoot;
    [SerializeField] private GameObject errorWarningRoot;

    [SerializeField] private TextMeshProUGUI errorWarningText;

    public void Start()
    {
        turnOnPreInstructions();
        TurnOffAll();
    }

    public void turnOnErrorWarning(String statusCode)
    {
        errorWarningText.text = "Status Code: " + statusCode;
        SetActiveSafe(errorWarningRoot, true, nameof(errorWarningRoot));
    }
    public void turnOffErrorWarning()
    {
        SetActiveSafe(errorWarningRoot, false, nameof(errorWarningRoot));
    }
    public void turnOnAutoGenerationUI()
    {
        SetActiveSafe(autoGenRoot, true, nameof(autoGenRoot));
    }

    public void turnOffAutoGenerationUI()
    {
        SetActiveSafe(autoGenRoot, false, nameof(autoGenRoot));
    }

    public void turnOnInstructions()
    {
        SetActiveSafe(postInstructionsRoot, true, nameof(postInstructionsRoot));
    }

    public void turnOffInstructions()
    {
        SetActiveSafe(postInstructionsRoot, false, nameof(postInstructionsRoot));
    }

    public void turnOnPreInstructions()
    {
        SetActiveSafe(preInstructionsRoot, true, nameof(preInstructionsRoot));
    }

    public void turnOffPreInstructions()
    {
        SetActiveSafe(preInstructionsRoot, false, nameof(preInstructionsRoot));
    }

    public void TurnOffAll()
    {
        turnOffAutoGenerationUI();
        turnOffInstructions();
        //turnOffPreInstructions();
        turnOffErrorWarning();
    }

    private void SetActiveSafe(GameObject target, bool active, string fieldName)
    {
        if (target == null)
        {
            Debug.LogWarning($"[QuestGenerationUI] {fieldName} is not assigned.");
            return;
        }

        target.SetActive(active);
    }
}
