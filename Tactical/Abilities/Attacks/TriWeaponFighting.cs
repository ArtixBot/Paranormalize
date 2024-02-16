using System;
using System.Collections.Generic;

public class TriWeaponFighting : AbstractAbility, IEventHandler<CombatEventDieHit>, IEventHandler<CombatEventClashLose> {
    public static string id = "TRI_WEAPON_FIGHTING";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 2;
    private static int min_range = 0;
    private static int max_range = 4;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die atkDieA = new Die(DieType.PIERCE, 6, 11, "PIERCE_PULL");
    private Die atkDieB = new Die(DieType.SLASH, 4, 7);
    private Die atkDieC = new Die(DieType.BLUNT, 4, 6);

    public TriWeaponFighting(): base(
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
        this.BASE_DICE = new List<Die>{atkDieA, atkDieB, atkDieC};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_DIE_HIT, this, CombatEventPriority.STANDARD);
    }

    public virtual void HandleEvent(CombatEventDieHit data){
        if (data.die.Equals(atkDieA)){
            CombatManager.ExecuteAction(new PullAction(this.OWNER, data.hitUnit, 1));
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
        if (data.losingDie.Equals(atkDieA)){
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