using System.Collections.Generic;
using System.Linq;
using Godot;

public class VirtuousStruggle : AbstractAbility, IEventHandler<CombatEventAbilityActivated>{
    public static string id = "VIRTUOUS_STRUGGLE";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 0;
    private static int min_range = 0;
    private static int max_range = 2;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die bluntDie = new Die(DieType.BLUNT, 5, 7);
    private Die bluntDieBonus = new Die(DieType.BLUNT, 4, 5);

    public VirtuousStruggle(): base(
        id,
        strings,
        AbilityType.ATTACK,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit,
        new HashSet<TargetingModifiers>{TargetingModifiers.ENEMIES_ONLY}
    ){
        this.BASE_DICE = new List<Die>{bluntDie};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_ABILITY_ACTIVATED, this, CombatEventPriority.STANDARD);
    }

    public override void HandleEvent(CombatEventAbilityActivated data){
        base.HandleEvent(data);
        if (data.abilityActivated == this){
            if (CombatManager.combatInstance.fighters.Where(character => character.CHAR_FACTION == this.OWNER.CHAR_FACTION).Count() == 1){
                CombatManager.ExecuteAction(new ApplyStatusAction(this.OWNER, new ConditionNextRoundStatusGain(new BuffStrength()), 1));
                data.abilityDice.Add(bluntDieBonus);
                data.abilityDice.Add(bluntDieBonus);
            }
        }
    }
}