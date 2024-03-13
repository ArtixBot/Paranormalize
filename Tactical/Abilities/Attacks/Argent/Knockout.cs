using System.Collections.Generic;
using System.Linq;
using Godot;

public class Knockout : AbstractAbility, IEventHandler<CombatEventAbilityActivated>, IEventHandler<CombatEventRoundStart>{
    public static string id = "KNOCKOUT";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 4;
    private static int min_range = 0;
    private static int max_range = 1;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die bluntDieA = new Die(DieType.BLUNT, 4, 8);
    private Die bluntDieB = new Die(DieType.BLUNT, 4, 7);
    private Die bluntKnockout = new Die(DieType.BLUNT, 8, 10);

    private Dictionary<AbstractCharacter, int> characterToLanePos = new();

    public Knockout(): base(
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
        this.BASE_DICE = new List<Die>{bluntDieA, bluntDieB, bluntDieA};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_ROUND_START, this, CombatEventPriority.STANDARD);
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_ABILITY_ACTIVATED, this, CombatEventPriority.STANDARD);
    }

    public virtual void HandleEvent(CombatEventRoundStart data){
        characterToLanePos.Clear();
        foreach (AbstractCharacter character in CombatManager.combatInstance.fighters){
            characterToLanePos[character] = character.Position;
        }
    }

    public override void HandleEvent(CombatEventAbilityActivated data){
        base.HandleEvent(data);

        if (data.abilityActivated == this && data.target?.Position != characterToLanePos.GetValueOrDefault(data.target)){
            Logging.Log($"Target is in lane {data.target.Position} and started the round in lane {characterToLanePos[data.target]}, triggering Knockout's bonus die!", Logging.LogLevel.ESSENTIAL);
            data.abilityDice.Add(bluntKnockout);
        }
    }

}