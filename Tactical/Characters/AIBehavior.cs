
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class AiBehavior {
    public readonly AbstractCharacter OWNER;
    public List<AbstractAbility> reactions = new();

    public AiBehavior(AbstractCharacter OWNER){
        this.OWNER = OWNER;
    }
    /// <summary>
    /// Decide on an ability to use. The decision process should vary between implementations.
    /// </summary>
    public abstract void DecideAbilityToUse();

    /// <summary>
    /// Decide on what abilities will be used as reactions this round.
    /// </summary>
    public abstract void DecideReactions();
}

/// <summary>
/// Units with this behavior:<br/>
/// - Pick random abilities on their turn, targeting a random eligible unit/lane.<br/>
/// - Pick random reactions on round start. They will avoid picking the same reaction multiple times in a round if it has a cooldown greater than zero.
/// </summary>
public class AiBehaviorPureRandom : AiBehavior {

    public AiBehaviorPureRandom(AbstractCharacter OWNER) : base(OWNER){}

    public override void DecideAbilityToUse(){
        List<AbstractAbility> usableAbilities = new();

        Dictionary<AbstractAbility, List<(int lane, HashSet<AbstractCharacter> targets)>> cache = new();
        // Go through all non-REACTION activatable abilities and determine whether we can use each one at the user's current position.
        foreach (AbstractAbility ability in this.OWNER.ActivatableAbilities.Where(ability => ability.TYPE != AbilityType.REACTION && ability.TYPE != AbilityType.SPECIAL)){
            List<(int lane, HashSet<AbstractCharacter> targets)> targets = ability.GetValidTargets();
            if (targets.Count == 0) {continue;}

            usableAbilities.Add(ability);
            cache[ability] = targets;           // Store the results for further use in this function w/o having to make another call.
        }

        if (usableAbilities.Count == 0) {
            CombatManager.InputAbility(OWNER.abilities.Where(ability => ability.ID == "PASS").FirstOrDefault(), new List<AbstractCharacter>{this.OWNER});
            return;
        }
        AbstractAbility abilityToUse = usableAbilities[Rng.RandiRange(0, usableAbilities.Count - 1)];
        Logging.Log($"{this.OWNER.CHAR_NAME} activates {abilityToUse.NAME}.", Logging.LogLevel.ESSENTIAL);

        List<(int lane, HashSet<AbstractCharacter> targets)> targeting = cache[abilityToUse];
        if (!abilityToUse.useLaneTargeting) {
            HashSet<AbstractCharacter> charTargets = new();
            foreach ((int _, HashSet<AbstractCharacter> targetSet) in targeting){
                charTargets.UnionWith(targetSet);
            }
            AbstractCharacter chosenChar = charTargets.ToList()[Rng.RandiRange(0, charTargets.Count - 1)];
            CombatManager.InputAbility(abilityToUse, new List<AbstractCharacter>{chosenChar});
        } else {
            List<int> laneTargets = targeting.Select(_ => _.lane).ToList();
            int laneChosen = laneTargets[Rng.RandiRange(0, laneTargets.Count - 1)];
            CombatManager.InputAbility(abilityToUse, new List<int>{laneChosen});
        }
    }

    public override void DecideReactions() {
        List<AbstractAbility> plannedReactions = new();

        // If an ability is chosen for a reaction, add it here so that the ability is not chosen for a subsequent reaction. This does not apply to abilities with an inherent zero cooldown.
        HashSet<AbstractAbility> doNotPlanSameReaction = new();

        for (int i = 0; i < this.OWNER.ActionsPerTurn; i++){
            // Choose random ATTACK-class and REACTION-class abilities for reaction abilities.
            List<AbstractAbility> activatableReactions = this.OWNER.ActivatableAbilities.Where(ability => ability.TYPE != AbilityType.UTILITY 
                                                                                                        && ability.TYPE != AbilityType.SPECIAL
                                                                                                        && !doNotPlanSameReaction.Contains(ability)).ToList();
            if (activatableReactions.Count <= 0) break;
            AbstractAbility abilityToReact = activatableReactions[Rng.RandiRange(0, activatableReactions.Count - 1)];

            plannedReactions.Add(abilityToReact);
            if (abilityToReact.BASE_CD != 0) {
                doNotPlanSameReaction.Add(abilityToReact);
            }
        }

        this.reactions = plannedReactions;
    }
}

/// <summary>
/// Generally used for backline units. Will prioritize attacking far-away enemies. If enemies are nearby, priority is split between attacking far-away enemies and getting away from nearby enemies.
/// </summary>
public class AiBehaviorBackline : AiBehavior {

    public AiBehaviorBackline(AbstractCharacter OWNER) : base(OWNER){}

    public override void DecideAbilityToUse(){
        return;
    }

    public override void DecideReactions(){
        throw new NotImplementedException();
    }
}