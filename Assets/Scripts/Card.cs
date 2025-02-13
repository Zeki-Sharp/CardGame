using UnityEngine;

public class Card : MonoBehaviour
{
    public int row, col;  // ���Ƶ�����λ��
    public bool isFaceUp = false; // �Ƿ���
    public bool isSelected = false; // �Ƿ�ѡ��
    public CardData cardData;  // ����CardData��Դ

    public string cardName;
    public int health;
    public int attackPower;
    public bool isEnemy;

    // ��ʼ��������Ϣ
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

    // ����λ��
    public void SetPosition(int newRow, int newCol)
    {
        row = newRow;
        col = newCol;
        // ������������
        transform.position = Board.Instance.GetCardPosition(row, col);
        Debug.Log($"Card {cardName} position: {transform.position}");  // ������Ƶ�λ��
    }

    // ���淽��
    public void FlipCard()
    {
        isFaceUp = !isFaceUp;
        // �����������ӷ�����Ӿ�Ч��
    }

    // ����Ŀ�꿨��
    public void Attack(Card target)
    {
        if (target != null && target.isEnemy != this.isEnemy)
        {
            target.TakeDamage(this.attackPower);
        }
    }

    // �ܵ��˺�
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Debug.Log($"{cardName} has been destroyed!");
        }
    }
}
