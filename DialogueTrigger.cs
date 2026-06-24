using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogues")]
    [SerializeField] private DialogueData dialogueBeforeTask;
    [SerializeField] private DialogueData dialogueAfterTask;

    [Header("Task State")]
    [SerializeField] private PanelTaskDefinition relatedTaskDefinition;

    [Header("Manager")]
    [SerializeField] private DialogueManager dialogueManager;

    private bool canTalk = false;

    private void Update()
    {
        if (!canTalk)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueManager != null && !dialogueManager.IsDialogueOpen)
            {
                DialogueData selectedDialogue = GetCurrentDialogue();

                if (selectedDialogue != null)
                {
                    dialogueManager.StartDialogue(selectedDialogue, transform);
                }
                else
                {
                    Debug.LogWarning("DialogueTrigger: подходящий диалог не назначен.", this);
                }
            }
        }
    }

    private DialogueData GetCurrentDialogue()
    {
        if (relatedTaskDefinition != null && relatedTaskDefinition.IsCompleted)
        {
            if (dialogueAfterTask != null)
                return dialogueAfterTask;
        }

        return dialogueBeforeTask;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canTalk = true;
            Debug.Log("Нажми E, чтобы поговорить");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canTalk = false;
        }
    }
}