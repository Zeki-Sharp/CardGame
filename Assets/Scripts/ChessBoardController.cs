using System;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoardController : MonoBehaviour
{
    public Camera cam; // ������Ⱦ���̵����
    public CellView prefabCell; // ��Ԫ����ͼ��Ԥ����
    public const int CellSize = 84; // ÿ����Ԫ��ĳߴ磨��λ�����أ�
    public Vector2 CellOffset = new Vector2(0.5f, 0.5f); // ��Ԫ��֮���ƫ����
    public Transform ChessBoardTrans; // ���̵ĸ����������ڴ�����е�Ԫ����ͼ
    private ChessBoard _chessBoard; // ���̵ĺ������ݽṹ���������̵ĳߴ�����е�Ԫ��
    public List<CellView> _cellViews = new List<CellView>(); // ���ڴ洢ÿ����Ԫ����ͼ���б�
    private BoxCollider2D _chessBoardTransCollider; // ���̵���ײ�壬���ڿ������̵���������
    private float CellUnit = CellSize / 100f; // ��Ԫ��ĵ�λת����������ת��������λ��

    // ��ʼ��������ͨ�������������̺͵�Ԫ��
    public void Awake()
    {
        // ��ȡ���̵���ײ�����
        _chessBoardTransCollider = ChessBoardTrans.GetComponent<BoxCollider2D>();

        // ��ʼ���������ݣ�������һ��5x5�����̣�
        _chessBoard = new ChessBoard(6, 4);

        // ����������ײ��Ĵ�С��ʹ�串��������������
        _chessBoardTransCollider.size = new Vector2(_chessBoard.Width * (CellSize+CellOffset.x), _chessBoard.Height * (CellSize+CellOffset.y));
        _chessBoardTransCollider.offset = Vector2.zero; // �趨��ײ���ƫ����

        // ���������ϵ�ÿ����Ԫ��ʵ������Ӧ�ĵ�Ԫ����ͼ
        foreach (var cell in _chessBoard.Cells)
        {
            // ʵ����һ����Ԫ����ͼ����
            var cellViewObj = Instantiate(prefabCell, ChessBoardTrans);

            // ȷ��ʵ�����ɹ�
            if (cellViewObj != null)
            {
                // ���õ�Ԫ����ͼ��������ƣ����ڵ���
                cellViewObj.name = $"{cell.CellPosition}";
                var cellViewTransform = cellViewObj.transform; // ��ȡ��ͼ����� Transform
                var cellView = cellViewObj.GetComponent<CellView>(); // ��ȡ CellView ���
                cellView.SetCell(cell); // ����ǰ��Ԫ�����ݰ󶨵���ͼ

                // ����ͼ��ӵ��б���
                _cellViews.Add(cellView);

                // ����õ�Ԫ���ʵ��λ�ã���������������ʾ��
                var x = cell.CellPosition.x;
                var y = cell.CellPosition.y;

                // ������ͼ����ľֲ����꣬���ݵ�Ԫ���λ��������
                cellViewTransform.localPosition = new Vector3(CellUnit * x+CellOffset.x*x, CellUnit * y+CellOffset.y*y);
            }
        }
    }
}
