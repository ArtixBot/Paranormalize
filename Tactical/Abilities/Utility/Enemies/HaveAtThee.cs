using System.Collections.Generic;
using System.Linq;
using Godot;

public class HaveAtThee : AbstractAbility, IEventHandler<CombatEventAbilityActivated> {
    public static string id = "HAVE_AT_THEE";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 1;
    private static int min_range = 0;
    private static int max_range = 5;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    // Override IsActivatable - this ability cannot be used while characters have the Dishonorable effect.
    // TODO: Better UI handling for when this is available, but not activatable.
    public override bool IsActivatable {
        get {
            HashSet<AbstractCharacter> fighters = CombatManager.combatInstance?.fighters;
            foreach(AbstractCharacter fighter in fighters){
                if (fighter == this.OWNER && this.OWNER.HasCondition("DUEL_TO_THE_DEATH")) return false;
                if (fighter.statusEffects.Find(effect => effect.ID == "DISHONORABLE") != default){
                    return false;
                }
            }
            return IsAvailable;
        }
    }

    public override List<(int lane, HashSet<AbstractCharacter> targetsInLane)> GetValidTargets(){
        List<(int lane, HashSet<AbstractCharacter> targetsInLane)> targets = base.GetValidTargets();

        List<(int, HashSet<AbstractCharacter>)> filtered = new();
        foreach ((int lane, HashSet<AbstractCharacter> targetsInLane) in targets){
            filtered.Add((lane, targetsInLane.Where(target => !target.HasCondition("DUEL_TO_THE_DEATH")).ToHashSet()));
        }
        return filtered;
    }

    public HaveAtThee(): base(
        id,
        strings,
        AbilityType.UTILITY,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit,
        new HashSet<TargetingModifiers>{TargetingModifiers.ENEMIES_ONLY}
    ){
    }

    public override void HandleEvent(CombatEventAbilityActivated data){
        base.HandleEvent(data);
        if (data.abilityActivated.Equals(this)){
            CombatManager.ExecuteAction(new ApplyStatusAction(this.OWNER, new ConditionDuelToTheDeath(data.target, this.OWNER), 1));
            CombatManager.ExecuteAction(new ApplyStatusAction(data.target, new ConditionDuelToTheDeath(this.OWNER, data.target), 1));
        }
    }
}