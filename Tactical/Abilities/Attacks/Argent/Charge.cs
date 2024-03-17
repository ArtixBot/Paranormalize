using System;
using System.Collections.Generic;
using Godot;

public class Charge : AbstractAbility, IEventHandler<CombatEventAbilityActivated>, IEventHandler<CombatEventCharacterDeath>, IEventHandler<CombatEventCombatStateChanged> {
    public static string id = "CHARGE";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int MOVE_DISTANCE = 2;
    private static int cd = 5;
    private static int min_range = 0;
    private static int max_range = 2;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private AbstractCharacter target;
    private bool targetKilled;

    private Die atkDieA = new Die(DieType.BLUNT, 14, 26);

    public Charge(): base(
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
        this.BASE_DICE = new List<Die>{atkDieA};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_ABILITY_ACTIVATED, this, CombatEventPriority.STANDARD);
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_COMBAT_STATE_CHANGE, this, CombatEventPriority.STANDARD);
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_CHARACTER_DEATH, this, CombatEventPriority.FINAL);
    }

    public override void HandleEvent(CombatEventAbilityActivated data){
        base.HandleEvent(data);
        if (data.abilityActivated == this){
            this.target = data.target;
            this.targetKilled = false;
            CombatManager.ExecuteAction(new ForwardAction(this.OWNER, data.target, MOVE_DISTANCE));
        }
    }

    public virtual void HandleEvent(CombatEventCharacterDeath data){
        if (data.deadChar == this.target){
            this.TAGS.Add(AbilityTag.CANTRIP);
            this.curCooldown = 0;
            this.targetKilled = true;
        }
    }

    public virtual void HandleEvent(CombatEventCombatStateChanged data){
        this.target = null;
        if (data.prevState == CombatState.RESOLVE_ABILITIES && this.targetKilled){
            this.TAGS.Remove(AbilityTag.CANTRIP);
        }
    }
}