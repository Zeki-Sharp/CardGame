using UnityEngine;
using ExcelDataReader;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;

// === ״̬ö�� ===
public enum GameState
{
    Idle,
    Select,
    Move,
    Attack
}

// === ״̬�ӿ� ===
public interface IChessState
{
    void Enter();
    void HandleInput();
    void Exit();
}

public class ChessManager : MonoBehaviour
{
    public string filePath = "Assets/Resources/ChessPieces.xlsx"; // Excel �ļ�·��
    public GameObject chessPiecePrefab; // ���ӵ�Ԥ����
    public ChessBoardController chessBoardController; // ���� ChessBoardController����ȡ���и��ӵ�λ����Ϣ
    public Transform chessPieceParent; // ���Ӹ������壨�����壩

    private List<ChessPieceData> pieceDataList = new List<ChessPieceData>();

    public Camera cam; // �������
    [SerializeField] public ChessPiece selectedPiece = null; // ��ǰѡ�е�����
    public string CurrentPlayerSide = "Red"; // ��ǰ��ҵ���Ӫ��Ĭ�Ϻ췽����

    // === ״̬�� ===
    private IChessState currentState;

    // === ״̬�л����� ===
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
        // ��ȡ Excel ����
        LoadChessPieceData(filePath);

        // ���������������
        ShufflePieces();

        // �����������λ��
        InitializeChessPieces();

        // ��ʼ��״̬��
        SetState(new IdleState(this));

        // �������
        DebugPiecePositions();
    }

    void Update()
    {
        if (currentState != null)
        {
            currentState.HandleInput();
        }
    }

    // === ���� Raycast ��� ===
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


    // === ��ȡ�������� ===
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


    // === �л��غ� ===
    public void SwitchTurn()
    {
        CurrentPlayerSide = (CurrentPlayerSide == "Red") ? "Black" : "Red";
        Debug.Log($"��ǰ�غϣ�{CurrentPlayerSide}");
    }

    // === ��ȡ Excel ���� ===
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

    // === ��������������� ===
    void ShufflePieces()
    {
        pieceDataList = pieceDataList.OrderBy(a => Random.Range(0f, 1f)).ToList();
    }

    // === ��ʼ������λ�� ===
    void InitializeChessPieces()
    {
        List<CellView> availableCells = new List<CellView>();

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

                    availableCells.RemoveAt(randomIndex);
                    isPlaced = true;
                }
            }
        }
    }

    // === ���������piecePositions �� currentPosition ״̬ ===
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

public class IdleState : IChessState
{
    private ChessManager manager;

