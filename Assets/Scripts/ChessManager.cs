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
        List<ChessPiece> redPieces = new List<ChessPiece>();
        List<ChessPiece> blackPieces = new List<ChessPiece>();

        // === ��ʼ�����Ӳ�Ĭ�ϱ��泯�� ===
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

                    // === Ĭ�ϱ��泯���Ҳ���ѡ�� ===
                    piece.ShowBack();

                    // === ������Ӫ���� ===
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

        // === ������������Ʋ�����Ϊ��ѡ�� ===
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

                // === ����������� ===
                if (clickedObject is ChessPiece clickedPiece)
                {
                    // === ���������Ǽ������濨�� ===
                    if (clickedPiece.side == manager.CurrentPlayerSide && clickedPiece.canBeSelected)
                    {
                        // === ���������ǵ�ǰ��ѡ�е����ӣ�����ѡ��״̬�����л� ===
                        if (clickedPiece == manager.selectedPiece)
                        {
                            return;
                        }

                        // === ������������Χ�ж� ===
                        if (manager.selectedPiece.CanAttackTarget(clickedPiece))
                        {
                            // === ����ڹ�����Χ�ڣ����� AttackState ��׼������ ===
                            manager.SetState(new AttackState(manager, clickedPiece));
                            return;
                        }
                        else
                        {
                            // === ������ڹ�����Χ�ڣ�ֱ���л�ѡ�У������� SelectState���������غ� ===
                            manager.selectedPiece.Deselect();
                            manager.selectedPiece = clickedPiece;
                            manager.selectedPiece.Select();
                            return;
                        }
                    }
                    else
                    {
                        // === ���������ǵз����ӣ����� AttackState ��׼������ ===
                        manager.SetState(new AttackState(manager, clickedPiece));
                        return;
                    }
                }

                // === ������ǿո��� ===
                else if (clickedObject is Cell cell)
                {
                    // === ���� MoveState ������λ�� ===
                    manager.SetState(new MoveState(manager, cell.CellPosition));
                }

                // === ������ǿհ����� ===
                else
                {
                    // === �л��� IdleState ��ȡ��ѡ�� ===
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

            // === ͳһ��������״̬������λ�� ===

            // === 1. ���������ĵ�ǰλ������Ϊ�� ===
            manager.chessBoardController.GetChessBoard().piecePositions[manager.selectedPiece.currentPosition.x, manager.selectedPiece.currentPosition.y] = null;

            // === 2. ��Ŀ��λ������Ϊ���������� ===
            manager.chessBoardController.GetChessBoard().piecePositions[targetPosition.x, targetPosition.y] = manager.selectedPiece;

            // === 3. ���� selectedPiece �� currentPosition �� transform.position ===
            manager.selectedPiece.currentPosition = targetPosition;
            manager.selectedPiece.transform.position = manager.GetCellWorldPosition(targetPosition);

            Debug.Log($"{manager.selectedPiece.name} �ƶ��� {targetPosition}");

            // === 4. �ƶ���ɺ��л��� IdleState �������غ� ===
            manager.selectedPiece.Deselect();
            manager.selectedPiece = null;
            manager.SwitchTurn();
            manager.SetState(new IdleState(manager));
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

            // === ������������Χ�ж� ===
            if (!manager.selectedPiece.CanAttackTarget(targetPiece))
            {
                Debug.Log("����Ŀ�겻�ڹ�����Χ�ڣ����� SelectState");
                manager.SetState(new SelectState(manager));
                return;
            }

            // === ����Ŀ��Ϊ���濨�� ===
            if (!targetPiece.canBeSelected)
            {
                // === ���۵��ҹ�ϵ�������沢��Ϊ��ѡ�� ===
                targetPiece.ShowFront();
                Debug.Log($"{targetPiece.name} �����沢��Ϊ��ѡ��");

                // === ����ǵз����濨�ƣ������ֱ�����٣������� MoveState ����λ�� ===
                if (targetPiece.side != manager.selectedPiece.side)
                {
                    Debug.Log($"{targetPiece.name} �ǵз����濨�ƣ����������");
                    Object.Destroy(targetPiece.gameObject);

                    // === ���� MoveState ������λ�� ===
                    manager.SetState(new MoveState(manager, targetPiece.currentPosition));
                    return;
                }
                else
                {
                    // === ����Ǽ������濨�ƣ���������٣�����Ϊ��ѡ�У�������ԭ�� ===
                    Debug.Log($"{targetPiece.name} �Ǽ������濨�ƣ�������Ϊ��ѡ�У������˺�");

                    // === ���� IdleState �������غ� ===
                    manager.selectedPiece.Deselect();
                    manager.selectedPiece = null;
                    manager.SwitchTurn();
                    manager.SetState(new IdleState(manager));
                    return;
                }
            }

            // === ����Ŀ��Ϊ���濨�� ===
            if (targetPiece.canBeSelected)
            {
                // === ����ǵз����濨�ƣ�ֱ�����٣������� MoveState ����λ�� ===
                if (targetPiece.side != manager.selectedPiece.side)
                {
                    Debug.Log($"{targetPiece.name} �ǵз����濨�ƣ�������");
                    Object.Destroy(targetPiece.gameObject);

                    // === ���� MoveState ������λ�� ===
                    manager.SetState(new MoveState(manager, targetPiece.currentPosition));
                    return;
                }
                else
                {
                    // === ����Ǽ������濨�ƣ�������Ч������ SelectState ������ѡ��״̬���������غ� ===
                    Debug.Log("�޷������������濨�ƣ����� SelectState");
                    manager.SetState(new SelectState(manager));
                    return;
                }
            }
        }

        // === ��ʽʵ�� IChessState �ӿڵ� HandleInput ���� ===
        void IChessState.HandleInput() { }

        // === ��ʽʵ�� IChessState �ӿڵ� Exit ���� ===
        void IChessState.Exit()
        {
            Debug.Log("�˳� Attack ״̬");
        }
    }


}
