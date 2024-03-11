using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class AbilityPass : AbstractAbility, IEventHandler<CombatEventAbilityActivated> {
    public static string id = "PASS";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 0;
    private static int min_range = 0;
    private static int max_range = 0;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    public AbilityPass(): base(
        id,
        strings,
        AbilityType.SPECIAL,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit,
        new HashSet<TargetingModifiers>{TargetingModifiers.SELF}
    ){}

    public override void HandleEvent(CombatEventAbilityActivated data){
        base.HandleEvent(data);
    }
}