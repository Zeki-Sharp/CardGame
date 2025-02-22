using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard
{
    public int Width { get; private set; } // 棋盘的宽度（单位：单元格数）
    public int Height { get; private set; } // 棋盘的高度（单位：单元格数）
    private List<Cell> _cells = new List<Cell>(); // 存储所有单元格的列表

    public List<Cell> Cells => _cells; // 公开属性，返回棋盘上的所有单元格

    // 构造函数，用于初始化棋盘的尺寸和单元格
    public ChessBoard(int width, int height)
    {
        Width = width; // 设置棋盘宽度
        Height = height; // 设置棋盘高度

        // 根据宽度和高度创建所有单元格
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                // 为每个位置创建一个新的单元格，并将其添加到列表中
                var cell = new Cell(new Vector2Int(i, j));
                _cells.Add(cell);
            }
        }
    }
}
