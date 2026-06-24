using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AlgorithmReader : MonoBehaviour
{
    [SerializeField] private Transform gridContainer;

    public List<CommandType> ReadCommands()
    {
        List<CommandType> commands = new List<CommandType>();

        Transform searchRoot = gridContainer != null ? gridContainer : transform;

        ItemSlot[] slots = searchRoot.GetComponentsInChildren<ItemSlot>(true);

        List<ItemSlot> sortedSlots = slots
            .OrderByDescending(slot => GetSlotPosition(slot).y)
            .ThenBy(slot => GetSlotPosition(slot).x)
            .ToList();

        foreach (ItemSlot slot in sortedSlots)
        {
            if (slot == null || slot.IsEmpty)
                continue;

            CommandBlock block = slot.CurrentBlock;

            if (block != null && block.CommandType != CommandType.None)
            {
                commands.Add(block.CommandType);
            }
        }

        Debug.Log("Алгоритм прочитан: " + string.Join(" -> ", commands));

        return commands;
    }

    public void LogCommands()
    {
        List<CommandType> commands = ReadCommands();

        if (commands.Count == 0)
        {
            Debug.Log("Алгоритм пуст.");
            return;
        }

        Debug.Log(string.Join(" -> ", commands));
    }

    private Vector2 GetSlotPosition(ItemSlot slot)
    {
        RectTransform rectTransform = slot.GetComponent<RectTransform>();

        if (rectTransform != null)
            return rectTransform.anchoredPosition;

        return slot.transform.localPosition;
    }
}