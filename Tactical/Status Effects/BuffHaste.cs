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
        List<(AbstractCharacter character, int spd)> newSpeeds = new();
        foreach ((AbstractCharacter character, int spd) in CombatManager.combatInstance.turnlist.GetAllInstancesOfItem(this.OWNER)){
            newSpeeds.Add((character, spd + this.STACKS));
        }

        // Remove the old actions and add the new actions.
        // TODO: Seems hacky? Maybe we could just add the speed to all existing actions, then resort the turnlist?
        CombatManager.combatInstance.turnlist.RemoveAllInstancesOfItem(this.OWNER);
        foreach ((AbstractCharacter c, int spd) in newSpeeds){
            CombatManager.combatInstance.turnlist.AddToQueue(c, spd);
        }
        CombatManager.ExecuteAction(new RemoveStatusAction(this.OWNER, this));
    }
}