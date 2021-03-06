using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckardHotheaded : AbstractCard
{
    public static string cardID = "DECKARD_HOTHEADED";
    private static Dictionary<string, string> cardStrings = LocalizationLibrary.Instance.GetCardStrings(cardID);
    private static int cardCost = 1;

    public int STACKS = 2;

    public DeckardHotheaded() : base(
        cardID,
        cardStrings,
        cardCost,
        CardAmbient.AGGRESSION,
        CardRarity.UNCOMMON,
        CardType.TRAIT
    ){}

    public override void Play(AbstractCharacter source, AbstractArgument target){
        base.Play(source, target);
        NegotiationManager.Instance.AddAction(new DeployArgumentAction(source, new ArgumentHotheaded(), STACKS));
    }

    public override void Upgrade(){
        base.Upgrade();
        this.COST -= 1;
    }
}
