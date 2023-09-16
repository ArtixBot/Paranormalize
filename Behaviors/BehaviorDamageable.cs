using Godot;
using System;

public partial class BehaviorDamageable : Node {
    private int _CurHP, _MaxHP;
    public int CurHP {
        get {return _CurHP;}
        set { _CurHP = Math.Min(value, MaxHP);}        // Whenever current HP is set, if it's greater than max HP, instead cap it at max HP.
    }
    public int MaxHP {
        get {return _MaxHP;}
        set { _MaxHP = value; CurHP = CurHP;}   // Call the CurHP setter to automatically update its value if curHP exceeded maxHP post-update.
    }
    public int barrier;       // Damageable things can have barrier applied to them. Barrier absorbs incoming damage before HP.
    
    // Emitted when an Damageable node takes damage.
    [Signal]
    public delegate void HealthDamagedEventHandler(BehaviorDamageable self);
    // Emitted when an Damageable node has its current HP reduced to or below zero.
    [Signal]
    public delegate void HealthDepletedEventHandler(BehaviorDamageable self);

    public void TakeDamage(int damage){
        if (damage <= 0) return;

        int barrierDmg = Math.Min(this.barrier, damage);
        this.barrier -= barrierDmg;
        damage -= barrierDmg;

        if (damage <= 0) return;
        this.CurHP -= damage;
        EmitSignal(SignalName.HealthDamaged, this);
        if (this.CurHP <= 0){
            EmitSignal(SignalName.HealthDepleted, this);
        }
    }
}
