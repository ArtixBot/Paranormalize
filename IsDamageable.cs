using Godot;
using System;

public partial class IsDamageable : Node {
    public int curHP, maxHP;
    public int curShield;       // Damageable things can have shield applied to them. Shield absorbs incoming damage before HP.
}
