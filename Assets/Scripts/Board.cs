using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board Instance;

    public int rows = 4;  // 棋盘行数
    public int cols = 6;  // 棋盘列数
    public float horizontalSpacing = 2.0f;  // 列间距
    public float verticalSpacing = 2.0f;    // 行间距
    public Vector3 boardStartPosition = new Vector3(0, 0, 0);  // 棋盘起始位置

    public Transform boardTransform;  // 用来设置卡牌的父物体

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 计算卡牌的世界坐标
    public Vector3 GetCardPosition(int row, int col)
    {
        // 计算卡牌的位置，基于棋盘的起始位置进行偏移
        float xPos = col * horizontalSpacing + boardStartPosition.x;
        float yPos = row * verticalSpacing + boardStartPosition.y;

        return new Vector3(xPos, yPos, 0);
    }

    // 设置卡牌的位置
    public void SetCardPosition(Card card, int row, int col)
    {
        card.SetPosition(row, col);
        // 使用棋盘起始位置来设置卡牌位置
        card.transform.position = GetCardPosition(row, col);
    }
}
