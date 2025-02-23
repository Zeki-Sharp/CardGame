using UnityEngine;
using ExcelDataReader;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;

// === 状态枚举 ===
public enum GameState
{
    Idle,
    Select,
    Move,
    Attack
}

// === 状态接口 ===
public interface IChessState
{
    void Enter();
    void HandleInput();
    void Exit();
}

public class ChessManager : MonoBehaviour
{
    public string filePath = "Assets/Resources/ChessPieces.xlsx"; // Excel 文件路径
    public GameObject chessPiecePrefab; // 棋子的预制体
    public ChessBoardController chessBoardController; // 引用 ChessBoardController，获取所有格子的位置信息
    public Transform chessPieceParent; // 棋子父级物体（空物体）

    private List<ChessPieceData> pieceDataList = new List<ChessPieceData>();

    public Camera cam; // 主摄像机
    [SerializeField] public ChessPiece selectedPiece = null; // 当前选中的棋子
    public string CurrentPlayerSide = "Red"; // 当前玩家的阵营，默认红方先手

    // === 状态机 ===
    private IChessState currentState;

    // === 状态切换方法 ===
    public void SetState(IChessState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }
        currentState = newState;
        currentState.Enter();
    }

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

        // 初始化状态机
        SetState(new IdleState(this));

        // 调试输出
        DebugPiecePositions();
    }

    void Update()
    {
        if (currentState != null)
        {
            currentState.HandleInput();
        }
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
                int randomIndex = Random.Range(0, availableCells.Count);
                CellView selectedCellView = availableCells[randomIndex];
                Cell selectedCell = chessBoardController.CellViewMap[selectedCellView];
                Vector2Int startPosition = selectedCell.CellPosition;

                if (chessBoardController.GetChessBoard().piecePositions[startPosition.x, startPosition.y] == null)
                {
                    GameObject pieceObj = Instantiate(chessPiecePrefab, selectedCellView.transform.position, Quaternion.identity, chessPieceParent);
                    ChessPiece piece = pieceObj.GetComponent<ChessPiece>();

                    piece.Initialize(data);
                    piece.currentPosition = startPosition;

                    chessBoardController.GetChessBoard().piecePositions[startPosition.x, startPosition.y] = piece;
                    pieceObj.name = $"{data.side}_{data.name}_{i + 1}";

                    availableCells.RemoveAt(randomIndex);
                    isPlaced = true;
                }
            }
        }
    }

    // === 获取世界坐标 ===
    public Vector3 GetCellWorldPosition(Vector2Int position)
    {
        foreach (var cellView in chessBoardController._cellViews)
        {
            Cell cell = chessBoardController.CellViewMap[cellView];
            if (cell.CellPosition == position)
            {
                return cellView.transform.position;
            }
        }
        return Vector3.zero;
    }

    // === 切换回合 ===
    public void SwitchTurn()
    {
        CurrentPlayerSide = (CurrentPlayerSide == "Red") ? "Black" : "Red";
        Debug.Log($"当前回合：{CurrentPlayerSide}");
    }

    // === 调试输出：piecePositions 和 currentPosition 状态 ===
    void DebugPiecePositions()
    {
        Debug.Log("=== ChessBoard Piece Positions ===");
        for (int x = 0; x < chessBoardController.GetChessBoard().Width; x++)
        {
            for (int y = 0; y < chessBoardController.GetChessBoard().Height; y++)
            {
                ChessPiece piece = chessBoardController.GetChessBoard().piecePositions[x, y];
                string pieceName = piece != null ? $"{piece.name} ({piece.currentPosition.x},{piece.currentPosition.y})" : "null";
                Debug.Log($"piecePositions[{x},{y}] = {pieceName}");
            }
        }

        Debug.Log("=== ChessPieces Current Positions ===");
        ChessPiece[] allPieces = chessPieceParent.GetComponentsInChildren<ChessPiece>();
        foreach (var piece in allPieces)
        {
            Debug.Log($"{piece.name} at Position: {piece.currentPosition.x},{piece.currentPosition.y}");
        }
    }
}

// === Idle 状态 ===
public class IdleState : IChessState
{
    private ChessManager manager;

    public IdleState(ChessManager manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {
        Debug.Log("进入 Idle 状态");
    }

    public void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = manager.cam.ScreenPointToRay(Input.mousePosition);
            int layerMask = LayerMask.GetMask("ChessPieceLayer");
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layerMask);

            if (hit.collider != null)
            {
                ChessPiece piece = hit.collider.GetComponent<ChessPiece>();

                // === 点击己方棋子：进入 Select 状态 ===
                if (piece != null && piece.side == manager.CurrentPlayerSide)
                {
                    manager.selectedPiece = piece;
                    manager.selectedPiece.Select();
                    manager.SetState(new SelectState(manager));
                }
            }
        }
    }

    public void Exit()
    {
        Debug.Log("退出 Idle 状态");
    }
}

