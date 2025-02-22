using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public Vector2Int CellPosition { get; private set; } // 单元格的位置，使用 `Vector2Int` 表示（X, Y 坐标）

    // 构造函数，用于初始化单元格的位置信息
    public Cell(Vector2Int cellPosition)
    {
        CellPosition = cellPosition; // 设置单元格的位置信息
    }
}
