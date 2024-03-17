public class RestoreAction : AbstractAction {

    public enum RestoreType {
        HEALTH,
        POISE
    };

    // Used for percentage-based recovery.
    public enum RestorePercentType {
        PERCENTAGE_MAX,     // If character has 50/100 HP and has 25% recovery, the character recovers 100 * (0.25) = 25 HP.
        PERCENTAGE_MISSING  // If character has 50/100 HP and has 25% recovery, the character recovers (100 - 50) * (0.25) = 12.5HP, but always floor the result.
    };

    private AbstractCharacter character;
    private int restoreAmount;
    private RestoreType restoreType;
    private RestorePercentType? restorePercentType;

    public RestoreAction(AbstractCharacter character, int restoreAmount, RestoreType restoreType){
        this.character = character;
        this.restoreAmount = restoreAmount;
        this.restoreType = restoreType;
    }

    public RestoreAction(AbstractCharacter character, float restorePercentage, RestoreType restoreType, RestorePercentType restorePercentType){
        this.character = character;
        this.restoreType = restoreType;
        this.restorePercentType = restorePercentType;

        int amountToRestore;
        if (restorePercentType == RestorePercentType.PERCENTAGE_MAX){
            amountToRestore = (restoreType == RestoreType.HEALTH) ? (int)(character.MaxHP * restorePercentage) : (int)(character.MaxPoise * restorePercentage);
        } else {
            amountToRestore = (restoreType == RestoreType.HEALTH) ? (int)((character.MaxHP - character.CurHP) * restorePercentage) : (int)((character.MaxPoise - character.CurPoise) * restorePercentage);
        }

        this.restoreAmount = amountToRestore;
    }

    public override void Execute(){
        if (this.restoreType == RestoreType.HEALTH){
            // TODO: CombatEventManager.BroadcastEvent(new CombatEventHealthRestored);
            this.character.CurHP += this.restoreAmount;      // No need to do a max check, CurHealth's setter automatically checks for this.
        } else {
            // TODO: CombatEventManager.BroadcastEvent(new CombatEventPoiseRestored);
            this.character.CurPoise += this.restoreAmount;
        }
    }
}