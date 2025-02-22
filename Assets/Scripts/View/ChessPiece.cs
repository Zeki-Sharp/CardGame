using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    public string side;  // ��Ӫ��Red / Black
    public string name;  // ���ͣ�Pawn / Chariot ��
    public int attack;
    public int health;
    public string specialAbility;

    // ��ʼ������
    public void Initialize(ChessPieceData data)
    {
        this.side = data.side;
        this.name = data.name;
        this.attack = data.attack;
        this.health = data.health;
        this.specialAbility = data.specialAbility;

        SetPieceAppearance();  // �������ӵ����
    }

    // �������ӵ���ۣ���ͼ��
    void SetPieceAppearance()
    {
        string imagePath = $"ChessPiecesSprites/{side}_{name}";  // ���� "Red_Pawn", "Black_Chariot" ��
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = Resources.Load<Sprite>(imagePath); // ��̬����ͼ��
    }
}
