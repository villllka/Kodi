using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    private Transform startParent;
    private Vector2 startAnchoredPosition;
    private Vector2 startSizeDelta;

    [SerializeField] private bool isPaletteBlock = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (isPaletteBlock)
        {
            GameObject clone = Instantiate(gameObject, rootCanvas.transform);
            clone.name = gameObject.name;

            DragDrop cloneDrag = clone.GetComponent<DragDrop>();
            RectTransform cloneRect = clone.GetComponent<RectTransform>();

            cloneDrag.SetPaletteBlock(false);

            cloneRect.anchorMin = rectTransform.anchorMin;
            cloneRect.anchorMax = rectTransform.anchorMax;
            cloneRect.pivot = rectTransform.pivot;
            cloneRect.sizeDelta = rectTransform.rect.size;
            cloneRect.localScale = Vector3.one;
            cloneRect.localRotation = Quaternion.identity;

            cloneDrag.PrepareForDrag(rootCanvas.transform);

            RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
            {
                cloneRect.localPosition = localPoint;
            }

            cloneRect.SetAsLastSibling();
            eventData.pointerDrag = clone;
        }
        else
        {
            PrepareForDrag(rootCanvas.transform);

            RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
            {
                rectTransform.localPosition = localPoint;
            }
        }
    }

    private void PrepareForDrag(Transform dragParent)
    {
        startParent = transform.parent;
        startAnchoredPosition = rectTransform.anchoredPosition;
        startSizeDelta = rectTransform.sizeDelta;

        canvasGroup.alpha = 0.85f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(dragParent, false);
        transform.SetAsLastSibling();

        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;

        if (rectTransform.sizeDelta == Vector2.zero)
        {
            rectTransform.sizeDelta = rectTransform.rect.size;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (rootCanvas == null)
            return;

        RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            rectTransform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == rootCanvas.transform)
        {
            if (startParent != null && startParent.GetComponent<ItemSlot>() != null)
            {
                ReturnToStart();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public void ReturnToStart()
    {
        if (startParent == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.SetParent(startParent, false);
        rectTransform.anchoredPosition = startAnchoredPosition;
        rectTransform.sizeDelta = startSizeDelta;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
    }

    public void PlaceInSlot(Transform slotTransform)
    {
        RectTransform slotRect = slotTransform.GetComponent<RectTransform>();

        transform.SetParent(slotTransform, false);

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.sizeDelta = slotRect.rect.size;
    }

    public Transform GetStartParent()
    {
        return startParent;
    }

    public void SetPaletteBlock(bool value)
    {
        isPaletteBlock = value;
    }
}