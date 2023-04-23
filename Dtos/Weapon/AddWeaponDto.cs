namespace RPG.Dtos.Weapon;

public class AddWeaponDto
{
    public string Name { get; set; } = null!;
    public int Damage { get; set; }
    public int CharacterId { get; set;}
}