using System.Collections.Generic;

public class Endure : AbstractAbility, IEventHandler<CombatEventAbilityActivated> {
    public static string id = "ENDURE";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 2;
    private static int min_range = 0;
    private static int max_range = 0;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    public int STACKS_TO_GAIN = 4;

    public Endure(): base(
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
            CombatManager.ExecuteAction(new ApplyStatusAction(this.OWNER, new BuffProtection(), STACKS_TO_GAIN));
            CombatManager.ExecuteAction(new ApplyStatusAction(this.OWNER, new BuffPoiseProtection(), STACKS_TO_GAIN));
            CombatManager.ExecuteAction(new ApplyStatusAction(this.OWNER, new ConditionNextRoundStatusGain(new BuffProtection()), STACKS_TO_GAIN));
            CombatManager.ExecuteAction(new ApplyStatusAction(this.OWNER, new ConditionNextRoundStatusGain(new BuffPoiseProtection()), STACKS_TO_GAIN));
        }
    }
}