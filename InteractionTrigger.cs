using UnityEngine;
public class InteractionTrigger : MonoBehaviour
{
    [SerializeField] private PanelController panelController;
    [SerializeField] private PanelTaskDefinition taskDefinition;
    [Header("UI Hint")]
    [SerializeField] private GameObject interactionHint;
    private bool canOpen = false;
    private void Start()
    {
        if (interactionHint != null)
            interactionHint.SetActive(false);
    }
    private void Update()
    {
        if (!canOpen || !Input.GetKeyDown(KeyCode.E))
            return;
        if (panelController == null)
        {
            Debug.LogError("InteractionTrigger: не назначен PanelController", this);
            return;
        }
        if (panelController.IsPanelOpen)
        {
            panelController.ClosePanel();
            return;
        }
        if (taskDefinition != null && taskDefinition.IsCompleted)
        {
            Debug.Log("Это задание уже выполнено.");
            return;
        }
        panelController.OpenPanel(taskDefinition);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canOpen = true;
            if (interactionHint != null)
                interactionHint.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canOpen = false;
            if (interactionHint != null)
                interactionHint.SetActive(false);
        }
    }
}