using UnityEngine;
using UnityEngine.EventSystems;

public class BlockDelete : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool canDeleteFromWorkspaceOnly = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        if (canDeleteFromWorkspaceOnly)
        {
            ItemSlot slot = transform.parent.GetComponent<ItemSlot>();
            if (slot == null)
                return;
        }

        Destroy(gameObject);
    }
}