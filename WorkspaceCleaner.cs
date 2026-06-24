using UnityEngine;

public class WorkspaceCleaner : MonoBehaviour
{
    [SerializeField] private Transform gridContainer;

    public void ClearWorkspace()
    {
        if (gridContainer == null)
        {
            Debug.LogError("WorkspaceCleaner: не назначен GridContainer.", this);
            return;
        }

        ItemSlot[] slots = gridContainer.GetComponentsInChildren<ItemSlot>(true);

        foreach (ItemSlot slot in slots)
        {
            slot.ClearSlot();
        }
    }
}