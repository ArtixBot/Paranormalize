using Godot;
using System;
using System.Collections.Generic;

public enum StatusEffectType {BUFF = 1, DEBUFF = 2, CONDITION = 3}
public abstract class AbstractStatusEffect : IEventSubscriber{
    public string ID;
    public AbstractCharacter OWNER;
    public StatusEffectType TYPE;
    public int STACKS;
    public bool CAN_GAIN_STACKS = true;       // If false, once applied, this cannot gain additional stacks until the status is removed. Defaults to true.

    public virtual void InitSubscriptions(){}

    public virtual void HandleEvent(CombatEventData data){
        CombatEventStatusApplied eventData = (CombatEventStatusApplied) data;
        if (eventData.statusEffect != this) return;
    }
}