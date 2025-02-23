using System.Data.Common;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    public string side;  // 阵营：Red / Black
    public string _name;
    public string name;  // 类型：Pawn / Chariot 等
    public int attack;
    public int health;
    public string specialAbility;

    private SpriteRenderer spriteRenderer;
    private Material outlineMaterial;
    private bool isSelected = false;
    public Vector2Int currentPosition;


    // 初始化棋子
    public void Initialize(ChessPieceData data)
    {
        this.side = data.side;
        this.name = $"{side}_{data.name}";
        this.attack = data.attack;
        this.health = data.health;
        this.specialAbility = data.specialAbility;

        SetPieceAppearance();  // 设置棋子的外观
    }

    void Awake()
    {
        // 获取 SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 获取 Outline 材质
        outlineMaterial = spriteRenderer.material;

        // 初始化边框颜色
        SetOutline(false);
    }

    // 设置棋子的外观（如图像）
    void SetPieceAppearance()
    {
        // 使用类成员变量 spriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 如果在当前对象上找不到 SpriteRenderer，则从子对象中获取
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        string imagePath = $"ChessPiecesSprites/{name}";
        spriteRenderer.sprite = Resources.Load<Sprite>(imagePath);
    }

    // 设置边框颜色和显示状态
    private void SetOutline(bool enabled)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (enabled)
        {
            // 根据阵营设置不同的浅色
            if (side.ToLower() == "red")
            {
                spriteRenderer.color = Color.Lerp(Color.white, Color.green, 0.4f); // 浅绿色
            }
            else if (side.ToLower() == "black")
            {
                spriteRenderer.color = Color.Lerp(Color.white, Color.red, 0.4f); // 浅红色
            }
        }
        else
        {
            spriteRenderer.color = Color.white; // 恢复为默认颜色
        }
    }

    // === 棋子合法移动判断（默认实现） ===
    public virtual bool IsLegalMove(Vector2Int targetPosition)
    {
        // 获取当前位置
        Vector2Int currentPos = currentPosition;

        // 计算位置差值
        int deltaX = Mathf.Abs(targetPosition.x - currentPos.x);
        int deltaY = Mathf.Abs(targetPosition.y - currentPos.y);

        // 默认规则：上下左右一步
        bool isOneStepMove = (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
        if (isOneStepMove)
        {
            Debug.Log($"{name} 合法移动到 {targetPosition}");
            return true;
        }

        Debug.Log($"{name} 非法移动到 {targetPosition}");
        return false;
    }

    // === 棋子合法攻击判断（默认实现） ===
    public virtual bool CanAttackTarget(ChessPiece targetPiece)
    {
    

        // 获取当前位置和目标位置
        Vector2Int currentPos = this.currentPosition;
        Vector2Int targetPos = targetPiece.currentPosition;

        // 计算位置差值
        int deltaX = Mathf.Abs(targetPos.x - currentPos.x);
        int deltaY = Mathf.Abs(targetPos.y - currentPos.y);

        // 检查是否为上下左右一步
        bool isOneStepMove = (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
        if (isOneStepMove)
        {
            Debug.Log($"{name} 可以攻击 {targetPiece.name}");
            return true;
        }

        Debug.Log($"{name} 无法攻击 {targetPiece.name}，不在攻击范围内");
        return false;
    }


    // 选中棋子
    public void Select()
    {
        isSelected = true;
        Debug.Log($"{gameObject.name} Selected");
        SetOutline(true);
    }

    // 取消选中棋子
    public void Deselect()
    {
        isSelected = false;
        Debug.Log($"{gameObject.name} Deselected");
        SetOutline(false);
    }
}
