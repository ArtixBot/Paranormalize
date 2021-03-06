using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeckard : AbstractCharacter
{
    public PlayerDeckard(){
        this.NAME = "Deckard";
        this.FACTION = FactionType.PLAYER;
        this.coreArgument = new ArgumentCoreDeckard();
        this.coreArgument.OWNER = this;
        this.maxAP = 3;

        this.AddStarterDeck();
    }

    public override void AddStarterDeck(){
        this.AddCardToPermaDeck("DECKARD_DIPLOMACY");
        // this.AddCardToPermaDeck("DECKARD_BARREL_THROUGH");
        // this.AddCardToPermaDeck("DECKARD_INSTINCTS_FINESSE");
        // this.AddCardToPermaDeck("DECKARD_INSTINCTS_HEATED");
        // this.AddCardToPermaDeck("DECKARD_INSTINCTS");
        // this.AddCardToPermaDeck("DECKARD_REVERSAL");
        // this.AddCardToPermaDeck("DECKARD_BRASH");
        // this.AddCardToPermaDeck("DECKARD_GOOD_IMPRESSION");
        // this.AddCardToPermaDeck("DECKARD_GRANDIOSITY");
        // this.AddCardToPermaDeck("DECKARD_INSULT");
        // this.AddCardToPermaDeck("DECKARD_SMOKE_BREAK");
        this.AddCardToPermaDeck("DECKARD_RAPID_FIRE");
        // this.AddCardToPermaDeck("DECKARD_RAPID_FIRE");
        // this.AddCardToPermaDeck("DECKARD_BURST_OF_ANGER");
        // this.AddCardToPermaDeck("DECKARD_AUTHORITATIVE", true);
        // this.AddCardToPermaDeck("DECKARD_WORDSMITH");
        // this.AddCardToPermaDeck("DECKARD_PRESENT_THE_EVIDENCE");
        // this.AddCardToPermaDeck("DECKARD_STAY_COOL");
        // this.AddCardToPermaDeck("DECKARD_BULLDOZE");
        // this.AddCardToPermaDeck("DECKARD_FLATTER");
        // this.AddCardToPermaDeck("DECKARD_STOIC");
        // this.AddCardToPermaDeck("DECKARD_CURVEBALL");
        // this.AddCardToPermaDeck("DECKARD_KNACK");
        // this.AddCardToPermaDeck("DECKARD_MAGNETIC_PERSONALITY");
        // this.AddCardToPermaDeck("DECKARD_FILIBUSTER");
        // this.AddCardToPermaDeck("DECKARD_REINFORCE");
        // this.AddCardToPermaDeck("DECKARD_COMMAND");
        // this.AddCardToPermaDeck("DECKARD_IMPATIENCE");
        this.AddCardToPermaDeck("DECKARD_SIMMER");
        // this.AddCardToPermaDeck("DECKARD_DEFUSAL");
        // this.AddCardToPermaDeck("DECKARD_CROSS_EXAMINATION");
        // this.AddCardToPermaDeck("DECKARD_SMOOTH_TALK");
        // this.AddCardToPermaDeck("DECKARD_FOLLOW_UP");
        this.AddCardToPermaDeck("DECKARD_DECISIVE_ACTION");
        this.AddCardToPermaDeck("DECKARD_OVERWHELM");
        // this.AddCardToPermaDeck("DECKARD_OPENING_OFFER");
        // this.AddCardToPermaDeck("DECKARD_ADAPTIVE");
        // this.AddCardToPermaDeck("DECKARD_RUMINATE");
        // this.AddCardToPermaDeck("DECKARD_DEEP_BREATH");
        // this.AddCardToPermaDeck("DECKARD_GUARDED_RESPONSE");
        // this.AddCardToPermaDeck("DECKARD_BREAKTHROUGH");
        // this.AddCardToPermaDeck("DECKARD_BREAKTHROUGH");
        // this.AddCardToPermaDeck("DECKARD_FOLLOW_UP");
        // this.AddCardToPermaDeck("DECKARD_RUMINATE");
        // this.AddCardToPermaDeck("DECKARD_RUMINATE");
        // this.AddCardToPermaDeck("DECKARD_RUMINATE");
    }
}