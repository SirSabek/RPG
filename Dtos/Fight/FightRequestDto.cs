namespace RPG.Dtos.Fight;

public class FightRequestDto
{
    public List<int> CharacterIds { get; set; }
    public int Reward { get; set; }
}