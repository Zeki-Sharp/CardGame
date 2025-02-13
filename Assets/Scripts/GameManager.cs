using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public List<CardData> playerCardsData;  // ��ҿ�������
    public List<CardData> enemyCardsData;   // ���˿�������
    public GameObject cardPrefab;           // ����Ԥ����
    public Transform boardTransform;        // ���̵� Transform���������ÿ��Ƶĸ�����

    void Start()
    {
        CreateCards();
    }

    void CreateCards()
    {
        List<CardData> allCards = new List<CardData>();

        // �ϲ���ҿ��ƺ͵��˿�������
        allCards.AddRange(playerCardsData);
        allCards.AddRange(enemyCardsData);

        // ���ҿ���˳��
        ShuffleList(allCards);

        // ���ɿ���
        for (int i = 0; i < allCards.Count; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, boardTransform);
            Card cardScript = cardObj.GetComponent<Card>();

            if (cardScript != null)
            {
                // ��ʼ����������
                cardScript.InitializeCard(allCards[i], i / 6, i % 6);

                // ���ÿ���λ��
                Board.Instance.SetCardPosition(cardScript, i / 6, i % 6);
            }
        }
    }

    // ����ϴ��
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
