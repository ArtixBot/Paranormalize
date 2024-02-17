using System.Collections.Generic;

public class Assault : AbstractAbility, IEventHandler<CombatEventAbilityActivated>, IEventHandler<CombatEventDieHit> {
    public static string id = "ASSAULT";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int MOVE_DISTANCE = 1;
    private static int cd = 2;
    private static int min_range = 0;
    private static int max_range = 2;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die atkDie = new Die(DieType.SLASH, 7, 13, "MOVE_UP_ACTIONS");
    private Die defDie = new Die(DieType.BLOCK, 5, 6);

    public Assault(): base(
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
        this.BASE_DICE = new List<Die>{atkDie, defDie};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_ABILITY_ACTIVATED, this, CombatEventPriority.STANDARD);
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_DIE_HIT, this, CombatEventPriority.STANDARD);
    }

    public override void HandleEvent(CombatEventAbilityActivated data){
        base.HandleEvent(data);
        if (data.abilityActivated.Equals(this)){
            CombatManager.ExecuteAction(new ForwardAction(this.OWNER, data.target, MOVE_DISTANCE));
        }
    }

    public virtual void HandleEvent(CombatEventDieHit data){
        if (data.die.Equals(atkDie)){
            CombatManager.combatInstance.turnlist.ModifyItemPriority(this.OWNER, CombatManager.combatInstance.turnlist.GetNextInstanceOfItem(this.OWNER).priority, setToValue: true);
        }
    }
}