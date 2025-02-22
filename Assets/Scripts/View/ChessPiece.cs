using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    public string side;  // ��Ӫ��Red / Black
    public string name;  // ���ͣ�Pawn / Chariot ��
    public int attack;
    public int health;
    public string specialAbility;

    private SpriteRenderer spriteRenderer;
    private Material outlineMaterial;
    private bool isSelected = false;

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

    void Awake()
    {
        // ��ȡ SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ��ȡ Outline ����
        outlineMaterial = spriteRenderer.material;

        // ��ʼ���߿���ɫ
        SetOutline(false);
    }

    // �������ӵ���ۣ���ͼ��
    void SetPieceAppearance()
    {
        // ʹ�����Ա���� spriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ����ڵ�ǰ�������Ҳ��� SpriteRenderer������Ӷ����л�ȡ
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        string imagePath = $"ChessPiecesSprites/{side}_{name}";
        spriteRenderer.sprite = Resources.Load<Sprite>(imagePath);
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


    // ѡ������
    public void Select()
    {
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
}
