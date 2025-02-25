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

    public GameObject cardFront;
    public GameObject cardBack;
    public bool canBeSelected = false;  // 默认不可选中


    // 初始化棋子
    public void Initialize(ChessPieceData data)
    {
        this.side = data.side;
        this.name = $"{side}_{data.name}";
        this.attack = data.attack;
        this.health = data.health;
        this.specialAbility = data.specialAbility;

        //正面卡面图片调用
        LoadSprite();
    }

    void Awake()
    {
        // 获取 SpriteRenderer
        spriteRenderer = cardFront.GetComponent<SpriteRenderer>();

        // 暂时注释 Outline Material 的初始化
        // outlineMaterial = spriteRenderer.material;

        // 初始化边框颜色
        SetOutline(false);

        // 初始化 CardFront 和 CardBack
        cardFront = transform.Find("CardFront").gameObject;
        cardBack = transform.Find("CardBack").gameObject;
        ShowBack();  // 默认背面朝上
    }

    // === 动态加载 Sprite 的方法 ===
    private void LoadSprite()
    {
        // 拼接图片路径
        string imagePath = $"ChessPiecesSprites/{name}";
        Sprite loadedSprite = Resources.Load<Sprite>(imagePath);

        if (loadedSprite != null)
        {
            // === 设置 CardFront 的 Sprite ===
            SpriteRenderer frontRenderer = cardFront.GetComponent<SpriteRenderer>();
            if (frontRenderer != null)
            {
                frontRenderer.sprite = loadedSprite;
            }
        }
        else
        {
            Debug.LogError($"未找到图片资源：{imagePath}");
        }
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
        // 增加 canBeSelected 判断，背面朝上时不响应点击
        if (!canBeSelected)
        {
            Debug.Log($"{gameObject.name} 不可被选中");
            return;  // 直接返回，不响应点击
        }

        // 如果可选中，继续执行选中逻辑
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

    public void ShowFront()
    {
        cardFront.SetActive(true);
        cardBack.SetActive(false);
        canBeSelected = true;  // 翻面后可选中
    }

    public void ShowBack()
    {
        cardFront.SetActive(false);
        cardBack.SetActive(true);
        canBeSelected = false;  // 背面朝上不可选中
    }
}
