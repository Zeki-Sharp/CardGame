using UnityEngine;

[CreateAssetMenu(fileName = "New Card Data", menuName = "Card Game/Card Data", order = 0)]
public class CardData : ScriptableObject
{
    public string cardName;      // 卡牌名字
    public int health;           // 生命值
    public int attackPower;      // 攻击力
    public bool isEnemy;         // 是否为敌方卡牌
}
