using UnityEngine;
using ExcelDataReader;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class ChessManager : MonoBehaviour
{
    public string filePath = "Assets/Resources/ChessPieces.xlsx"; // Excel 文件路径
    public GameObject chessPiecePrefab; // 棋子的预制体
    public ChessBoardController chessBoardController; // 引用 ChessBoardController，获取所有格子的位置信息
    public Transform chessPieceParent; // 棋子父级物体（空物体）

    private List<ChessPieceData> pieceDataList = new List<ChessPieceData>();

    public Camera cam; // 主摄像机
    [SerializeField] private ChessPiece selectedPiece = null; // 当前选中的棋子

    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
        // 读取 Excel 数据
        LoadChessPieceData(filePath);

        // 随机打乱棋子配置
        ShufflePieces();

        // 随机分配棋子位置
        InitializeChessPieces();

    }

    void Update()
    {
        HandleSelection(); // 保留 HandleSelection 方法，确保点击选择功能不受影响
    }

    // 读取 Excel 文件并加载棋子数据
    void LoadChessPieceData(string filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath);

        using (var stream = fileInfo.Open(FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                do
                {
                    while (reader.Read()) // 读取每一行
                    {
                        // 跳过表头
                        if (reader.Depth == 0) continue;

                        int id = int.Parse(reader.GetValue(0).ToString());
                        string side = reader.GetValue(1).ToString();
                        string name = reader.GetValue(2).ToString();
                        int attack = int.Parse(reader.GetValue(3).ToString());
                        int health = int.Parse(reader.GetValue(4).ToString());
                        string specialAbility = reader.GetValue(5)?.ToString();

                        ChessPieceData data = new ChessPieceData(id, side, name, attack, health, specialAbility);
                        pieceDataList.Add(data);
                    }
                } while (reader.NextResult());
            }
        }
    }

    // 随机打乱棋子配置
    void ShufflePieces()
    {
        pieceDataList = pieceDataList.OrderBy(a => Random.Range(0f, 1f)).ToList();
    }

    // 随机分配棋子位置
    void InitializeChessPieces()
    {
        List<CellView> availableCells = new List<CellView>();

        // 将所有格子的位置添加到 availableCells 列表
        foreach (var cellView in chessBoardController._cellViews)
        {
            availableCells.Add(cellView); // 获取每个 CellView
        }

        // 随机为棋子分配位置
        for (int i = 0; i < pieceDataList.Count; i++)
        {
            ChessPieceData data = pieceDataList[i];
            bool isPlaced = false;

            while (!isPlaced)
            {
                // 随机选择一个 CellView
                int randomIndex = Random.Range(0, availableCells.Count);
                CellView selectedCellView = availableCells[randomIndex];

                // === 通过 CellViewMap 获取 CellPosition ===
                Cell selectedCell = chessBoardController.CellViewMap[selectedCellView];
                Vector2Int startPosition = selectedCell.CellPosition;
                // ========================================

                // === 使用 GetChessBoard() 间接访问 piecePositions ===
                if (chessBoardController.GetChessBoard().piecePositions[startPosition.x, startPosition.y] == null)
                {
                    // 位置未被占用，则实例化棋子
                    GameObject pieceObj = Instantiate(chessPiecePrefab, selectedCellView.transform.position, Quaternion.identity, chessPieceParent);
                    ChessPiece piece = pieceObj.GetComponent<ChessPiece>();

                    // 初始化棋子并传入初始位置
                    piece.Initialize(data);
                    piece.currentPosition = startPosition;

                    // 更新棋盘占用状态
                    chessBoardController.GetChessBoard().piecePositions[startPosition.x, startPosition.y] = piece;

                    // 设置棋子的名字
                    pieceObj.name = $"{data.side}_{data.name}_{i + 1}";

                    // 移除已占用的位置
                    availableCells.RemoveAt(randomIndex);

                    // 标记棋子已成功放置
                    isPlaced = true;
                }
                else
                {
                    // 如果已被占用，重新随机选择
                    Debug.LogWarning($"Position {startPosition} already occupied. Re-selecting...");
                }
            }
        }
    }

    void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse Clicked");

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            int layerMask = LayerMask.GetMask("ChessPieceLayer");
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layerMask);

            if (hit.collider != null)
            {
                Debug.Log($"Hit: {hit.collider.name}");
                ChessPiece piece = hit.collider.GetComponent<ChessPiece>();

                if (piece != null && piece != selectedPiece)
                {
                    if (selectedPiece != null)
                    {
                        selectedPiece.Deselect();
                    }

                    selectedPiece = piece;
                    selectedPiece.Select();
                }
                else if (piece == selectedPiece)
                {
                    selectedPiece.Deselect();
                    selectedPiece = null;
                }
            }
            else
            {
                Debug.Log("No Hit");
                if (selectedPiece != null)
                {
                    selectedPiece.Deselect();
                    selectedPiece = null;
                }
            }
        }
    }

}
