using UnityEngine;

public class Card : MonoBehaviour
{
    public int row, col;  // 卡牌的行列位置
    public bool isFaceUp = false; // 是否翻面
    public bool isSelected = false; // 是否被选中
    public CardData cardData;  // 引用CardData资源

    public string cardName;
    public int health;
    public int attackPower;
    public bool isEnemy;

    // 初始化卡牌信息
    public void InitializeCard(CardData data, int row, int col)
    {
        if (data != null)
        {
            cardName = data.cardName;
            health = data.health;
            attackPower = data.attackPower;
            isEnemy = data.isEnemy;
        }
        this.row = row;
        this.col = col;
    }

    // 设置位置
    public void SetPosition(int newRow, int newCol)
    {
        row = newRow;
        col = newCol;
        // 更新世界坐标
        transform.position = Board.Instance.GetCardPosition(row, col);
        Debug.Log($"Card {cardName} position: {transform.position}");  // 输出卡牌的位置
    }

    // 翻面方法
    public void FlipCard()
    {
        isFaceUp = !isFaceUp;
        // 在这里可以添加翻面的视觉效果
    }

    // 攻击目标卡牌
    public void Attack(Card target)
    {
        if (target != null && target.isEnemy != this.isEnemy)
        {
            target.TakeDamage(this.attackPower);
        }
    }

    // 受到伤害
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Debug.Log($"{cardName} has been destroyed!");
        }
    }
}
