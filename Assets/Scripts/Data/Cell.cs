using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public Vector2Int CellPosition { get; private set; } // ��Ԫ���λ�ã�ʹ�� `Vector2Int` ��ʾ��X, Y ���꣩

    // ���캯�������ڳ�ʼ����Ԫ���λ����Ϣ
    public Cell(Vector2Int cellPosition)
    {
        CellPosition = cellPosition; // ���õ�Ԫ���λ����Ϣ
    }
}
