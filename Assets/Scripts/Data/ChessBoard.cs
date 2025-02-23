using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard
{
    public int Width { get; private set; } // ���̵Ŀ�ȣ���λ����Ԫ������
    public int Height { get; private set; } // ���̵ĸ߶ȣ���λ����Ԫ������
    private List<Cell> _cells = new List<Cell>(); // �洢���е�Ԫ����б�

    public List<Cell> Cells => _cells; // �������ԣ����������ϵ����е�Ԫ��
    public ChessPiece[,] piecePositions;

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

        // ��ʼ������ռ��״̬
        piecePositions = new ChessPiece[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                piecePositions[x, y] = null; // ��ʼ״̬Ϊ��
            }
        }
    }


}
