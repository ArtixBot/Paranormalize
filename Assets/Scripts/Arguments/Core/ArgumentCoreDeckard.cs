using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEvent;

public class ArgumentCoreDeckard : AbstractArgument
{
    public ArgumentCoreDeckard(){
        this.ID = "CORE_DECKARD";
        Dictionary<string, string> strings = LocalizationLibrary.Instance.GetArgumentStrings(this.ID);
        this.NAME = strings["NAME"];
        this.DESC = strings["DESC"];
        this.ORIGIN = ArgumentOrigin.DEPLOYED;
        this.IMG = Resources.Load<Sprite>("Images/Arguments/composed");

        this.curHP = 30;
        this.maxHP = 30;
        this.stacks = 1;
        this.isCore = true;
    }

    public override void TriggerOnDeploy(){
        base.TriggerOnDeploy();
        EventSystemManager.Instance.SubscribeToEvent(this, EventType.TURN_START);
    }

    public override void NotifyOfEvent(AbstractEvent eventData){
        EventTurnStart data = (EventTurnStart) eventData;
        if (data.start == this.OWNER){
            NegotiationManager.Instance.AddAction(new ApplyPoiseAction(this.OWNER, this, this.stacks));
        }
    }
}