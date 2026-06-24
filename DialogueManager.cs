using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBubble;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private bool isDialogueOpen = false;
    private Transform currentSpeaker;
    private Camera mainCamera;
    private RectTransform bubbleRect;

    public bool IsDialogueOpen => isDialogueOpen;

    private void Start()
    {
        mainCamera = Camera.main;

        if (dialogueBubble != null)
        {
            dialogueBubble.SetActive(false);
            bubbleRect = dialogueBubble.GetComponent<RectTransform>();
        }
    }

    private void Update()
    {
        if (!isDialogueOpen)
            return;

        UpdateBubblePosition();

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            ShowNextLine();
        }
    }

    public void StartDialogue(DialogueData dialogueData, Transform speakerTransform)
    {
        if (dialogueData == null || dialogueData.Lines.Count == 0)
        {
            Debug.LogWarning("DialogueManager: диалог пустой или не назначен.");
            return;
        }

        currentDialogue = dialogueData;
        currentSpeaker = speakerTransform;
        currentLineIndex = 0;
        isDialogueOpen = true;

        if (dialogueBubble != null)
            dialogueBubble.SetActive(true);

        if (characterNameText != null)
            characterNameText.text = currentDialogue.SpeakerName;

        if (dialogueText != null)
            dialogueText.text = currentDialogue.Lines[currentLineIndex];

        UpdateBubblePosition();
    }

    public void ShowNextLine()
    {
        if (currentDialogue == null)
            return;

        currentLineIndex++;

        if (currentLineIndex >= currentDialogue.Lines.Count)
        {
            EndDialogue();
            return;
        }

        if (dialogueText != null)
            dialogueText.text = currentDialogue.Lines[currentLineIndex];
    }

    public void EndDialogue()
    {
        isDialogueOpen = false;
        currentDialogue = null;
        currentSpeaker = null;
        currentLineIndex = 0;

        if (dialogueBubble != null)
            dialogueBubble.SetActive(false);
    }

    private void UpdateBubblePosition()
    {
        if (currentSpeaker == null || bubbleRect == null || mainCamera == null)
            return;

        Vector3 worldPosition = currentSpeaker.position + worldOffset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        bubbleRect.position = screenPosition;
    }
}