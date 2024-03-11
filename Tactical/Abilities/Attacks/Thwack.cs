using System.Collections.Generic;

public class Thwack : AbstractAbility {
    public static string id = "THWACK";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 5;
    private static int min_range = 0;
    private static int max_range = 1;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die atkDieA = new Die(DieType.BLUNT, 3, 4);
    private Die atkDieB = new Die(DieType.SLASH, 2, 6);

    public Thwack(): base(
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
}