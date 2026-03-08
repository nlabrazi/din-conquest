using UnityEngine;
using UnityEngine.InputSystem;

public class NpcInteractable : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private PlayerMovement2D playerMovement;
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private NpcWander2D npcWander;
    [SerializeField]
    private string dialogueText =
        "As-salāmu ʿalaykum.\nLa recherche du savoir est une obligation.";

    private bool playerInRange = false;
    private Transform playerTransform;

    private void Update()
    {
        if (!playerInRange) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (dialogueManager == null)
            {
                Debug.LogWarning("DialogueManager non assigné sur " + gameObject.name);
                return;
            }

            if (!dialogueManager.IsDialogueOpen)
            {
                FacePlayer();

                if (npcWander != null)
                    npcWander.IsPausedExternally = true;

                if (playerMovement != null)
                    playerMovement.CanMove = false;

                dialogueManager.ShowDialogue(dialogueText);
            }
            else
            {
                dialogueManager.HideDialogue();

                if (npcWander != null)
                    npcWander.IsPausedExternally = false;

                if (playerMovement != null)
                    playerMovement.CanMove = true;
            }
        }
    }

    private void FacePlayer()
    {
        if (npcAnimator == null || playerTransform == null) return;

        Vector2 dir = playerTransform.position - transform.position;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            npcAnimator.SetFloat("MoveX", dir.x > 0 ? 1f : -1f);
            npcAnimator.SetFloat("MoveY", 0f);
        }
        else
        {
            npcAnimator.SetFloat("MoveX", 0f);
            npcAnimator.SetFloat("MoveY", dir.y > 0 ? 1f : -1f);
        }

        npcAnimator.SetFloat("Speed", 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = other.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerTransform = null;

            if (dialogueManager != null && dialogueManager.IsDialogueOpen)
                dialogueManager.HideDialogue();

            if (npcWander != null)
                npcWander.IsPausedExternally = false;

            if (playerMovement != null)
                playerMovement.CanMove = true;
        }
    }
}