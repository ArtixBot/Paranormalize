using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class AbilityPass : AbstractAbility {
    public static string id = "PASS";
    // TODO: Make all of these read in by JSON.
    private static string name = "Pass";
    private static string desc = "Skip this unit's turn.";

    // TODO: Should gameplay attributes also be defined in JSON? e.g. base CD, min range, max range, dice, etc...
    private static int cd = 0;
    private static int min_range = 0;
    private static int max_range = 0;

    public AbilityPass(): base(
        id,
        name,
        desc,
        AbilityType.SPECIAL,
        cd,
        min_range,
        max_range,
        new List<TargetingModifiers>{TargetingModifiers.SELF}
    ){}

    public override void Activate(CombatEventAbilityActivated data){
        base.Activate(data);
        this.OWNER.Position = data.lanes[0];
    }
}