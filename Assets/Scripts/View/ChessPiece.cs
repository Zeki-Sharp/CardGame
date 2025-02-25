using System.Data.Common;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    public string side;  // ��Ӫ��Red / Black
    public string _name;
    public string name;  // ���ͣ�Pawn / Chariot ��
    public int attack;
    public int health;
    public string specialAbility;

    private SpriteRenderer spriteRenderer;
    private Material outlineMaterial;
    private bool isSelected = false;
    public Vector2Int currentPosition;

    public GameObject cardFront;
    public GameObject cardBack;
    public bool canBeSelected = false;  // Ĭ�ϲ���ѡ��


    // ��ʼ������
    public void Initialize(ChessPieceData data)
    {
        this.side = data.side;
        this.name = $"{side}_{data.name}";
        this.attack = data.attack;
        this.health = data.health;
        this.specialAbility = data.specialAbility;

        //���濨��ͼƬ����
        LoadSprite();
    }

    void Awake()
    {
        // ��ȡ SpriteRenderer
        spriteRenderer = cardFront.GetComponent<SpriteRenderer>();

        // ��ʱע�� Outline Material �ĳ�ʼ��
        // outlineMaterial = spriteRenderer.material;

        // ��ʼ���߿���ɫ
        SetOutline(false);

        // ��ʼ�� CardFront �� CardBack
        cardFront = transform.Find("CardFront").gameObject;
        cardBack = transform.Find("CardBack").gameObject;
        ShowBack();  // Ĭ�ϱ��泯��
    }

    // === ��̬���� Sprite �ķ��� ===
    private void LoadSprite()
    {
        // ƴ��ͼƬ·��
        string imagePath = $"ChessPiecesSprites/{name}";
        Sprite loadedSprite = Resources.Load<Sprite>(imagePath);

        if (loadedSprite != null)
        {
            // === ���� CardFront �� Sprite ===
            SpriteRenderer frontRenderer = cardFront.GetComponent<SpriteRenderer>();
            if (frontRenderer != null)
            {
                frontRenderer.sprite = loadedSprite;
            }
        }
        else
        {
            Debug.LogError($"δ�ҵ�ͼƬ��Դ��{imagePath}");
        }
    }

    // ���ñ߿���ɫ����ʾ״̬
    private void SetOutline(bool enabled)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (enabled)
        {
            // ������Ӫ���ò�ͬ��ǳɫ
            if (side.ToLower() == "red")
            {
                spriteRenderer.color = Color.Lerp(Color.white, Color.green, 0.4f); // ǳ��ɫ
            }
            else if (side.ToLower() == "black")
            {
                spriteRenderer.color = Color.Lerp(Color.white, Color.red, 0.4f); // ǳ��ɫ
            }
        }
        else
        {
            spriteRenderer.color = Color.white; // �ָ�ΪĬ����ɫ
        }
    }

    // === ���ӺϷ��ƶ��жϣ�Ĭ��ʵ�֣� ===
    public virtual bool IsLegalMove(Vector2Int targetPosition)
    {
        // ��ȡ��ǰλ��
        Vector2Int currentPos = currentPosition;

        // ����λ�ò�ֵ
        int deltaX = Mathf.Abs(targetPosition.x - currentPos.x);
        int deltaY = Mathf.Abs(targetPosition.y - currentPos.y);

        // Ĭ�Ϲ�����������һ��
        bool isOneStepMove = (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
        if (isOneStepMove)
        {
            Debug.Log($"{name} �Ϸ��ƶ��� {targetPosition}");
            return true;
        }

        Debug.Log($"{name} �Ƿ��ƶ��� {targetPosition}");
        return false;
    }

    // === ���ӺϷ������жϣ�Ĭ��ʵ�֣� ===
    public virtual bool CanAttackTarget(ChessPiece targetPiece)
    {
    

        // ��ȡ��ǰλ�ú�Ŀ��λ��
        Vector2Int currentPos = this.currentPosition;
        Vector2Int targetPos = targetPiece.currentPosition;

        // ����λ�ò�ֵ
        int deltaX = Mathf.Abs(targetPos.x - currentPos.x);
        int deltaY = Mathf.Abs(targetPos.y - currentPos.y);

        // ����Ƿ�Ϊ��������һ��
        bool isOneStepMove = (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
        if (isOneStepMove)
        {
            Debug.Log($"{name} ���Թ��� {targetPiece.name}");
            return true;
        }

        Debug.Log($"{name} �޷����� {targetPiece.name}�����ڹ�����Χ��");
        return false;
    }


    // ѡ������
    public void Select()
    {
        // ���� canBeSelected �жϣ����泯��ʱ����Ӧ���
        if (!canBeSelected)
        {
            Debug.Log($"{gameObject.name} ���ɱ�ѡ��");
            return;  // ֱ�ӷ��أ�����Ӧ���
        }

        // �����ѡ�У�����ִ��ѡ���߼�
        isSelected = true;
        Debug.Log($"{gameObject.name} Selected");
        SetOutline(true);
    }

    // ȡ��ѡ������
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
        canBeSelected = true;  // ������ѡ��
    }

    public void ShowBack()
    {
        cardFront.SetActive(false);
        cardBack.SetActive(true);
        canBeSelected = false;  // ���泯�ϲ���ѡ��
    }
}
