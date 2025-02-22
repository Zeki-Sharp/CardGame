using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard
{
    public int Width { get; private set; } // ���̵Ŀ�ȣ���λ����Ԫ������
    public int Height { get; private set; } // ���̵ĸ߶ȣ���λ����Ԫ������
    private List<Cell> _cells = new List<Cell>(); // �洢���е�Ԫ����б�

    public List<Cell> Cells => _cells; // �������ԣ����������ϵ����е�Ԫ��

    // ���캯�������ڳ�ʼ�����̵ĳߴ�͵�Ԫ��
    public ChessBoard(int width, int height)
    {
        Width = width; // �������̿��
        Height = height; // �������̸߶�

        // ���ݿ�Ⱥ͸߶ȴ������е�Ԫ��
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                // Ϊÿ��λ�ô���һ���µĵ�Ԫ�񣬲�������ӵ��б���
                var cell = new Cell(new Vector2Int(i, j));
                _cells.Add(cell);
            }
        }
    }
}
