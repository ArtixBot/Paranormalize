using System.Collections.Generic;

public class SecondWind : AbstractAbility, IEventHandler<CombatEventAbilityActivated> {
    public static string id = "SECOND_WIND";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 3;
    private static int min_range = 0;
    private static int max_range = 0;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    public SecondWind(): base(
        id,
        strings,
        AbilityType.UTILITY,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit,
        new HashSet<TargetingModifiers>{TargetingModifiers.SELF}
    ){
    }

    public override void HandleEvent(CombatEventAbilityActivated data){
        base.HandleEvent(data);
        if (data.abilityActivated.Equals(this)){
            CombatManager.ExecuteAction(new RestoreAction(this.OWNER, 0.33f, RestoreAction.RestoreType.HEALTH, RestoreAction.RestorePercentType.PERCENTAGE_MISSING));
            CombatManager.ExecuteAction(new RestoreAction(this.OWNER, 0.33f, RestoreAction.RestoreType.POISE, RestoreAction.RestorePercentType.PERCENTAGE_MISSING));
        }
    }
}