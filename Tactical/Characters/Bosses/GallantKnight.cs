using CharacterPassives;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Character;

public partial class GallantKnight : AbstractCharacter {
    private static readonly CharacterFaction faction = CharacterFaction.ENEMY;
    private static readonly string name = "Gallant Knight";

    private static readonly int maxHP = 100;
    private static readonly int maxPoise = 38;

    private static readonly int minSpd = 4;
    private static readonly int maxSpd = 7;

    public GallantKnight() : base(maxHP, maxPoise, minSpd, maxSpd, faction, name){
        EquipAbility(new GallantSlashes());
        EquipAbility(new GallopingTilt());
        EquipAbility(new Parry());
        EquipAbility(new HaveAtThee());
        EquipAbility(new VirtuousStruggle());
        EquipAbility(new Challenge());

        EquipPassive(new SacredDuel());
        EquipPassive(new DeathBeforeDishonor());
        EquipPassive(new CullTheDishonorable());
        EquipPassive(new InMemoriam());
    }
}