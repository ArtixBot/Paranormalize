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

    private static readonly AbstractCharacter characterA = new AbstractCharacter("Player", CharacterFaction.PLAYER);
    private static readonly AbstractCharacter characterB = new AbstractCharacter("Test Dummy A", CharacterFaction.ENEMY);
    private static readonly AbstractCharacter characterC = new AbstractCharacter("Test Dummy B", CharacterFaction.ENEMY);
    private static readonly AbstractCharacter characterD = new AbstractCharacter("Test Dummy C", CharacterFaction.ENEMY);
    private static List<(AbstractCharacter character, int position)> scenarioFighters = new(){
        (characterA, 3),
        (characterB, 4),
        (characterC, 5),
        (characterD, 6),
    };

    public TestScenario() : base(scenarioFighters){
        characterA.EquipAbility(new TestReact());
        characterA.EquipAbility(new RelentlessStabbing());
        characterA.EquipAbility(new TestAttack());
        characterA.EquipAbility(new Discharge());
        characterA.EquipAbility(new Thwack());
        characterA.MinSpd = 10;
        characterA.MaxSpd = 10;

        characterB.ActionsPerTurn = 1;
        characterB.MaxHP = 100;
        characterB.CurHP = 100;
        characterB.MaxPoise = 10;
        characterB.EquipAbility(new Thwack());
        characterB.EquipAbility(new Discharge());
        characterB.Behavior = new AiBehaviorPureRandom(characterB);
        characterC.ActionsPerTurn = 0;
        characterD.ActionsPerTurn = 0;
    }
}