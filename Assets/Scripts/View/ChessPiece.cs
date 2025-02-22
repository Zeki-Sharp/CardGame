using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    public string side;  // 阵营：Red / Black
    public string name;  // 类型：Pawn / Chariot 等
    public int attack;
    public int health;
    public string specialAbility;

    // 初始化棋子
    public void Initialize(ChessPieceData data)
    {
        this.side = data.side;
        this.name = data.name;
        this.attack = data.attack;
        this.health = data.health;
        this.specialAbility = data.specialAbility;

        SetPieceAppearance();  // 设置棋子的外观
    }

    // 设置棋子的外观（如图像）
    void SetPieceAppearance()
    {
        string imagePath = $"ChessPiecesSprites/{side}_{name}";  // 例如 "Red_Pawn", "Black_Chariot" 等
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = Resources.Load<Sprite>(imagePath); // 动态加载图像
    }
}
