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
    }

    void Update()
    {
        if (currentState != null)
        {
            currentState.HandleInput();
        }
    }

    // === 集中 Raycast 检测 ===
    public object GetClickedObject()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        int layerMask = LayerMask.GetMask("ChessPieceLayer");
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, layerMask);

        if (hit.collider != null)
        {
            ChessPiece piece = hit.collider.GetComponent<ChessPiece>();
            if (piece != null)
            {
                return piece;
            }
        }

        Vector2 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        foreach (var cellView in chessBoardController._cellViews)
        {
            Cell cell = chessBoardController.CellViewMap[cellView];
            if (Vector2.Distance(cellView.transform.position, mousePosition) < 0.5f)
            {
                return cell;
            }
        }

        return null;
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

    // === 读取 Excel 数据 ===
    void LoadChessPieceData(string filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath);

        using (var stream = fileInfo.Open(FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                do
                {
                    while (reader.Read())
                    {
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

    // === 随机打乱棋子配置 ===
    void ShufflePieces()
    {
        pieceDataList = pieceDataList.OrderBy(a => Random.Range(0f, 1f)).ToList();
    }

    // === 初始化棋子位置 ===
    void InitializeChessPieces()
    {
        List<CellView> availableCells = new List<CellView>();
        List<ChessPiece> redPieces = new List<ChessPiece>();
        List<ChessPiece> blackPieces = new List<ChessPiece>();

        // === 初始化棋子并默认背面朝上 ===
        foreach (var cellView in chessBoardController._cellViews)
        {
            availableCells.Add(cellView);
        }

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

                    // === 默认背面朝上且不可选中 ===
                    piece.ShowBack();

                    // === 根据阵营分类 ===
                    if (piece.side == "Red")
                    {
                        redPieces.Add(piece);
                    }
                    else if (piece.side == "Black")
                    {
                        blackPieces.Add(piece);
                    }

                    availableCells.RemoveAt(randomIndex);
                    isPlaced = true;
                }
            }
        }

        // === 随机翻面两张牌并设置为可选中 ===
        if (redPieces.Count > 0)
        {
            int randomRedIndex = Random.Range(0, redPieces.Count);
            redPieces[randomRedIndex].ShowFront();
        }

        if (blackPieces.Count > 0)
        {
            int randomBlackIndex = Random.Range(0, blackPieces.Count);
            blackPieces[randomBlackIndex].ShowFront();
        }
    }

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
                object clickedObject = manager.GetClickedObject();

                // === 先判断类型，再判断 canBeSelected ===
                if (clickedObject is ChessPiece)
                {
                    ChessPiece clickedPiece = (ChessPiece)clickedObject;

                    // === 新增 canBeSelected 判断 ===
                    if (!clickedPiece.canBeSelected)
                    {
                        Debug.Log($"{clickedPiece.gameObject.name} 不可被选中，状态机不切换");
                        return;  // 背面朝上时直接返回，不进入 Select 状态
                    }

                    // 如果可以被选中且是当前回合的棋子
                    if (clickedPiece.side == manager.CurrentPlayerSide)
                    {
                        manager.selectedPiece = clickedPiece;
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

        public void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                object clickedObject = manager.GetClickedObject();

                // === 点击的是棋子 ===
                if (clickedObject is ChessPiece clickedPiece)
                {
                    // === 如果点击的是己方正面卡牌 ===
                    if (clickedPiece.side == manager.CurrentPlayerSide && clickedPiece.canBeSelected)
                    {
                        // === 如果点击的是当前已选中的棋子，保持选中状态，不切换 ===
                        if (clickedPiece == manager.selectedPiece)
                        {
                            return;
                        }

                        // === 新增：攻击范围判断 ===
                        if (manager.selectedPiece.CanAttackTarget(clickedPiece))
                        {
                            // === 如果在攻击范围内，进入 AttackState 并准备攻击 ===
                            manager.SetState(new AttackState(manager, clickedPiece));
                            return;
                        }
                        else
                        {
                            // === 如果不在攻击范围内，直接切换选中，不返回 SelectState，不结束回合 ===
                            manager.selectedPiece.Deselect();
                            manager.selectedPiece = clickedPiece;
                            manager.selectedPiece.Select();
                            return;
                        }
                    }
                    else
                    {
                        // === 如果点击的是敌方棋子，进入 AttackState 并准备攻击 ===
                        manager.SetState(new AttackState(manager, clickedPiece));
                        return;
                    }
                }

                // === 点击的是空格子 ===
                else if (clickedObject is Cell cell)
                {
                    // === 进入 MoveState 并更新位置 ===
                    manager.SetState(new MoveState(manager, cell.CellPosition));
                }

                // === 点击的是空白区域 ===
                else
                {
                    // === 切换到 IdleState 并取消选中 ===
                    manager.selectedPiece.Deselect();
                    manager.selectedPiece = null;
                    manager.SetState(new IdleState(manager));
                }
            }
        }

        public void Exit()
        {
            Debug.Log("退出 Select 状态");
        }
    }


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

            // === 统一更新棋盘状态和棋子位置 ===

            // === 1. 将攻击方的当前位置设置为空 ===
            manager.chessBoardController.GetChessBoard().piecePositions[manager.selectedPiece.currentPosition.x, manager.selectedPiece.currentPosition.y] = null;

            // === 2. 将目标位置设置为攻击方棋子 ===
            manager.chessBoardController.GetChessBoard().piecePositions[targetPosition.x, targetPosition.y] = manager.selectedPiece;

            // === 3. 更新 selectedPiece 的 currentPosition 和 transform.position ===
            manager.selectedPiece.currentPosition = targetPosition;
            manager.selectedPiece.transform.position = manager.GetCellWorldPosition(targetPosition);

            Debug.Log($"{manager.selectedPiece.name} 移动到 {targetPosition}");

            // === 4. 移动完成后，切换到 IdleState 并结束回合 ===
            manager.selectedPiece.Deselect();
            manager.selectedPiece = null;
            manager.SwitchTurn();
            manager.SetState(new IdleState(manager));
        }

        // === 修复：显式实现 IChessState 接口的 HandleInput 方法 ===
        void IChessState.HandleInput() { }

        // === 修复：显式实现 IChessState 接口的 Exit 方法 ===
        void IChessState.Exit()
        {
            Debug.Log("退出 Move 状态");
        }
    }



    public class AttackState : IChessState
    {
        private ChessManager manager;
        private ChessPiece targetPiece;

        public AttackState(ChessManager manager, ChessPiece targetPiece)
        {
            this.manager = manager;
            this.targetPiece = targetPiece;
        }

        public void Enter()
        {
            Debug.Log("进入 Attack 状态");

            // === 新增：攻击范围判断 ===
            if (!manager.selectedPiece.CanAttackTarget(targetPiece))
            {
                Debug.Log("攻击目标不在攻击范围内，返回 SelectState");
                manager.SetState(new SelectState(manager));
                return;
            }

            // === 攻击目标为背面卡牌 ===
            if (!targetPiece.canBeSelected)
            {
                // === 无论敌我关系，都翻面并变为可选中 ===
                targetPiece.ShowFront();
                Debug.Log($"{targetPiece.name} 被翻面并变为可选中");

                // === 如果是敌方背面卡牌，翻面后直接销毁，并进入 MoveState 更新位置 ===
                if (targetPiece.side != manager.selectedPiece.side)
                {
                    Debug.Log($"{targetPiece.name} 是敌方背面卡牌，翻面后被销毁");
                    Object.Destroy(targetPiece.gameObject);

                    // === 进入 MoveState 并更新位置 ===
                    manager.SetState(new MoveState(manager, targetPiece.currentPosition));
                    return;
                }
                else
                {
                    // === 如果是己方背面卡牌，翻面后不销毁，仅变为可选中，保持在原地 ===
                    Debug.Log($"{targetPiece.name} 是己方背面卡牌，翻面后变为可选中，不受伤害");

                    // === 进入 IdleState 并结束回合 ===
                    manager.selectedPiece.Deselect();
                    manager.selectedPiece = null;
                    manager.SwitchTurn();
                    manager.SetState(new IdleState(manager));
                    return;
                }
            }

            // === 攻击目标为正面卡牌 ===
            if (targetPiece.canBeSelected)
            {
                // === 如果是敌方正面卡牌，直接销毁，并进入 MoveState 更新位置 ===
                if (targetPiece.side != manager.selectedPiece.side)
                {
                    Debug.Log($"{targetPiece.name} 是敌方正面卡牌，被销毁");
                    Object.Destroy(targetPiece.gameObject);

                    // === 进入 MoveState 并更新位置 ===
                    manager.SetState(new MoveState(manager, targetPiece.currentPosition));
                    return;
                }
                else
                {
                    // === 如果是己方正面卡牌，操作无效，返回 SelectState 并保持选中状态，不结束回合 ===
                    Debug.Log("无法攻击己方正面卡牌，返回 SelectState");
                    manager.SetState(new SelectState(manager));
                    return;
                }
            }
        }

        // === 显式实现 IChessState 接口的 HandleInput 方法 ===
        void IChessState.HandleInput() { }

        // === 显式实现 IChessState 接口的 Exit 方法 ===
        void IChessState.Exit()
        {
            Debug.Log("退出 Attack 状态");
        }
    }


}
