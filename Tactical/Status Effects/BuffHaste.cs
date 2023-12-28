using Godot;
using System;
using System.Collections.Generic;

public class BuffHaste : AbstractStatusEffect{
    public BuffHaste(){
        this.ID = "HASTE";
        this.TYPE = StatusEffectType.BUFF;
    }

    public override void InitSubscriptions(){
        CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_START, this, CombatEventPriority.STANDARD);
    }

    public override void HandleEvent(CombatEventData data){
        CombatManager.combatInstance.turnlist.ModifyItemPriority(this.OWNER, this.STACKS);
        CombatManager.ExecuteAction(new RemoveStatusAction(this.OWNER, this));
    }
}