using System.Collections;
using UnityEngine;

public class GridExecutor : MonoBehaviour
{
    [SerializeField] private GridTaskBoard taskBoard;
    [SerializeField] private RectTransform executorRect;
    [SerializeField] private float moveDuration = 0.25f;

    private Vector2Int currentCell;
    private bool hasFailed = false;
    private string failReason = "";

    public Vector2Int CurrentCell => currentCell;
    public bool HasFailed => hasFailed;
    public string FailReason => failReason;

    private void Awake()
    {
        if (executorRect == null)
            executorRect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        ResetToStart();
    }

    public void ResetToStart()
    {
        if (taskBoard == null || executorRect == null)
            return;

        hasFailed = false;
        failReason = "";

        currentCell = taskBoard.StartCell;
        executorRect.anchoredPosition = taskBoard.GetCellLocalPosition(currentCell);
    }

    public IEnumerator MoveByCommand(CommandType command)
    {
        if (hasFailed)
            yield break;

        Vector2Int direction = GetDirection(command);

        if (direction == Vector2Int.zero)
            yield break;

        Vector2Int targetCell = currentCell + direction;

        if (!taskBoard.IsInsideGrid(targetCell))
        {
            Fail("Исполнитель вышел за границы поля.");
            yield break;
        }

        if (taskBoard.IsBlockedCell(targetCell))
        {
            Fail("Исполнитель упёрся в препятствие.");
            yield break;
        }

        Vector2 startPos = executorRect.anchoredPosition;
        Vector2 endPos = taskBoard.GetCellLocalPosition(targetCell);

        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / moveDuration);
            executorRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        executorRect.anchoredPosition = endPos;
        currentCell = targetCell;

        if (taskBoard.IsDangerCell(currentCell))
        {
            Fail("Исполнитель наступил на опасную клетку.");
        }
    }

    public bool IsOnGoal()
    {
        return taskBoard != null && taskBoard.IsGoalCell(currentCell);
    }

    public bool IsDangerInDirection(CommandType directionCommand)
    {
        if (taskBoard == null)
            return true;

        Vector2Int direction = GetDirection(directionCommand);

        if (direction == Vector2Int.zero)
            return true;

        Vector2Int targetCell = currentCell + direction;

        return !taskBoard.IsSafeCell(targetCell);
    }

    private void Fail(string reason)
    {
        hasFailed = true;
        failReason = reason;
        Debug.Log(reason);
    }

    private Vector2Int GetDirection(CommandType command)
    {
        switch (command)
        {
            case CommandType.MoveUp:
                return new Vector2Int(0, -1);

            case CommandType.MoveDown:
                return new Vector2Int(0, 1);

            case CommandType.MoveLeft:
                return new Vector2Int(-1, 0);

            case CommandType.MoveRight:
                return new Vector2Int(1, 0);

            default:
                return Vector2Int.zero;
        }
    }
}