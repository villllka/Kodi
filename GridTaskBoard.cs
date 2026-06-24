using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridTaskBoard : MonoBehaviour
{
    [Header("Grid Size")]
    [SerializeField] private int columns = 5;
    [SerializeField] private int rows = 5;

    [Header("Start / Goal")]
    [SerializeField] private Vector2Int startCell = new Vector2Int(0, 0);
    [SerializeField] private Vector2Int goalCell = new Vector2Int(4, 4);

    [Header("Blocked Cells")]
    [SerializeField] private List<Vector2Int> blockedCells = new List<Vector2Int>();

    [Header("Danger Cells")]
    [SerializeField] private List<Vector2Int> dangerCells = new List<Vector2Int>();

    [Header("References")]
    [SerializeField] private RectTransform boardRect;
    [SerializeField] private RectTransform goalMarkerRect;

    [Header("Debug Grid")]
    [SerializeField] private bool drawDebugGrid = true;
    [SerializeField] private Color gridLineColor = Color.black;
    [SerializeField] private float gridLineThickness = 2f;
    [SerializeField] private RectTransform debugLinesRoot;

    [Header("Danger Visuals")]
    [SerializeField] private GameObject dangerCellPrefab;
    [SerializeField] private RectTransform dangerCellsRoot;

    public int Columns => columns;
    public int Rows => rows;
    public Vector2Int StartCell => startCell;
    public Vector2Int GoalCell => goalCell;
    public RectTransform BoardRect => boardRect;

    public float CellWidth => boardRect.rect.width / columns;
    public float CellHeight => boardRect.rect.height / rows;

    private void Awake()
    {
        if (boardRect == null)
            boardRect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        RefreshBoard();
    }

    private void Start()
    {
        RefreshBoard();
    }

    public void RefreshBoard()
    {
        if (boardRect == null)
            boardRect = GetComponent<RectTransform>();

        if (drawDebugGrid)
            DrawDebugGrid();

        DrawDangerCells();
        UpdateGoalMarkerPosition();
    }

    public bool IsInsideGrid(Vector2Int cell)
    {
        return cell.x >= 0 &&
               cell.x < columns &&
               cell.y >= 0 &&
               cell.y < rows;
    }

    public bool IsBlockedCell(Vector2Int cell)
    {
        return blockedCells.Contains(cell);
    }

    public bool IsDangerCell(Vector2Int cell)
    {
        return dangerCells.Contains(cell);
    }

    public bool IsSafeCell(Vector2Int cell)
    {
        return IsInsideGrid(cell) &&
               !IsBlockedCell(cell) &&
               !IsDangerCell(cell);
    }

    public bool IsGoalCell(Vector2Int cell)
    {
        return cell == goalCell;
    }

    public Vector2 GetCellLocalPosition(Vector2Int cell)
    {
        float boardWidth = boardRect.rect.width;
        float boardHeight = boardRect.rect.height;

        float x = -boardWidth / 2f + CellWidth / 2f + cell.x * CellWidth;
        float y = boardHeight / 2f - CellHeight / 2f - cell.y * CellHeight;

        return new Vector2(x, y);
    }

    public void UpdateGoalMarkerPosition()
    {
        if (goalMarkerRect == null)
            return;

        goalMarkerRect.anchorMin = new Vector2(0.5f, 0.5f);
        goalMarkerRect.anchorMax = new Vector2(0.5f, 0.5f);
        goalMarkerRect.pivot = new Vector2(0.5f, 0.5f);
        goalMarkerRect.anchoredPosition = GetCellLocalPosition(goalCell);
    }

    [ContextMenu("Redraw Debug Grid")]
    public void DrawDebugGrid()
    {
        if (debugLinesRoot == null || boardRect == null)
            return;

        ClearRoot(debugLinesRoot);

        float boardWidth = boardRect.rect.width;
        float boardHeight = boardRect.rect.height;

        debugLinesRoot.anchorMin = new Vector2(0.5f, 0.5f);
        debugLinesRoot.anchorMax = new Vector2(0.5f, 0.5f);
        debugLinesRoot.pivot = new Vector2(0.5f, 0.5f);
        debugLinesRoot.anchoredPosition = Vector2.zero;
        debugLinesRoot.sizeDelta = new Vector2(boardWidth, boardHeight);

        for (int x = 0; x <= columns; x++)
        {
            CreateLine(
                $"VLine_{x}",
                new Vector2(-boardWidth / 2f + x * CellWidth, 0f),
                new Vector2(gridLineThickness, boardHeight)
            );
        }

        for (int y = 0; y <= rows; y++)
        {
            CreateLine(
                $"HLine_{y}",
                new Vector2(0f, boardHeight / 2f - y * CellHeight),
                new Vector2(boardWidth, gridLineThickness)
            );
        }
    }

    private void CreateLine(string name, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject line = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        line.transform.SetParent(debugLinesRoot, false);

        RectTransform lineRect = line.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.anchoredPosition = anchoredPosition;
        lineRect.sizeDelta = size;

        Image image = line.GetComponent<Image>();
        image.color = gridLineColor;
        image.raycastTarget = false;
    }

    private void DrawDangerCells()
    {
        if (dangerCellPrefab == null || dangerCellsRoot == null)
            return;

        ClearRoot(dangerCellsRoot);

        dangerCellsRoot.anchorMin = new Vector2(0.5f, 0.5f);
        dangerCellsRoot.anchorMax = new Vector2(0.5f, 0.5f);
        dangerCellsRoot.pivot = new Vector2(0.5f, 0.5f);
        dangerCellsRoot.anchoredPosition = Vector2.zero;
        dangerCellsRoot.sizeDelta = boardRect.rect.size;

        foreach (Vector2Int cell in dangerCells)
        {
            GameObject dangerCell = Instantiate(dangerCellPrefab, dangerCellsRoot);

            RectTransform rect = dangerCell.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.anchoredPosition = GetCellLocalPosition(cell);
            rect.sizeDelta = new Vector2(CellWidth, CellHeight);

            Image image = dangerCell.GetComponent<Image>();
            if (image != null)
                image.raycastTarget = false;
        }
    }

    private void ClearRoot(RectTransform root)
    {
        if (root == null)
            return;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(root.GetChild(i).gameObject);
            else
                DestroyImmediate(root.GetChild(i).gameObject);
        }
    }
}