using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgorithmExecutor : MonoBehaviour
{
    [SerializeField] private AlgorithmReader algorithmReader;
    [SerializeField] private GridExecutor gridExecutor;
    [SerializeField] private PreviewPanelUIController previewUI;
    [SerializeField] private PanelController panelController;

    [SerializeField] private float delayBetweenCommands = 0.15f;
    [SerializeField] private float endStateDuration = 3f;
    [SerializeField] private int maxLoopIterations = 200;

    private Coroutine currentExecution;

    private bool isExecuting = false;
    private bool isPaused = false;
    private bool stopRequested = false;
    private bool goalReached = false;

    public bool IsExecuting => isExecuting;
    public bool IsPaused => isPaused;

    private class ConditionResult
    {
        public bool IsValid;
        public bool Value;
        public int EndIndex;
        public string ErrorMessage;
    }

    public void SetGridExecutor(GridExecutor newGridExecutor)
    {
        gridExecutor = newGridExecutor;
    }

    public void ExecuteFromReader()
    {
        if (algorithmReader == null)
        {
            Debug.LogError("AlgorithmExecutor: не назначен AlgorithmReader", this);
            return;
        }

        if (gridExecutor == null)
        {
            Debug.LogError("AlgorithmExecutor: не назначен GridExecutor", this);
            return;
        }

        if (isExecuting)
        {
            Debug.LogWarning("Алгоритм уже выполняется.");
            return;
        }

        List<CommandType> commands = algorithmReader.ReadCommands();

        if (commands == null || commands.Count == 0)
        {
            if (previewUI != null)
                previewUI.SetLog("Алгоритм пуст.");

            return;
        }

        if (!CheckTaskRequirements(commands))
            return;

        gridExecutor.ResetToStart();

        goalReached = false;
        stopRequested = false;
        isPaused = false;

        currentExecution = StartCoroutine(ExecuteAlgorithm(commands));
    }

    private bool CheckTaskRequirements(List<CommandType> commands)
    {
        PanelTaskDefinition taskDefinition = panelController != null
            ? panelController.CurrentTaskDefinition
            : null;

        if (taskDefinition == null)
            return true;

        if (taskDefinition.RequireConditionBlock && !commands.Contains(CommandType.If))
        {
            if (previewUI != null)
                previewUI.SetLog("В задании нужно использовать условие.");

            return false;
        }

        if (taskDefinition.RequireLoopBlock &&
            !commands.Contains(CommandType.While) &&
            !commands.Contains(CommandType.For))
        {
            if (previewUI != null)
                previewUI.SetLog("В задании нужно использовать цикл.");

            return false;
        }

        return true;
    }

    private IEnumerator ExecuteAlgorithm(List<CommandType> commands)
    {
        isExecuting = true;

        if (previewUI != null)
        {
            previewUI.SetState(PreviewPanelUIController.PreviewState.Running);
            previewUI.SetLog("Запуск алгоритма...");
        }

        yield return ExecuteRange(commands, 0, commands.Count - 1);

        if (gridExecutor.HasFailed)
        {
            yield return HandleFailed(gridExecutor.FailReason);
            yield break;
        }

        if (goalReached || gridExecutor.IsOnGoal())
        {
            yield return HandleSuccess();
            yield break;
        }

        yield return HandleFailed("Цель не достигнута.");
    }

    private IEnumerator ExecuteRange(List<CommandType> commands, int startIndex, int endIndex)
    {
        int index = startIndex;

        while (index <= endIndex)
        {
            if (stopRequested)
            {
                HandleStopped();
                yield break;
            }

            while (isPaused)
                yield return null;

            if (gridExecutor.IsOnGoal())
            {
                goalReached = true;
                yield break;
            }

            CommandType command = commands[index];

            if (IsMoveCommand(command))
            {
                yield return ExecuteMoveCommand(command);

                if (goalReached || gridExecutor.HasFailed)
                    yield break;

                index++;
                continue;
            }

            if (command == CommandType.If)
            {
                int endIfIndex = FindMatchingEndIf(commands, index);

                if (endIfIndex == -1)
                {
                    yield return HandleFailed("Ошибка: не найден КонецЕсли.");
                    yield break;
                }

                yield return ExecuteIfBlock(commands, index, endIfIndex);

                if (goalReached || gridExecutor.HasFailed)
                    yield break;

                index = endIfIndex + 1;
                continue;
            }

            if (command == CommandType.While || command == CommandType.For)
            {
                int endLoopIndex = FindMatchingEndLoop(commands, index);

                if (endLoopIndex == -1)
                {
                    yield return HandleFailed("Ошибка: не найден КонецЦикла.");
                    yield break;
                }

                yield return ExecuteWhileBlock(commands, index, endLoopIndex);

                if (goalReached || gridExecutor.HasFailed)
                    yield break;

                index = endLoopIndex + 1;
                continue;
            }

            index++;
        }
    }

    private IEnumerator ExecuteMoveCommand(CommandType command)
    {
        if (previewUI != null)
            previewUI.SetLog("Выполняется: " + command);

        yield return gridExecutor.MoveByCommand(command);

        if (gridExecutor.HasFailed)
            yield break;

        if (gridExecutor.IsOnGoal())
        {
            goalReached = true;
            yield break;
        }

        yield return WaitBetweenCommands();
    }

    private IEnumerator ExecuteIfBlock(List<CommandType> commands, int ifIndex, int endIfIndex)
    {
        int currentBranchStart = ifIndex;
        bool branchExecuted = false;

        while (currentBranchStart < endIfIndex)
        {
            CommandType branchCommand = commands[currentBranchStart];

            if (branchCommand == CommandType.If || branchCommand == CommandType.ElseIf)
            {
                ConditionResult condition = ReadConditionWithThen(commands, currentBranchStart + 1, endIfIndex);

                if (!condition.IsValid)
                {
                    yield return HandleFailed(condition.ErrorMessage);
                    yield break;
                }

                int bodyStart = condition.EndIndex + 1;
                int nextBranchIndex = FindNextIfBranch(commands, bodyStart, endIfIndex);
                int bodyEnd = nextBranchIndex == -1 ? endIfIndex - 1 : nextBranchIndex - 1;

                if (!branchExecuted && condition.Value)
                {
                    branchExecuted = true;

                    if (bodyStart <= bodyEnd)
                        yield return ExecuteRange(commands, bodyStart, bodyEnd);

                    yield break;
                }

                if (nextBranchIndex == -1)
                    yield break;

                currentBranchStart = nextBranchIndex;
                continue;
            }

            if (branchCommand == CommandType.Else)
            {
                int bodyStart = currentBranchStart + 1;
                int bodyEnd = endIfIndex - 1;

                if (!branchExecuted && bodyStart <= bodyEnd)
                    yield return ExecuteRange(commands, bodyStart, bodyEnd);

                yield break;
            }

            currentBranchStart++;
        }
    }

    private IEnumerator ExecuteWhileBlock(List<CommandType> commands, int whileIndex, int endLoopIndex)
    {
        int iteration = 0;

        while (true)
        {
            if (stopRequested)
            {
                HandleStopped();
                yield break;
            }

            while (isPaused)
                yield return null;

            if (gridExecutor.IsOnGoal())
            {
                goalReached = true;
                yield break;
            }

            ConditionResult condition = ReadLoopCondition(commands, whileIndex + 1, endLoopIndex);

            if (!condition.IsValid)
            {
                yield return HandleFailed(condition.ErrorMessage);
                yield break;
            }

            if (!condition.Value)
                break;

            int bodyStart = condition.EndIndex + 1;
            int bodyEnd = endLoopIndex - 1;

            if (bodyStart <= bodyEnd)
                yield return ExecuteRange(commands, bodyStart, bodyEnd);

            if (goalReached || gridExecutor.HasFailed)
                yield break;

            iteration++;

            if (iteration >= maxLoopIterations)
            {
                yield return HandleFailed("Цикл выполнялся слишком долго.");
                yield break;
            }
        }
    }

    private ConditionResult ReadConditionWithThen(List<CommandType> commands, int startIndex, int limitIndex)
    {
        int thenIndex = -1;

        for (int i = startIndex; i < limitIndex; i++)
        {
            if (commands[i] == CommandType.Then)
            {
                thenIndex = i;
                break;
            }
        }

        if (thenIndex == -1)
        {
            return new ConditionResult
            {
                IsValid = false,
                Value = false,
                EndIndex = startIndex,
                ErrorMessage = "Ошибка: в условии не найден блок То."
            };
        }

        ConditionResult condition = ReadCondition(commands, startIndex, thenIndex - 1);
        condition.EndIndex = thenIndex;
        return condition;
    }

    private ConditionResult ReadLoopCondition(List<CommandType> commands, int startIndex, int endLoopIndex)
    {
        return ReadCondition(commands, startIndex, endLoopIndex - 1);
    }

    private ConditionResult ReadCondition(List<CommandType> commands, int startIndex, int endIndex)
    {
        if (startIndex > endIndex || startIndex >= commands.Count)
        {
            return new ConditionResult
            {
                IsValid = false,
                Value = false,
                EndIndex = startIndex,
                ErrorMessage = "Ошибка: условие пустое."
            };
        }

        CommandType first = commands[startIndex];

        if (first == CommandType.True)
        {
            return new ConditionResult
            {
                IsValid = true,
                Value = true,
                EndIndex = startIndex
            };
        }

        if (first == CommandType.False)
        {
            return new ConditionResult
            {
                IsValid = true,
                Value = false,
                EndIndex = startIndex
            };
        }

        if (startIndex + 2 <= endIndex &&
            IsMoveCommand(first) &&
            commands[startIndex + 1] == CommandType.Equal &&
            commands[startIndex + 2] == CommandType.Danger)
        {
            return new ConditionResult
            {
                IsValid = true,
                Value = gridExecutor.IsDangerInDirection(first),
                EndIndex = startIndex + 2
            };
        }

        if (startIndex + 3 <= endIndex &&
            IsMoveCommand(first) &&
            commands[startIndex + 1] == CommandType.Not &&
            commands[startIndex + 2] == CommandType.Equal &&
            commands[startIndex + 3] == CommandType.Danger)
        {
            return new ConditionResult
            {
                IsValid = true,
                Value = !gridExecutor.IsDangerInDirection(first),
                EndIndex = startIndex + 3
            };
        }

        return new ConditionResult
        {
            IsValid = false,
            Value = false,
            EndIndex = startIndex,
            ErrorMessage = "Ошибка: условие составлено неверно."
        };
    }

    private int FindNextIfBranch(List<CommandType> commands, int startIndex, int endIfIndex)
    {
        int ifDepth = 0;
        int loopDepth = 0;

        for (int i = startIndex; i < endIfIndex; i++)
        {
            if (commands[i] == CommandType.If)
                ifDepth++;

            if (commands[i] == CommandType.EndIf)
                ifDepth--;

            if (commands[i] == CommandType.While || commands[i] == CommandType.For)
                loopDepth++;

            if (commands[i] == CommandType.EndLoop)
                loopDepth--;

            if (ifDepth == 0 && loopDepth == 0 &&
                (commands[i] == CommandType.ElseIf || commands[i] == CommandType.Else))
            {
                return i;
            }
        }

        return -1;
    }

    private int FindMatchingEndIf(List<CommandType> commands, int ifIndex)
    {
        int depth = 0;

        for (int i = ifIndex; i < commands.Count; i++)
        {
            if (commands[i] == CommandType.If)
                depth++;

            if (commands[i] == CommandType.EndIf)
            {
                depth--;

                if (depth == 0)
                    return i;
            }
        }

        return -1;
    }

    private int FindMatchingEndLoop(List<CommandType> commands, int loopIndex)
    {
        int depth = 0;

        for (int i = loopIndex; i < commands.Count; i++)
        {
            if (commands[i] == CommandType.While || commands[i] == CommandType.For)
                depth++;

            if (commands[i] == CommandType.EndLoop)
            {
                depth--;

                if (depth == 0)
                    return i;
            }
        }

        return -1;
    }

    private bool IsMoveCommand(CommandType command)
    {
        return command == CommandType.MoveUp ||
               command == CommandType.MoveDown ||
               command == CommandType.MoveLeft ||
               command == CommandType.MoveRight;
    }

    private IEnumerator WaitBetweenCommands()
    {
        float timer = 0f;

        while (timer < delayBetweenCommands)
        {
            if (stopRequested)
            {
                HandleStopped();
                yield break;
            }

            while (isPaused)
                yield return null;

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator HandleSuccess()
    {
        if (previewUI != null)
        {
            previewUI.SetState(PreviewPanelUIController.PreviewState.Success);
            previewUI.SetLog("Цель достигнута.");
        }

        yield return new WaitForSeconds(endStateDuration);

        if (panelController != null)
            panelController.CompleteCurrentTaskAndClose();

        ResetExecutionState();
    }

    private IEnumerator HandleFailed(string message)
    {
        Debug.Log(message);

        if (previewUI != null)
        {
            previewUI.SetState(PreviewPanelUIController.PreviewState.Error);
            previewUI.SetLog(message);
        }

        yield return new WaitForSeconds(endStateDuration);

        if (gridExecutor != null)
            gridExecutor.ResetToStart();

        if (previewUI != null)
            previewUI.ResetPreview();

        ResetExecutionState();
    }

    public void PauseExecution()
    {
        if (!isExecuting)
        {
            Debug.Log("Нечего ставить на паузу.");
            return;
        }

        isPaused = !isPaused;

        if (previewUI != null)
            previewUI.SetLog(isPaused ? "Пауза." : "Продолжение.");
    }

    public void StopExecution()
    {
        if (!isExecuting)
        {
            Debug.Log("Нет активного алгоритма для остановки.");
            return;
        }

        stopRequested = true;
        isPaused = false;
    }

    private void HandleStopped()
    {
        if (previewUI != null)
        {
            previewUI.SetLog("Алгоритм остановлен.");
            previewUI.ResetPreview();
        }

        if (gridExecutor != null)
            gridExecutor.ResetToStart();

        ResetExecutionState();
    }

    private void ResetExecutionState()
    {
        if (currentExecution != null)
        {
            StopCoroutine(currentExecution);
            currentExecution = null;
        }

        isExecuting = false;
        isPaused = false;
        stopRequested = false;
        goalReached = false;
    }
}