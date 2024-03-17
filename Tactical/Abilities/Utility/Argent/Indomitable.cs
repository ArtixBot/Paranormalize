using System.Collections.Generic;

public class Indomitable : AbstractAbility, IEventHandler<CombatEventAbilityActivated> {
    public static string id = "INDOMITABLE";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 5;
    private static int min_range = 0;
    private static int max_range = 0;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    public Indomitable(): base(
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
            CombatManager.ExecuteAction(new RestoreAction(this.OWNER, 0.33f, RestoreAction.RestoreType.HEALTH, RestoreAction.RestorePercentType.PERCENTAGE_MAX));
            CombatManager.ExecuteAction(new RestoreAction(this.OWNER, 0.33f, RestoreAction.RestoreType.POISE, RestoreAction.RestorePercentType.PERCENTAGE_MAX));
        }
    }
}