using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RemoveCombatantAction : AbstractAction {

    private AbstractCharacter target;

    public RemoveCombatantAction(AbstractCharacter target){
        this.target = target;
    }

    public override void Execute(){
        CombatInstance data = CombatManager.combatInstance;
        if (data == null) return;
        // In a resolve ability state, immediately clear the remaining dice queue if no targets are remaining (an AoE attack continues unless ALL units are dead).
        // Do nothing for all other states other than standard unsubscribes.
        if (data.combatState == CombatState.RESOLVE_ABILITIES){
            data.activeAbilityTargets?.Remove(this.target);      // Remove dead char from targets list.
            // If targets list is empty, or if the dead character was the attacker, immediately clear dice queue. This will effectively move force a post-resolve state.
            if (data.activeAbilityTargets?.Count == 0 || this.target == data.activeChar){
                data.activeAbilityDice?.Clear();
                data.reactAbilityDice?.Clear();
            }
        }

        // Handle on-death effects.
        CombatManager.eventManager?.BroadcastEvent(new CombatEventCharacterDeath(this.target));
        // TODO: Unsubscribe all of the fighter's abilities and passives as well.
        CombatManager.eventManager?.UnsubscribeAll(this.target);

        data?.fighters.Remove(this.target);
        bool playersRemaining = data.fighters.Where(fighter => fighter.CHAR_FACTION == CharacterFaction.PLAYER).ToHashSet().Count > 0;
        bool enemiesRemaining = data.fighters.Where(fighter => fighter.CHAR_FACTION == CharacterFaction.ENEMY).ToHashSet().Count > 0;

        if (!enemiesRemaining || !playersRemaining) {
            CombatManager.ChangeCombatState(CombatState.COMBAT_END);
        }
    }
}