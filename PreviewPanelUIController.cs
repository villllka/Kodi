using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreviewPanelUIController : MonoBehaviour
{
    public enum PreviewState
    {
        Off,
        Running,
        Success,
        Error
    }

    [Header("Screen")]
    [SerializeField] private Image previewScreenBg;

    [Header("Lamp")]
    [SerializeField] private Image statusLamp;
    [SerializeField] private Sprite lampOffSprite;
    [SerializeField] private Sprite lampYellowSprite;
    [SerializeField] private Sprite lampGreenSprite;
    [SerializeField] private Sprite lampRedSprite;

    [Header("Logs")]
    [SerializeField] private TMP_Text logText;

    [Header("Task Popup")]
    [SerializeField] private GameObject taskPopup;
    [SerializeField] private TMP_Text taskTitleText;
    [SerializeField] private TMP_Text taskDescriptionText;
    [SerializeField] private Image taskPicture;
    [SerializeField] private Animator taskPictureAnimator;

    [Header("Buttons")]
    [SerializeField] private Button closePanelButton;
    [SerializeField] private Button taskButton;
    [SerializeField] private Button taskPopupCloseButton;

    private PanelController panelController;
    private bool initialized = false;

    public void Initialize(PanelController controller)
    {
        panelController = controller;

        AutoFindButtonsIfNeeded();
        AutoFindAnimatorIfNeeded();
        BindButtons();

        ResetPreview();
        HideTaskPopup();

        initialized = true;
    }

    private void Awake()
    {
        AutoFindButtonsIfNeeded();
        AutoFindAnimatorIfNeeded();
    }

    private void OnEnable()
    {
        if (!initialized)
        {
            AutoFindButtonsIfNeeded();
            AutoFindAnimatorIfNeeded();
        }
    }

    private void AutoFindAnimatorIfNeeded()
    {
        if (taskPictureAnimator == null && taskPicture != null)
        {
            taskPictureAnimator = taskPicture.GetComponent<Animator>();
        }
    }

    private void AutoFindButtonsIfNeeded()
    {
        if (closePanelButton == null)
        {
            Transform closeTransform = FindChildRecursive(transform, "Btn_Close");
            if (closeTransform != null)
                closePanelButton = closeTransform.GetComponent<Button>();
        }

        if (taskButton == null)
        {
            Transform taskTransform = FindChildRecursive(transform, "Btn_Task");
            if (taskTransform != null)
                taskButton = taskTransform.GetComponent<Button>();
        }

        if (taskPopupCloseButton == null)
        {
            Transform popupCloseTransform = FindChildRecursive(transform, "Btn_TaskClose");
            if (popupCloseTransform != null)
                taskPopupCloseButton = popupCloseTransform.GetComponent<Button>();
        }
    }

    private void BindButtons()
    {
        if (closePanelButton != null)
        {
            closePanelButton.onClick.RemoveListener(ClosePanel);
            closePanelButton.onClick.AddListener(ClosePanel);
        }

        if (taskButton != null)
        {
            taskButton.onClick.RemoveListener(ShowTaskPopup);
            taskButton.onClick.AddListener(ShowTaskPopup);
        }

        if (taskPopupCloseButton != null)
        {
            taskPopupCloseButton.onClick.RemoveListener(HideTaskPopup);
            taskPopupCloseButton.onClick.AddListener(HideTaskPopup);
        }
    }

    private void ClosePanel()
    {
        if (panelController != null)
        {
            panelController.ClosePanel();
        }
        else
        {
            Debug.LogError("PreviewPanelUIController: PanelController не назначен.");
        }
    }

    public void SetState(PreviewState state)
    {
        if (statusLamp == null)
            return;

        switch (state)
        {
            case PreviewState.Off:
                statusLamp.sprite = lampOffSprite;
                break;

            case PreviewState.Running:
                statusLamp.sprite = lampYellowSprite;
                break;

            case PreviewState.Success:
                statusLamp.sprite = lampGreenSprite;
                break;

            case PreviewState.Error:
                statusLamp.sprite = lampRedSprite;
                break;
        }
    }

    public void SetLog(string message)
    {
        if (logText != null)
            logText.text = message;
    }

    public void AppendLog(string message)
    {
        if (logText != null)
            logText.text = message;
    }

    public void ResetPreview()
    {
        SetState(PreviewState.Off);
        SetLog("Ожидание запуска...");
    }

    public void ShowTaskPopup()
    {
        if (taskPopup != null)
            taskPopup.SetActive(true);

        RestartTaskAnimation();
    }

    public void HideTaskPopup()
    {
        if (taskPopup != null)
            taskPopup.SetActive(false);
    }

    public void SetTaskData(
        string title,
        string description,
        Sprite picture,
        RuntimeAnimatorController animationController
    )
    {
        if (taskTitleText != null)
            taskTitleText.text = title;

        if (taskDescriptionText != null)
            taskDescriptionText.text = description;

        AutoFindAnimatorIfNeeded();
        ResetTaskPictureVisual();

        if (animationController != null && taskPictureAnimator != null)
        {
            ShowTaskAnimation(animationController);
        }
        else if (picture != null && taskPicture != null)
        {
            ShowTaskPicture(picture);
        }
    }

    private void ResetTaskPictureVisual()
    {
        if (taskPictureAnimator != null)
        {
            taskPictureAnimator.runtimeAnimatorController = null;
            taskPictureAnimator.enabled = false;
        }

        if (taskPicture != null)
        {
            taskPicture.sprite = null;
            taskPicture.enabled = false;
        }
    }

    private void ShowTaskAnimation(RuntimeAnimatorController animationController)
    {
        if (taskPicture == null || taskPictureAnimator == null)
            return;

        taskPicture.enabled = true;

        taskPictureAnimator.runtimeAnimatorController = animationController;
        taskPictureAnimator.enabled = true;

        if (taskPictureAnimator.gameObject.activeInHierarchy)
        {
            taskPictureAnimator.Rebind();
            taskPictureAnimator.Play(0, 0, 0f);
        }
    }

    private void ShowTaskPicture(Sprite picture)
    {
        if (taskPicture == null)
            return;

        taskPicture.sprite = picture;
        taskPicture.enabled = true;
    }

    private void RestartTaskAnimation()
    {
        if (taskPictureAnimator == null)
            return;

        if (!taskPictureAnimator.enabled)
            return;

        if (taskPictureAnimator.runtimeAnimatorController == null)
            return;

        if (!taskPictureAnimator.gameObject.activeInHierarchy)
            return;

        taskPictureAnimator.Rebind();
        taskPictureAnimator.Play(0, 0, 0f);
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