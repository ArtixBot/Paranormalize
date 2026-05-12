using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Godot;

public class AbilityReposition : AbstractAbility, IEventHandler<CombatEventAbilityActivated>, IEventHandler<CombatEventRoundStart>, IEventHandler<CombatEventCombatStateChanged> {
    public static string id = "REPOSITION";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 0;
    private static int min_range = 1;
    private static int max_range = 1;
    private static bool targetsLane = true;
    private static bool needsUnit = false;

    public bool HAS_BEEN_ACTIVATED_THIS_ROUND = false;

    public AbilityReposition(): base(
        id,
        strings,
        AbilityType.SPECIAL,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit
    ){}

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_ROUND_START, this, CombatEventPriority.STANDARD);
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_COMBAT_STATE_CHANGE, this, CombatEventPriority.STANDARD);
    }

    public override void HandleEvent(CombatEventAbilityActivated data){
        base.HandleEvent(data);
        if (data.abilityActivated == this && data.lanes != null && data.lanes.Count == 1){
            if (data.lanes.First() > this.OWNER.Position){
                CombatManager.ExecuteAction(new ForwardAction(this.OWNER, null, 1));
            }
            else {
                CombatManager.ExecuteAction(new BackAction(this.OWNER, null, 1));
            }
        }
        if (!this.HAS_BEEN_ACTIVATED_THIS_ROUND){
            this.TAGS.Add(AbilityTag.CANTRIP);
            this.HAS_BEEN_ACTIVATED_THIS_ROUND = true;
        }
    }

    public virtual void HandleEvent(CombatEventRoundStart data){
        this.HAS_BEEN_ACTIVATED_THIS_ROUND = false;
    }

    public virtual void HandleEvent(CombatEventCombatStateChanged data){
        if (data.prevState == CombatState.RESOLVE_ABILITIES){
            this.TAGS.Remove(AbilityTag.CANTRIP);
        }
    }
}