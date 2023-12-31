using System.Collections.Generic;

public class RelentlessStabbing : AbstractAbility, IEventHandler<CombatEventDieHit>, IEventHandler<CombatEventAbilityActivated> {
    public static string id = "RELENTLESS_STABBING";
    // TODO: Make all of these read in by JSON.
    private static string name = "Relentless Stabbing";
    private static string desc = "On lethal hit: Cycle this die (up to 2 times).";

    // TODO: Should gameplay attributes also be defined in JSON? e.g. base CD, min range, max range, dice, etc...
    private static int cd = 2;
    private static int min_range = 0;
    private static int max_range = 1;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private int cycles = 0;

    private Die evadeDie = new Die(DieType.EVADE, 7, 10);
    private Die atkDie = new Die(DieType.PIERCE, 7, 8, "LETHAL_HIT_CYCLING");

    public RelentlessStabbing(): base(
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
        this.BASE_DICE = new List<Die>{evadeDie, atkDie};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_DIE_HIT, this, CombatEventPriority.STANDARD);
    }

    public virtual void HandleEvent(CombatEventDieHit data){
        // TODO: Add && this.OWNER.HasStatusEffect(Lethality);
        if (data.die.Equals(atkDie) && data.rolledMaximumNaturalValue && this.cycles < 2){
            this.cycles += 1;
            CombatManager.CycleDie(this.OWNER, data.die);
        }
    }

    public override void HandleEvent(CombatEventAbilityActivated data){
        base.HandleEvent(data);
        if (data.abilityActivated.Equals(this)){
            this.cycles = 0;
        }
    }
}