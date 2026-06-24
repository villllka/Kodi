using UnityEngine;

public class PanelTaskDefinition : MonoBehaviour
{
    [Header("Task Info")]
    [SerializeField] private string taskTitle;

    [TextArea(3, 8)]
    [SerializeField] private string taskDescription;

    [Header("Task Visual")]
    [SerializeField] private Sprite taskPicture;

    [Tooltip("Необязательная анимация для условия задачи. Если назначена, она показывается вместо картинки.")]
    [SerializeField] private RuntimeAnimatorController taskAnimation;

    [SerializeField] private GameObject previewTaskPrefab;

    [Header("Algorithm Requirements")]
    [SerializeField] private bool requireConditionBlock = false;
    [SerializeField] private bool requireLoopBlock = false;

    [Header("Panel Visual")]
    [SerializeField] private Animator panelAnimator;

    [Header("World Unlock")]
    [SerializeField] private GameObject objectToDisable;
    [SerializeField] private GameObject brokenBridgeObject;
    [SerializeField] private GameObject builtBridgeObject;

    [SerializeField] private bool isCompleted = false;

    [Header("NPC Unlock")]
    [SerializeField] private Animator npcAnimator;

    public string TaskTitle => taskTitle;
    public string TaskDescription => taskDescription;
    public Sprite TaskPicture => taskPicture;
    public RuntimeAnimatorController TaskAnimation => taskAnimation;
    public GameObject PreviewTaskPrefab => previewTaskPrefab;

    public bool RequireConditionBlock => requireConditionBlock;
    public bool RequireLoopBlock => requireLoopBlock;

    public bool IsCompleted => isCompleted;

    private void Start()
    {
        RefreshWorldVisual();
        RefreshWorldObjects();
    }

    public void MarkCompleted()
    {
        isCompleted = true;
        RefreshWorldVisual();
        RefreshWorldObjects();
    }

    private void RefreshWorldVisual()
    {
        if (panelAnimator != null)
        {
            panelAnimator.SetBool("Completed", isCompleted);
        }
        if (npcAnimator != null)
        {
            npcAnimator.SetBool("Completed", isCompleted);
        }
    }

    private void RefreshWorldObjects()
    {
        if (!isCompleted)
        {
            if (brokenBridgeObject != null)
                brokenBridgeObject.SetActive(true);

            if (builtBridgeObject != null)
                builtBridgeObject.SetActive(false);
        }
        else
        {
            if (objectToDisable != null)
                objectToDisable.SetActive(false);

            if (brokenBridgeObject != null)
                brokenBridgeObject.SetActive(false);

            if (builtBridgeObject != null)
                builtBridgeObject.SetActive(true);
        }
    }
}