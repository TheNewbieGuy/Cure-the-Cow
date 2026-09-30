using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Required for TextMeshPro

public class DialogUI : MonoBehaviour
{
    public static DialogUI Instance;

    [Header("References")]
    [Tooltip("The root panel GameObject that gets shown/hidden.")]
    public GameObject dialogPanel;

    [Tooltip("The TextMeshPro text field the current line is written into.")]
    public TextMeshProUGUI dialogText; // Changed from Text to TextMeshProUGUI

    [Tooltip("Button the player clicks to advance to the next line.")]
    public Button nextButton;

    private List<string> currentLines;
    private int currentIndex;

    void Awake()
    {
        Instance = this;

        if (nextButton != null)
            nextButton.onClick.AddListener(Advance);

        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    public bool IsShowingDialog()
    {
        return dialogPanel != null && dialogPanel.activeSelf;
    }

    /// <summary>
    /// Shows the given lines one at a time and yields until the player has
    /// clicked through all of them (or immediately if the list is empty).
    /// </summary>
    public IEnumerator ShowDialogRoutine(List<string> lines)
    {
        if (lines == null || lines.Count == 0 || dialogPanel == null)
            yield break;

        currentLines = lines;
        currentIndex = 0;

        dialogPanel.SetActive(true);
        DisplayCurrentLine();

        while (dialogPanel.activeSelf)
        {
            yield return null;
        }
    }

    void DisplayCurrentLine()
    {
        if (dialogText != null &&
            currentLines != null &&
            currentIndex < currentLines.Count)
        {
            dialogText.text = "'" + currentLines[currentIndex] + "'";
        }
    }

    void Advance()
    {
        if (currentLines == null || !dialogPanel.activeSelf)
            return;

        currentIndex++;

        if (currentIndex >= currentLines.Count)
        {
            dialogPanel.SetActive(false);
            currentLines = null;
            return;
        }

        DisplayCurrentLine();
    }
}