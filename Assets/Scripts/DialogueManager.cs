using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueText;

    public bool IsDialogueOpen => dialogueBox != null && dialogueBox.activeSelf;

    public void ShowDialogue(string text)
    {
        dialogueBox.SetActive(true);
        dialogueText.text = text;
    }

    public void HideDialogue()
    {
        dialogueBox.SetActive(false);
    }

    public void ToggleDialogue(string text)
    {
        if (IsDialogueOpen)
            HideDialogue();
        else
            ShowDialogue(text);
    }
}