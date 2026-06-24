using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler
{
    public bool IsEmpty => transform.childCount == 0;

    public CommandBlock CurrentBlock
    {
        get
        {
            if (transform.childCount == 0)
                return null;

            return transform.GetChild(0).GetComponent<CommandBlock>();
        }
    }

    public void ClearSlot()
    {
        if (transform.childCount == 0)
            return;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        GameObject droppedObject = eventData.pointerDrag;
        DragDrop droppedDrag = droppedObject.GetComponent<DragDrop>();

        if (droppedDrag == null)
            return;

        if (transform.childCount == 0)
        {
            droppedDrag.PlaceInSlot(transform);
            return;
        }

        if (transform.GetChild(0) == droppedObject.transform)
            return;

        Transform existingBlock = transform.GetChild(0);
        DragDrop existingDrag = existingBlock.GetComponent<DragDrop>();

        Transform oldParentOfDragged = droppedDrag.GetStartParent();

        droppedDrag.PlaceInSlot(transform);

        if (oldParentOfDragged != null && oldParentOfDragged.GetComponent<ItemSlot>() != null)
        {
            existingDrag.PlaceInSlot(oldParentOfDragged);
        }
        else
        {
            Destroy(existingBlock.gameObject);
        }
    }
}