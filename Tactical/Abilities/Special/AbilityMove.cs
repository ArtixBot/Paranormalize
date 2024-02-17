using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Godot;

public class AbilityMove : AbstractAbility, IEventHandler<CombatEventAbilityActivated> {
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

    public override void HandleEvent(CombatEventAbilityActivated data){
        base.HandleEvent(data);
        if (data.abilityActivated == this && data.lanes != null && data.lanes.Count == 1){
            this.OWNER.Position = data.lanes[0];
        }
    }
}