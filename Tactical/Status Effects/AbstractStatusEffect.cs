using Godot;
using System;
using System.Collections.Generic;

public enum StatusEffectType {BUFF = 1, DEBUFF = 2, CONDITION = 3}
public abstract class AbstractStatusEffect : IEventSubscriber{
    public string ID;
    public StatusEffectType TYPE;
    public int STACKS;

    public virtual void InitSubscriptions(){}

    public virtual void HandleEvent(CombatEventData data){
        CombatEventStatusApplied eventData = (CombatEventStatusApplied) data;
        if (eventData.statusEffect != this) return;
    }
}