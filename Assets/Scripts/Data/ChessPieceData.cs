public class ChessPieceData
{
    public int id;
    public string side;  // ��Ӫ��Red / Black��
    public string name;  // �������ƣ��� Pawn, Chariot �ȣ�
    public int attack;
    public int health;
    public string specialAbility;

    public ChessPieceData(int id, string side, string name, int attack, int health, string specialAbility)
    {
        this.id = id;
        this.side = side;
        this.name = name;
        this.attack = attack;
        this.health = health;
        this.specialAbility = specialAbility;
    }
}
