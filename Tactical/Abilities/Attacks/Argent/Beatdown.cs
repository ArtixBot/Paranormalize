using System.Collections.Generic;
using System.Linq;
using Godot;

public class Beatdown : AbstractAbility, IEventHandler<CombatEventDieHit>{
    public static string id = "BEATDOWN";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 2;
    private static int min_range = 0;
    private static int max_range = 1;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die bluntDie = new Die(DieType.BLUNT, 6, 7, "NEXT_ROUND_STR_ON_HIT");

    public Beatdown(): base(
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
        this.BASE_DICE = new List<Die>{bluntDie, bluntDie};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_DIE_HIT, this, CombatEventPriority.STANDARD);
    }

    public void HandleEvent(CombatEventDieHit data){
        if (data.die == bluntDie){
            CombatManager.ExecuteAction(new ApplyStatusAction(this.OWNER, new ConditionNextRoundStatusGain(new BuffStrength()), 1));
        }
    }
}