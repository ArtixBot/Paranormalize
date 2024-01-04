
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class AiBehavior {
    public readonly AbstractCharacter OWNER;

    public AiBehavior(AbstractCharacter OWNER){
        this.OWNER = OWNER;
    }
    /// <summary>
    /// Decide on an ability to use. The decision process should vary between implementations.
    /// </summary>
    public abstract void DecideAbilityToUse();
}

/// <summary>
/// Units with this behavior will pick a random ability on their turn, and also pick a random target for that ability.
/// </summary>
public class AiBehaviorPureRandom : AiBehavior {

    public AiBehaviorPureRandom(AbstractCharacter OWNER) : base(OWNER){}

    public override void DecideAbilityToUse(){
        List<AbstractAbility> usableAbilities = new();

        Dictionary<AbstractAbility, List<(int lane, HashSet<AbstractCharacter> targets)>> cache = new();
        // Go through all non-REACTION available abilities and determine whether we can use each one at the user's current position.
        foreach (AbstractAbility ability in this.OWNER.AvailableAbilities.Where(ability => ability.TYPE != AbilityType.REACTION && ability.TYPE != AbilityType.SPECIAL)){
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
}

/// <summary>
/// Generally used for backline units. Will prioritize attacking far-away enemies. If enemies are nearby, priority is split between attacking far-away enemies and getting away from nearby enemies.
/// </summary>
public class AiBehaviorBackline : AiBehavior {

    public AiBehaviorBackline(AbstractCharacter OWNER) : base(OWNER){}

    public override void DecideAbilityToUse(){
        return;
    }
}