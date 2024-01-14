using System.Collections.Generic;

public class Discharge : AbstractAbility {
    public static string id = "DISCHARGE";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 5;
    private static int min_range = 0;
    private static int max_range = 2;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die atkDie = new Die(DieType.PIERCE, 5, 5);

    public Discharge(): base(
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
        this.BASE_DICE = new List<Die>{atkDie, atkDie, atkDie, atkDie, atkDie, atkDie};
    }
}