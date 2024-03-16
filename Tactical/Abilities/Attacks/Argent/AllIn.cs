using System.Collections.Generic;
using System.Linq;
using Godot;

public class AllIn : AbstractAbility, IEventHandler<CombatEventAbilityActivated>, IEventHandler<CombatEventDieRolled>, IEventHandler<CombatEventDieHit> {
    public static string id = "ALL_IN";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 6;
    private static int min_range = 0;
    private static int max_range = 0;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private int addedRoll = 0;

    private Die atkDie = new Die(DieType.BLUNT, 15, 20, "ALL_IN");

    public AllIn(): base(
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
        this.BASE_DICE = new List<Die>{atkDie};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_ABILITY_ACTIVATED, this, CombatEventPriority.STANDARD);
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_DIE_ROLLED, this, CombatEventPriority.STANDARD);
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_DIE_HIT, this, CombatEventPriority.STANDARD);
    }

    public override void HandleEvent(CombatEventAbilityActivated data){
        base.HandleEvent(data);

        if (data.abilityActivated == this){
            this.addedRoll = 0;

            foreach (AbstractAbility ability in this.OWNER.AvailableAbilities){
                // TODO: Replace this with a Combat Action that allows for exert actions to take place.
                ability.curCooldown = 2;
                this.addedRoll += 2;
            }
        }
    }

    public virtual void HandleEvent(CombatEventDieRolled data){
        if (data.die == atkDie){
            data.rolledValue += this.addedRoll;
            Logging.Log($"All In exerted {this.addedRoll / 2} abilities, increasing this die's roll by +{this.addedRoll} (now {data.rolledValue}).", Logging.LogLevel.ESSENTIAL);
        }
    }

    public virtual void HandleEvent(CombatEventDieHit data){
        if (data.die == atkDie && data.actualRoll >= 32){
            Logging.Log("All In triggers, causing a stagger!", Logging.LogLevel.ESSENTIAL);
            CombatManager.ExecuteAction(new ApplyStatusAction(data.hitUnit, new ConditionStaggered(), stacksToApply: 2));
        }
    }
}