    public IdleState(ChessManager manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {
        Debug.Log("���� Idle ״̬");
    }

    public void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            object clickedObject = manager.GetClickedObject();

            // === ���ж����ͣ����ж� canBeSelected ===
            if (clickedObject is ChessPiece)
            {
                ChessPiece clickedPiece = (ChessPiece)clickedObject;

                // === ���� canBeSelected �ж� ===
                if (!clickedPiece.canBeSelected)
                {
                    Debug.Log($"{clickedPiece.gameObject.name} ���ɱ�ѡ�У�״̬�����л�");
                    return;  // ���泯��ʱֱ�ӷ��أ������� Select ״̬
                }

                // ������Ա�ѡ�����ǵ�ǰ�غϵ�����
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
        Debug.Log("�˳� Idle ״̬");
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
        Debug.Log("���� Select ״̬");
    }

    public void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            object clickedObject = manager.GetClickedObject();
            if (clickedObject is ChessPiece piece)
            {
                if (piece.side == manager.CurrentPlayerSide)
                {
                    manager.selectedPiece.Deselect();
                    manager.selectedPiece = piece;
                    manager.selectedPiece.Select();
                }
                else
                {
                    manager.SetState(new AttackState(manager, piece));
                }
            }
            else if (clickedObject is Cell cell)
            {
                manager.SetState(new MoveState(manager, cell.CellPosition));
            }
            else
            {
                manager.selectedPiece.Deselect();
                manager.selectedPiece = null;
                manager.SetState(new IdleState(manager));
            }
        }
    }


    public void Exit()
    {
        Debug.Log("�˳� Select ״̬");
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
        Debug.Log("���� Move ״̬");

        // === ������������ĺϷ��ƶ��ж� ===
        if (manager.selectedPiece.IsLegalMove(targetPosition))
        {
            // === �ж�Ŀ�����״̬ ===
            ChessPiece targetPiece = manager.chessBoardController.GetChessBoard().piecePositions[targetPosition.x, targetPosition.y];

            // === Ŀ�����Ϊ�գ�ֱ���ƶ� ===
            if (targetPiece == null)
            {
                // ��������ռ��״̬
                manager.chessBoardController.GetChessBoard().piecePositions[manager.selectedPiece.currentPosition.x, manager.selectedPiece.currentPosition.y] = null;
                manager.chessBoardController.GetChessBoard().piecePositions[targetPosition.x, targetPosition.y] = manager.selectedPiece;

                // �������ӵĵ�ǰλ��
                manager.selectedPiece.currentPosition = targetPosition;

                // �ƶ����ӵ�����λ��
                manager.selectedPiece.transform.position = manager.GetCellWorldPosition(targetPosition);

                Debug.Log($"{manager.selectedPiece.name} �ƶ��� {targetPosition}");

                // �ƶ����л��غ�
                manager.selectedPiece.Deselect();
                manager.selectedPiece = null;
                manager.SwitchTurn();
                manager.SetState(new IdleState(manager));
            }
            // === Ŀ����ӱ��Է�����ռ�ã��л��� AttackState ===
            else if (targetPiece.side != manager.CurrentPlayerSide)
            {
                Debug.Log($"׼��������{manager.selectedPiece.name} ׼������ {targetPiece.name}");
                manager.SetState(new AttackState(manager, targetPiece));
            }
            else
            {
                Debug.Log("Ŀ����ӱ���������ռ�ã��Ƿ��ƶ�");
                // ����ѡ��״̬������ SelectState
                manager.SetState(new SelectState(manager));
            }
        }
        else
        {
            Debug.Log("����������������ƶ�������ѡ��״̬");
            // ���� SelectState����ȡ��ѡ��
            manager.SetState(new SelectState(manager));
        }
    }

    // === �޸�����ʽʵ�� IChessState �ӿڵ� HandleInput ���� ===
    void IChessState.HandleInput() { }

    // === �޸�����ʽʵ�� IChessState �ӿڵ� Exit ���� ===
    void IChessState.Exit()
    {
        Debug.Log("�˳� Move ״̬");
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
        Debug.Log("���� Attack ״̬");

        // === ������������ĺϷ������ж� ===
        if (manager.selectedPiece.CanAttackTarget(targetPiece))
        {
            Debug.Log($"{manager.selectedPiece.name} ���� {targetPiece.name}");

            // === �Ƴ������������� ===
            Object.Destroy(targetPiece.gameObject);

            // ��������ռ��״̬
            Vector2Int oldPosition = manager.selectedPiece.currentPosition;
            Vector2Int newPosition = targetPiece.currentPosition;
            manager.chessBoardController.GetChessBoard().piecePositions[oldPosition.x, oldPosition.y] = null;
            manager.chessBoardController.GetChessBoard().piecePositions[newPosition.x, newPosition.y] = manager.selectedPiece;

            // �������ӵĵ�ǰλ��
            manager.selectedPiece.currentPosition = newPosition;

            // �ƶ����ӵ�����λ��
            manager.selectedPiece.transform.position = manager.GetCellWorldPosition(newPosition);

            Debug.Log($"{manager.selectedPiece.name} �ƶ��� {newPosition}");

            // �������л��غ�
            manager.selectedPiece.Deselect();
            manager.selectedPiece = null;
            manager.SwitchTurn();
            manager.SetState(new IdleState(manager));
        }
        else
        {
            Debug.Log("�������Ϸ�������ѡ��״̬");
            // ���� SelectState����ȡ��ѡ��
            manager.SetState(new SelectState(manager));
        }
    }

    // === �޸�����ʽʵ�� IChessState �ӿڵ� HandleInput ���� ===
    void IChessState.HandleInput() { }

    // === �޸�����ʽʵ�� IChessState �ӿڵ� Exit ���� ===
    void IChessState.Exit()
    {
        Debug.Log("�˳� Attack ״̬");
    }
}
