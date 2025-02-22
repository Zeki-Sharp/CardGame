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

    void Start()
    {
        // 读取 Excel 数据
        LoadChessPieceData(filePath);

        // 随机打乱棋子配置
        ShufflePieces();

        // 随机分配棋子位置
        InitializeChessPieces();
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
        List<Transform> availablePositions = new List<Transform>();

        // 将所有格子的位置添加到 availablePositions 列表
        foreach (var cellView in chessBoardController._cellViews)
        {
            availablePositions.Add(cellView.transform); // 获取每个 CellView 的 Transform 位置
        }

        // 随机为棋子分配位置
        for (int i = 0; i < pieceDataList.Count; i++)
        {
            ChessPieceData data = pieceDataList[i];

            // 随机选择一个位置
            int randomIndex = Random.Range(0, availablePositions.Count);
            Transform selectedPosition = availablePositions[randomIndex];

            // 创建棋子实例，并将其放置到随机位置
            GameObject pieceObj = Instantiate(chessPiecePrefab, selectedPosition.position, Quaternion.identity, chessPieceParent);

            // 获取棋子组件并初始化
            ChessPiece piece = pieceObj.GetComponent<ChessPiece>();
            piece.Initialize(data);  // 传递数据给棋子进行初始化

            // 设置棋子的名字
            pieceObj.name = $"{data.side}_{data.name}_{i + 1}";  // 格式如: Red_Pawn_1, Black_Knight_2

            // 移除已占用的位置
            availablePositions.RemoveAt(randomIndex);
        }
    }
}
