using System;
using System.Collections.Generic;

public class Obliterate : AbstractAbility, IEventHandler<CombatEventDieHit>, IEventHandler<CombatEventClashLose> {
    public static string id = "OBLITERATE";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 6;
    private static int min_range = 0;
    private static int max_range = 4;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die atkDieA = new Die(DieType.PIERCE, 6, 11, "PIERCE_PULL");
    private Die atkDieB = new Die(DieType.SLASH, 7, 9);
    private Die blkDieC = new Die(DieType.BLOCK, 7, 10);
    private Die atkDieD = new Die(DieType.BLUNT, 4, 8);
    private Die atkDieE = new Die(DieType.BLUNT, 9, 11);

    public Obliterate(): base(
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
        this.BASE_DICE = new List<Die>{atkDieA, atkDieB, blkDieC, atkDieD, atkDieE};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_DIE_HIT, this, CombatEventPriority.STANDARD);
    }

    public virtual void HandleEvent(CombatEventDieHit data){
        if (data.die == atkDieA){
            CombatManager.ExecuteAction(new PullAction(this.OWNER, data.hitUnit, 3));
            if (Math.Abs(this.OWNER.Position - data.hitUnit.Position) > 1){
                if (this == CombatManager.combatInstance.activeAbility){
                    CombatManager.combatInstance.activeAbilityDice.Clear();
                } else if (this == CombatManager.combatInstance.reactAbility){
                    CombatManager.combatInstance.activeAbilityDice.Clear();
                }
            }
        }
    }

    public virtual void HandleEvent(CombatEventClashLose data){
        if (data.losingDie == atkDieA){
            if (Math.Abs(this.OWNER.Position - data.winningClasher.Position) > 1){
                if (this == CombatManager.combatInstance.activeAbility){
                    CombatManager.combatInstance.activeAbilityDice.Clear();
                } else if (this == CombatManager.combatInstance.reactAbility){
                    CombatManager.combatInstance.activeAbilityDice.Clear();
                }
            }
        }
    }
}