using System.Collections.Generic;
using System.Linq;
using Godot;

public class GallantSlashes : AbstractAbility, IEventHandler<CombatEventDieHit>{
    public static string id = "GALLANT_SLASHES";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 1;
    private static int min_range = 0;
    private static int max_range = 2;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die slashDie = new Die(DieType.SLASH, 6, 12, "NEXT_ROUND_STR_ON_HIT");

    public GallantSlashes(): base(
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
        this.BASE_DICE = new List<Die>{slashDie, slashDie};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_DIE_HIT, this, CombatEventPriority.STANDARD);
    }

    public void HandleEvent(CombatEventDieHit data){
        if (data.die == slashDie){
            CombatManager.ExecuteAction(new ApplyStatusAction(this.OWNER, new ConditionNextRoundStatusGain(new BuffStrength()), 1));
        }
    }
}