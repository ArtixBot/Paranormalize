using System.Collections.Generic;
using System.Linq;
using Localization;
namespace CharacterPassives;

public class SacredDuel : AbstractPassive, IEventHandler<CombatEventCombatStart> {

    public static readonly string id = "SACRED_DUEL";
    public static readonly int cost = 1;
    private static readonly PassiveStrings strings = LocalizationLibrary.Instance.GetPassiveStrings(id);
    
    public SacredDuel() : base(id, strings, cost){}

    public override void InitSubscriptions() {
        CombatManager.eventManager.Subscribe(CombatEventType.ON_COMBAT_START, this, CombatEventPriority.STANDARD);
    }

    public void HandleEvent(CombatEventCombatStart data){
        HashSet<AbstractCharacter> enemies = CombatManager.combatInstance.fighters.Where(fighter => fighter.CHAR_FACTION != this.OWNER.CHAR_FACTION).ToHashSet();
        HashSet<AbstractCharacter> enemiesWithoutDD = enemies.Where(fighter => !fighter.statusEffects.Contains(fighter.statusEffects.Find(effect => effect.ID == "DUEL_TO_THE_DEATH"))).ToHashSet();

        AbstractCharacter target = enemiesWithoutDD.ElementAt(Rng.RandiRange(0, enemiesWithoutDD.Count - 1));
        CombatManager.ExecuteAction(new ApplyStatusAction(target, new ConditionDuelToTheDeath(this.OWNER, target), 1));
    }
}