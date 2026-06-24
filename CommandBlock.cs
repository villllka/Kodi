using UnityEngine;

public class CommandBlock : MonoBehaviour
{
    [SerializeField] private CommandType commandType = CommandType.None;

    public CommandType CommandType => commandType;
}