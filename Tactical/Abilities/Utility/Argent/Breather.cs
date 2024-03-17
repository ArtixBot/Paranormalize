using System.Collections.Generic;

public class Breather : AbstractAbility, IEventHandler<CombatEventAbilityActivated> {
    public static string id = "BREATHER";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 0;
    private static int min_range = 0;
    private static int max_range = 0;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    public Breather(): base(
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
            for (int i = 0; i < 2; i++){
                List<AbstractAbility> unavailableAbilities = this.OWNER.UnavailableAbilities;
                if (unavailableAbilities.Count == 0) break;
                CombatManager.ExecuteAction(new InvigorateAction(unavailableAbilities[Rng.RandiRange(0, unavailableAbilities.Count-1)], 1));
            }
        }
    }
}