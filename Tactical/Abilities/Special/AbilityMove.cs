using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Godot;

public class AbilityMove : AbstractAbility {
    public static string id = "MOVE";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 0;
    private static int min_range = 1;
    private static int max_range = 1;
    private static bool targetsLane = true;
    private static bool needsUnit = false;

    public AbilityMove(): base(
        id,
        strings,
        AbilityType.SPECIAL,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit
    ){}

    public override void Activate(CombatEventAbilityActivated data){
        base.Activate(data);
        if (data.lanes != null && data.lanes.Count == 1){
            this.OWNER.Position = data.lanes[0];
        }
    }
}