using UnityEngine;

public class PanelController : MonoBehaviour
{
    [SerializeField] private GameObject panelUI;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PreviewPanelUIController previewPanelUIController;
    [SerializeField] private WorkspaceCleaner workspaceCleaner;
    [SerializeField] private Transform previewContentRoot;
    [SerializeField] private AlgorithmExecutor algorithmExecutor;

    private bool isPanelOpen = false;
    private Rigidbody2D playerRb;
    private PanelTaskDefinition currentTaskDefinition;
    private GameObject currentPreviewTask;

    public bool IsPanelOpen => isPanelOpen;
    public PanelTaskDefinition CurrentTaskDefinition => currentTaskDefinition;

    private void Start()
    {
        if (panelUI == null)
        {
            Debug.LogError("PanelController: не назначен Panel UI", this);
            return;
        }

        panelUI.SetActive(false);

        if (playerMovement == null)
        {
            Debug.LogError("PanelController: не назначен PlayerMovement", this);
            return;
        }

        playerRb = playerMovement.GetComponent<Rigidbody2D>();

        if (previewPanelUIController == null)
            previewPanelUIController = panelUI.GetComponentInChildren<PreviewPanelUIController>(true);

        if (workspaceCleaner == null)
            workspaceCleaner = panelUI.GetComponentInChildren<WorkspaceCleaner>(true);

        if (algorithmExecutor == null)
            algorithmExecutor = panelUI.GetComponentInChildren<AlgorithmExecutor>(true);

        if (previewContentRoot == null)
        {
            Transform foundPreviewContent = FindChildRecursive(panelUI.transform, "PreviewContent");

            if (foundPreviewContent != null)
                previewContentRoot = foundPreviewContent;
        }

        if (previewPanelUIController != null)
            previewPanelUIController.Initialize(this);
        else
            Debug.LogError("PanelController: не найден PreviewPanelUIController", this);
    }

    public void OpenPanel(PanelTaskDefinition taskDefinition)
    {
        if (panelUI == null || playerMovement == null)
            return;

        currentTaskDefinition = taskDefinition;
        isPanelOpen = true;

        panelUI.SetActive(true);

        if (workspaceCleaner != null)
            workspaceCleaner.ClearWorkspace();

        LoadPreviewTask(taskDefinition);

        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        playerMovement.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (previewPanelUIController != null)
        {
            previewPanelUIController.ResetPreview();

            if (taskDefinition != null)
            {
                previewPanelUIController.SetTaskData(
                    taskDefinition.TaskTitle,
                    taskDefinition.TaskDescription,
                    taskDefinition.TaskPicture,
                    taskDefinition.TaskAnimation
                );
            }

            previewPanelUIController.ShowTaskPopup();
        }
    }

    private void LoadPreviewTask(PanelTaskDefinition taskDefinition)
    {
        ClearPreviewContent();

        if (taskDefinition == null)
        {
            Debug.LogError("PanelController: taskDefinition не назначен.");
            return;
        }

        if (taskDefinition.PreviewTaskPrefab == null)
        {
            Debug.LogError("PanelController: у задачи не назначен PreviewTaskPrefab.", taskDefinition);
            return;
        }

        if (previewContentRoot == null)
        {
            Debug.LogError("PanelController: не назначен PreviewContentRoot.", this);
            return;
        }

        currentPreviewTask = Instantiate(taskDefinition.PreviewTaskPrefab, previewContentRoot);
        currentPreviewTask.transform.localScale = Vector3.one;
        currentPreviewTask.transform.localRotation = Quaternion.identity;

        RectTransform previewRect = currentPreviewTask.GetComponent<RectTransform>();

        if (previewRect != null)
        {
            previewRect.anchorMin = Vector2.zero;
            previewRect.anchorMax = Vector2.one;
            previewRect.offsetMin = Vector2.zero;
            previewRect.offsetMax = Vector2.zero;
            previewRect.anchoredPosition = Vector2.zero;
        }

        GridExecutor newGridExecutor = currentPreviewTask.GetComponentInChildren<GridExecutor>(true);

        if (algorithmExecutor != null)
        {
            algorithmExecutor.SetGridExecutor(newGridExecutor);
        }
        else
        {
            Debug.LogError("PanelController: не назначен AlgorithmExecutor.", this);
        }
    }

    private void ClearPreviewContent()
    {
        if (previewContentRoot == null)
            return;

        for (int i = previewContentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(previewContentRoot.GetChild(i).gameObject);
        }

        currentPreviewTask = null;

        if (algorithmExecutor != null)
            algorithmExecutor.SetGridExecutor(null);
    }

    public void ClosePanel()
    {
        if (!isPanelOpen)
            return;

        isPanelOpen = false;

        if (previewPanelUIController != null)
        {
            previewPanelUIController.HideTaskPopup();
            previewPanelUIController.ResetPreview();
        }

        ClearPreviewContent();

        panelUI.SetActive(false);

        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        playerMovement.enabled = true;
        Cursor.visible = false;
    }

    public void TogglePanel()
    {
        if (isPanelOpen)
            ClosePanel();
    }

    public void CompleteCurrentTaskAndClose()
    {
        if (currentTaskDefinition != null && !currentTaskDefinition.IsCompleted)
            currentTaskDefinition.MarkCompleted();

        ClosePanel();
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindChildRecursive(child, childName);

            if (result != null)
                return result;
        }

        return null;
    }
}