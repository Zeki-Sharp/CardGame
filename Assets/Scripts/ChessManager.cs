using UnityEngine;
using ExcelDataReader;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class ChessManager : MonoBehaviour
{
    public string filePath = "Assets/Resources/ChessPieces.xlsx"; // Excel �ļ�·��
    public GameObject chessPiecePrefab; // ���ӵ�Ԥ����
    public ChessBoardController chessBoardController; // ���� ChessBoardController����ȡ���и��ӵ�λ����Ϣ
    public Transform chessPieceParent; // ���Ӹ������壨�����壩

    private List<ChessPieceData> pieceDataList = new List<ChessPieceData>();

    void Start()
    {
        // ��ȡ Excel ����
        LoadChessPieceData(filePath);

        // ���������������
        ShufflePieces();

        // �����������λ��
        InitializeChessPieces();
    }

    // ��ȡ Excel �ļ���������������
    void LoadChessPieceData(string filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath);

        using (var stream = fileInfo.Open(FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                do
                {
                    while (reader.Read()) // ��ȡÿһ��
                    {
                        // ������ͷ
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

    // ���������������
    void ShufflePieces()
    {
        pieceDataList = pieceDataList.OrderBy(a => Random.Range(0f, 1f)).ToList();
    }

    // �����������λ��
    void InitializeChessPieces()
    {
        List<Transform> availablePositions = new List<Transform>();

        // �����и��ӵ�λ����ӵ� availablePositions �б�
        foreach (var cellView in chessBoardController._cellViews)
        {
            availablePositions.Add(cellView.transform); // ��ȡÿ�� CellView �� Transform λ��
        }

        // ���Ϊ���ӷ���λ��
        for (int i = 0; i < pieceDataList.Count; i++)
        {
            ChessPieceData data = pieceDataList[i];

            // ���ѡ��һ��λ��
            int randomIndex = Random.Range(0, availablePositions.Count);
            Transform selectedPosition = availablePositions[randomIndex];

            // ��������ʵ������������õ����λ��
            GameObject pieceObj = Instantiate(chessPiecePrefab, selectedPosition.position, Quaternion.identity, chessPieceParent);

            // ��ȡ�����������ʼ��
            ChessPiece piece = pieceObj.GetComponent<ChessPiece>();
            piece.Initialize(data);  // �������ݸ����ӽ��г�ʼ��

            // �������ӵ�����
            pieceObj.name = $"{data.side}_{data.name}_{i + 1}";  // ��ʽ��: Red_Pawn_1, Black_Knight_2

            // �Ƴ���ռ�õ�λ��
            availablePositions.RemoveAt(randomIndex);
        }
    }
}
