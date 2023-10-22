using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class TestAttack : AbstractAbility {
    public static string id = "BASE_ATTACK";
    // TODO: Make all of these read in by JSON.
    private static string name = "Base Attack";
    private static string desc = "";

    // TODO: Should gameplay attributes also be defined in JSON? e.g. base CD, min range, max range, dice, etc...
    private static int cd = 1;
    private static int min_range = 0;
    private static int max_range = 1;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    public TestAttack(): base(
        id,
        name,
        desc,
        AbilityType.ATTACK,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit,
        new HashSet<TargetingModifiers>{TargetingModifiers.ENEMIES_ONLY}
    ){
        Die atkDieA = new Die(DieType.MELEE, 3, 5);
        this.BASE_DICE = new List<Die>{atkDieA};
    }
}