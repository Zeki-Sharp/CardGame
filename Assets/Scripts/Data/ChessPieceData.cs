public class ChessPieceData
{
    public int id;
    public string side;  // 阵营（Red / Black）
    public string name;  // 棋子名称（如 Pawn, Chariot 等）
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
