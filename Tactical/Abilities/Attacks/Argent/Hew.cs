using System.Collections.Generic;
using System.Linq;
using Godot;

public class Hew : AbstractAbility{
    public static string id = "HEW";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 2;
    private static int min_range = 0;
    private static int max_range = 1;
    private static bool targetsLane = true;
    private static bool needsUnit = true;

    private Die atkDie = new Die(DieType.SLASH, 6, 16);

    public Hew(): base(
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
        this.BASE_DICE = new List<Die>{atkDie};
        this.TAGS.Add(AbilityTag.AOE);
    }
}