/// === Select 状态 ===
public class SelectState : IChessState
{
    private ChessManager manager;

    public SelectState(ChessManager manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {
        Debug.Log("进入 Select 状态");
    }

    // === 修复：显式实现 IChessState 接口的 HandleInput 方法 ===
    void IChessState.HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = manager.cam.ScreenPointToRay(Input.mousePosition);
            int layerMask = LayerMask.GetMask("ChessPieceLayer");
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layerMask);

            if (hit.collider != null)
            {
                ChessPiece piece = hit.collider.GetComponent<ChessPiece>();

                // === 点击己方棋子：切换选中，保持 Select 状态 ===
                if (piece != null && piece.side == manager.CurrentPlayerSide)
                {
                    manager.selectedPiece.Deselect();
                    manager.selectedPiece = piece;
                    manager.selectedPiece.Select();
                    Debug.Log("切换己方棋子，保持 Select 状态");
                }
                // === 点击敌方棋子：进入 AttackState ===
                else if (piece != null && piece.side != manager.CurrentPlayerSide)
                {
                    Debug.Log($"点击敌方棋子：准备攻击 {piece.name}");
                    // 这里先用 Debug 代替 AttackState
                    Debug.Log($"AttackState（调试中）：{manager.selectedPiece.name} 准备攻击 {piece.name}");
                }
            }
            else
            {
                // === 获取点击位置并判断目标格子 ===
                Vector2 mousePosition = manager.cam.ScreenToWorldPoint(Input.mousePosition);

                // 遍历所有 CellView，找到点击的格子
                foreach (var cellView in manager.chessBoardController._cellViews)
                {
                    Cell cell = manager.chessBoardController.CellViewMap[cellView];
                    Vector2Int targetPosition = cell.CellPosition;

                    // === 使用棋子的 IsLegalMove 判断合法性 ===
                    if (manager.selectedPiece.IsLegalMove(targetPosition))
                    {
                        Debug.Log("点击空格子，进入 Move 状态");
                        manager.SetState(new MoveState(manager, targetPosition));
                        return;
                    }
                }

                // 点击空白区域，回到 Idle 状态
                Debug.Log("点击空白区域，回到 Idle 状态");
                manager.selectedPiece.Deselect();
                manager.selectedPiece = null;
                manager.SetState(new IdleState(manager));
            }
        }
    }

    // === 修复：显式实现 IChessState 接口的 Exit 方法 ===
    void IChessState.Exit()
    {
        Debug.Log("退出 Select 状态");
    }
}

// === Move 状态 ===
public class MoveState : IChessState
{
    private ChessManager manager;
    private Vector2Int targetPosition;

    public MoveState(ChessManager manager, Vector2Int targetPosition)
    {
        this.manager = manager;
        this.targetPosition = targetPosition;
    }

    public void Enter()
    {
        Debug.Log("进入 Move 状态");

        // === 调用棋子自身的合法移动判断 ===
        if (manager.selectedPiece.IsLegalMove(targetPosition))
        {
            // === 判断目标格子状态 ===
            ChessPiece targetPiece = manager.chessBoardController.GetChessBoard().piecePositions[targetPosition.x, targetPosition.y];

            // === 目标格子为空：直接移动 ===
            if (targetPiece == null)
            {
                // 更新棋盘占用状态
                manager.chessBoardController.GetChessBoard().piecePositions[manager.selectedPiece.currentPosition.x, manager.selectedPiece.currentPosition.y] = null;
                manager.chessBoardController.GetChessBoard().piecePositions[targetPosition.x, targetPosition.y] = manager.selectedPiece;

                // 更新棋子的当前位置
                manager.selectedPiece.currentPosition = targetPosition;

                // 移动棋子的物理位置
                manager.selectedPiece.transform.position = manager.GetCellWorldPosition(targetPosition);

                Debug.Log($"{manager.selectedPiece.name} 移动到 {targetPosition}");

                // 移动后，切换回合
                manager.selectedPiece.Deselect();
                manager.selectedPiece = null;
                manager.SwitchTurn();
                manager.SetState(new IdleState(manager));
            }
            // === 目标格子被对方棋子占用：切换到 AttackState ===
            else if (targetPiece.side != manager.CurrentPlayerSide)
            {
                Debug.Log($"吃子操作：{manager.selectedPiece.name} 准备攻击 {targetPiece.name}");
                // 这里先用 Debug 代替 AttackState
            }
            else
            {
                Debug.Log("目标格子被己方棋子占用，非法移动");
            }
        }
        else
        {
            Debug.Log("棋子自身规则不允许移动，保持选中状态");
        }
    }

    // === 修复：显式实现 IChessState 接口的 HandleInput 方法 ===
    void IChessState.HandleInput() { }

    // === 修复：显式实现 IChessState 接口的 Exit 方法 ===
    void IChessState.Exit()
    {
        Debug.Log("退出 Move 状态");
    }
}



