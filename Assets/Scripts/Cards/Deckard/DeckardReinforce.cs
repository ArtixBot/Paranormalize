using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckardReinforce : AbstractCard {

    public static string cardID = "DECKARD_REINFORCE";
    private static Dictionary<string, string> cardStrings = LocalizationLibrary.Instance.GetCardStrings(cardID);
    private static int cardCost = 1;

    private int multiplier = 2;

    public DeckardReinforce() : base(
        cardID,
        cardStrings,
        cardCost,
        CardAmbient.INFLUENCE,
        CardRarity.UNCOMMON,
        CardType.SKILL,
        new List<CardTags>{CardTags.SCOUR}
    ){}

    public override void Play(AbstractCharacter source, AbstractArgument target){
        base.Play(source, target);
        target.stacks *= this.multiplier;
    }

    public override void Upgrade(){
        base.Upgrade();
        this.multiplier += 1;
    }
}