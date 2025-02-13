using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board Instance;

    public int rows = 4;  // ��������
    public int cols = 6;  // ��������
    public float horizontalSpacing = 2.0f;  // �м��
    public float verticalSpacing = 2.0f;    // �м��
    public Vector3 boardStartPosition = new Vector3(0, 0, 0);  // ������ʼλ��

    public Transform boardTransform;  // �������ÿ��Ƶĸ�����

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ���㿨�Ƶ���������
    public Vector3 GetCardPosition(int row, int col)
    {
        // ���㿨�Ƶ�λ�ã��������̵���ʼλ�ý���ƫ��
        float xPos = col * horizontalSpacing + boardStartPosition.x;
        float yPos = row * verticalSpacing + boardStartPosition.y;

        return new Vector3(xPos, yPos, 0);
    }

    // ���ÿ��Ƶ�λ��
    public void SetCardPosition(Card card, int row, int col)
    {
        card.SetPosition(row, col);
        // ʹ��������ʼλ�������ÿ���λ��
        card.transform.position = GetCardPosition(row, col);
    }
}
