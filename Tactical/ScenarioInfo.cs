using CharacterPassives;
using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// When initiating a combat encounter, the ScenarioInfo provides all of the information necessary to the CombatManager.
/// </summary>
public partial class ScenarioInfo {
    public List<(AbstractCharacter character, int startingPosition)> fighters;

    public ScenarioInfo(List<(AbstractCharacter character, int startingPosition)> fighters){
        this.fighters = fighters;
    }
}

public class TestScenario : ScenarioInfo {

    private static readonly AbstractCharacter playerA = new AbstractCharacter("Duelist", CharacterFaction.PLAYER);
    private static readonly AbstractCharacter playerB = new AbstractCharacter("Cinq", CharacterFaction.PLAYER);
    private static readonly AbstractCharacter characterB = new AbstractCharacter("Test Dummy A", CharacterFaction.ENEMY);
    private static readonly AbstractCharacter characterC = new AbstractCharacter("Test Dummy B", CharacterFaction.ENEMY);
    private static readonly AbstractCharacter characterD = new AbstractCharacter("Test Dummy C", CharacterFaction.ENEMY);
    private static readonly AbstractCharacter characterE = new AbstractCharacter("Test Dummy D", CharacterFaction.ENEMY);
    private static List<(AbstractCharacter character, int position)> scenarioFighters = new(){
        (playerA, 3),
        // (playerB, 2),
        (characterB, 4),
        // (characterC, 5),
        // (characterD, 1),
        // (characterE, 2),
    };

    public TestScenario() : base(scenarioFighters){
        playerA.EquipAbility(new RelentlessStabbing());
        playerA.EquipAbility(new Obliterate());
        playerA.EquipAbility(new Beatdown());
        playerA.EquipAbility(new GallopingTilt());
        playerA.EquipAbility(new Challenge());
        playerA.EquipAbility(new HaveAtThee());
        playerA.EquipAbility(new Brutalize());
        playerA.EquipAbility(new Charge());
        playerA.EquipAbility(new Purge());
        playerA.EquipAbility(new SecondWind());
        playerA.EquipAbility(new Indomitable());
        playerA.EquipAbility(new Knockout());
        playerA.EquipAbility(new ShrugOff());
        playerA.EquipAbility(new VirtuousStruggle());
        playerA.EquipAbility(new Parry());

        playerA.EquipPassive(new SacredDuel());
        playerA.EquipPassive(new DeathBeforeDishonor());
        playerA.EquipPassive(new CullTheDishonorable());
        playerA.EquipPassive(new InMemoriam());
        playerA.MinSpd = 10;
        playerA.MaxSpd = 20;
        playerA.ActionsPerTurn = 3;
        playerA.MaxHP = 100;
        playerA.CurHP = 68;
        playerA.MaxPoise = 100;
        playerA.CurPoise = 68;

        playerB.EquipAbility(new Discharge());
        playerB.EquipAbility(new Repartee());
        playerB.MinSpd = 10;
        playerB.MaxSpd = 30;

        characterB.MaxHP = 100;
        characterB.CurHP = 100;
        characterB.MaxPoise = 60;
        characterB.CurPoise = 60;
        characterB.MaxSpd = 1;
        characterB.EquipAbility(new Thwack());
        characterB.EquipAbility(new Parry());
        // characterB.EquipAbility(new Discharge());
        characterB.ActionsPerTurn = 1;
        characterB.Behavior = new AiBehaviorPureRandom(characterB);
        characterC.ActionsPerTurn = 0;
        characterC.MaxHP = 100;
        characterC.CurHP = 100;
        characterD.ActionsPerTurn = 0;
        characterE.ActionsPerTurn = 0;
    }
}