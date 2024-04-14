using Godot;
using System;
using System.Collections.Generic;

public class ConditionDishonorable : AbstractStatusEffect{

    public static string id = "DISHONORABLE";
    private static Localization.EffectStrings strings = Localization.LocalizationLibrary.Instance.GetEffectStrings(id);

    public ConditionDishonorable() : base(
        id,
        strings,
        StatusEffectType.CONDITION,
        CAN_GAIN_STACKS: false
    ){ }
}