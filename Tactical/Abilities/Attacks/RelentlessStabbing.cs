using System.Collections.Generic;

public class RelentlessStabbing : AbstractAbility, IEventHandler<CombatEventDieHit>, IEventHandler<CombatEventAbilityActivated> {
    public static string id = "RELENTLESS_STABBING";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 2;
    private static int min_range = 0;
    private static int max_range = 1;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die evadeDie = new Die(DieType.EVADE, 7, 10);
    private Die atkDie = new Die(DieType.PIERCE, 7, 8, "LETHAL_HIT_CYCLING");

    private int cycles = 0;

    public RelentlessStabbing(): base(
        id,
        strings,
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