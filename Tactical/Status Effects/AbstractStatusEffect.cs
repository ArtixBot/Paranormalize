using Godot;
using System;
using System.Collections.Generic;

public enum StatusEffectType {BUFF = 1, DEBUFF = 2, CONDITION = 3}
public abstract class AbstractStatusEffect : IEventSubscriber{
    public string ID;
    public string NAME;
    public string DESC;
    public AbstractCharacter OWNER;
    public StatusEffectType TYPE;
    public int STACKS;
    public bool CAN_GAIN_STACKS = true;       // If false, once applied, this cannot gain additional stacks until the status is removed. Defaults to true.

    public AbstractStatusEffect(string ID, Localization.EffectStrings EFFECT_STRINGS, StatusEffectType TYPE, bool CAN_GAIN_STACKS = true){
        this.ID = ID;
        this.NAME = EFFECT_STRINGS.NAME;
        this.DESC = EFFECT_STRINGS.DESC;
        this.TYPE = TYPE;
        this.CAN_GAIN_STACKS = CAN_GAIN_STACKS;
    }

    public virtual void InitSubscriptions(){}
}