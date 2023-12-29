using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Godot;

public class TestAttack : AbstractAbility, IEventHandler<CombatEventDieHit> {
    public static string id = "BASE_ATTACK";
    // TODO: Make all of these read in by JSON.
    private static string name = "Base Attack";
    private static string desc = "";

    // TODO: Should gameplay attributes also be defined in JSON? e.g. base CD, min range, max range, dice, etc...
    private static int cd = 0;
    private static int min_range = 0;
    private static int max_range = 1;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die atkDieA = new Die(DieType.BLUNT, 3, 3, "APPLY_VULNERABILITY");

    public TestAttack(): base(
        id,
        name,
        desc,
        AbilityType.ATTACK,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit,
        new HashSet<TargetingModifiers>{TargetingModifiers.ENEMIES_ONLY}
    ){
        this.BASE_DICE = new List<Die>{atkDieA};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_DIE_HIT, this, CombatEventPriority.STANDARD);
    }

    public virtual void HandleEvent(CombatEventDieHit data){
        if (data.die.Equals(atkDieA)){
            CombatManager.ExecuteAction(new ApplyStatusAction(data.hitUnit, new DebuffVulnerable(), 1));
        }
    }
}