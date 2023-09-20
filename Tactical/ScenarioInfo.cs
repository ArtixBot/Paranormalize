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

    private static readonly AbstractCharacter characterA = new AbstractCharacter("Player");
    private static readonly AbstractCharacter characterB = new AbstractCharacter("Enemy");
    private static List<(AbstractCharacter character, int position)> scenarioFighters = new(){
        (characterA, 1),
        (characterB, 5)
    };

    public TestScenario() : base(scenarioFighters){
        characterA.EquipAbility(new TestAttack());
        characterA.EquipAbility(new TestReact());
    }
}