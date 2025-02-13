using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public List<CardData> playerCardsData;  // 玩家卡牌数据
    public List<CardData> enemyCardsData;   // 敌人卡牌数据
    public GameObject cardPrefab;           // 卡牌预制体
    public Transform boardTransform;        // 棋盘的 Transform，用来设置卡牌的父物体

    void Start()
    {
        CreateCards();
    }

    void CreateCards()
    {
        List<CardData> allCards = new List<CardData>();

        // 合并玩家卡牌和敌人卡牌数据
        allCards.AddRange(playerCardsData);
        allCards.AddRange(enemyCardsData);

        // 打乱卡牌顺序
        ShuffleList(allCards);

        // 生成卡牌
        for (int i = 0; i < allCards.Count; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, boardTransform);
            Card cardScript = cardObj.GetComponent<Card>();

            if (cardScript != null)
            {
                // 初始化卡牌数据
                cardScript.InitializeCard(allCards[i], i / 6, i % 6);

                // 设置卡牌位置
                Board.Instance.SetCardPosition(cardScript, i / 6, i % 6);
            }
        }
    }

    // 卡牌洗牌
    void ShuffleList(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            CardData temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
