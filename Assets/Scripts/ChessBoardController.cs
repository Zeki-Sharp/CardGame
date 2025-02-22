using System;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoardController : MonoBehaviour
{
    public Camera cam; // 用于渲染棋盘的相机
    public CellView prefabCell; // 单元格视图的预制体
    public const int CellSize = 84; // 每个单元格的尺寸（单位：像素）
    public Vector2 CellOffset = new Vector2(0.5f, 0.5f); // 单元格之间的偏移量
    public Transform ChessBoardTrans; // 棋盘的父级对象，用于存放所有单元格视图
    private ChessBoard _chessBoard; // 棋盘的核心数据结构，包含棋盘的尺寸和所有单元格
    public List<CellView> _cellViews = new List<CellView>(); // 用于存储每个单元格视图的列表
    private BoxCollider2D _chessBoardTransCollider; // 棋盘的碰撞体，用于控制棋盘的物理区域
    private float CellUnit = CellSize / 100f; // 单元格的单位转换（从像素转换到网格单位）

    // 初始化函数，通常用于设置棋盘和单元格
    public void Awake()
    {
        // 获取棋盘的碰撞体组件
        _chessBoardTransCollider = ChessBoardTrans.GetComponent<BoxCollider2D>();

        // 初始化棋盘数据（这里是一个5x5的棋盘）
        _chessBoard = new ChessBoard(6, 4);

        // 设置棋盘碰撞体的大小，使其覆盖整个棋盘区域
        _chessBoardTransCollider.size = new Vector2(_chessBoard.Width * (CellSize+CellOffset.x), _chessBoard.Height * (CellSize+CellOffset.y));
        _chessBoardTransCollider.offset = Vector2.zero; // 设定碰撞体的偏移量

        // 遍历棋盘上的每个单元格，实例化对应的单元格视图
        foreach (var cell in _chessBoard.Cells)
        {
            // 实例化一个单元格视图对象
            var cellViewObj = Instantiate(prefabCell, ChessBoardTrans);

            // 确保实例化成功
            if (cellViewObj != null)
            {
                // 设置单元格视图对象的名称，便于调试
                cellViewObj.name = $"{cell.CellPosition}";
                var cellViewTransform = cellViewObj.transform; // 获取视图对象的 Transform
                var cellView = cellViewObj.GetComponent<CellView>(); // 获取 CellView 组件
                cellView.SetCell(cell); // 将当前单元格数据绑定到视图

                // 将视图添加到列表中
                _cellViews.Add(cellView);

                // 计算该单元格的实际位置（用于在棋盘上显示）
                var x = cell.CellPosition.x;
                var y = cell.CellPosition.y;

                // 设置视图对象的局部坐标，根据单元格的位置来排列
                cellViewTransform.localPosition = new Vector3(CellUnit * x+CellOffset.x*x, CellUnit * y+CellOffset.y*y);
            }
        }
    }
}
