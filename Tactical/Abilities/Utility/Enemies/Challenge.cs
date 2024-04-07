using System.Collections.Generic;
using System.Linq;

public class Challenge : AbstractAbility, IEventHandler<CombatEventAbilityActivated> {
    public static string id = "CHALLENGE";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int MOVE_DISTANCE = 1;
    private static int cd = 0;
    private static int min_range = 0;
    private static int max_range = 5;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    public Challenge(): base(
        id,
        strings,
        AbilityType.UTILITY,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit,
        new HashSet<TargetingModifiers>{TargetingModifiers.ENEMIES_ONLY}
    ){
    }

    public override void HandleEvent(CombatEventAbilityActivated data){
        base.HandleEvent(data);
        if (data.abilityActivated == this){
            CombatManager.ExecuteAction(new ForwardAction(this.OWNER, data.target, MOVE_DISTANCE));
            CombatManager.ExecuteAction(new PullAction(this.OWNER, data.target, MOVE_DISTANCE));
        }
    }
}