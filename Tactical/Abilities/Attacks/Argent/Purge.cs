using System.Collections.Generic;
using System.Linq;
using Godot;

public class Purge : AbstractAbility{
    public static string id = "PURGE";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 4;
    private static int min_range = 0;
    private static int max_range = 2;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die atkDieA = new Die(DieType.SLASH, 6, 16);
    private Die atkDieB = new Die(DieType.SLASH, 9, 12);
    private Die atkDieC = new Die(DieType.SLASH, 5, 13);

    public Purge(): base(
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
        this.BASE_DICE = new List<Die>{atkDieA, atkDieB, atkDieC};
    }
}