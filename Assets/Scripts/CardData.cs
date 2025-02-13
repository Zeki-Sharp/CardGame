using UnityEngine;

[CreateAssetMenu(fileName = "New Card Data", menuName = "Card Game/Card Data", order = 0)]
public class CardData : ScriptableObject
{
    public string cardName;      // ��������
    public int health;           // ����ֵ
    public int attackPower;      // ������
    public bool isEnemy;         // �Ƿ�Ϊ�з�����
}
