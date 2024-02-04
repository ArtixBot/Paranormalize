using System.Collections.Generic;

public class TestAttack : AbstractAbility, IEventHandler<CombatEventDieHit> {
    public static string id = "TEST_ATTACK";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 0;
    private static int min_range = 0;
    private static int max_range = 2;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die atkDieA = new Die(DieType.BLUNT, 3, 3, "FIRST_DIE");
    private Die atkDieB = new Die(DieType.SLASH, 1, 5);

    public TestAttack(): base(
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
        this.BASE_DICE = new List<Die>{atkDieA, atkDieB};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_DIE_HIT, this, CombatEventPriority.STANDARD);
    }

    public virtual void HandleEvent(CombatEventDieHit data){
        if (data.die.Equals(atkDieA)){
            CombatManager.ExecuteAction(new ApplyStatusAction(data.hitUnit, new DebuffVulnerable(), 1));
            CombatManager.ExecuteAction(new ApplyStatusAction(data.hitUnit, new DebuffSlow(), 2));
            CombatManager.ExecuteAction(new ApplyStatusAction(this.OWNER, new BuffHaste(), 2));
        }
    }
}