using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class TestReact : AbstractAbility {
    public static string id = "TEST_REACT";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 1;
    private static int min_range = 0;
    private static int max_range = 0;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    public TestReact(): base(
        id,
        strings,
        AbilityType.REACTION,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit
    ){
        Die atkDieA = new Die(DieType.BLOCK, 40, 60);
        this.BASE_DICE = new List<Die>{atkDieA, atkDieA};
    }
}