using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class TestReact : AbstractAbility {
    public static string id = "BASE_REACT";
    // TODO: Make all of these read in by JSON.
    private static string name = "Base React";
    private static string desc = "";

    // TODO: Should gameplay attributes also be defined in JSON? e.g. base CD, min range, max range, dice, etc...
    private static int cd = 1;
    private static int min_range = 0;
    private static int max_range = 0;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    public TestReact(): base(
        id,
        name,
        desc,
        AbilityType.REACTION,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit
    ){
        Die atkDieA = new Die(DieType.BLUNT, 4, 6);
        Die atkDieB = new Die(DieType.PIERCE, 4, 6);
        Die defDieA = new Die(DieType.BLOCK, 3, 8);
        Die defDieB = new Die(DieType.EVADE, 1, 11);
        this.BASE_DICE = new List<Die>{atkDieA, atkDieB, defDieA, defDieB};
    }
}