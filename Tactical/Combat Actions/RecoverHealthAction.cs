public class RecoverHealthAction : AbstractAction {

    private AbstractCharacter character;
    private int recoveredHealth;

    public RecoverHealthAction(AbstractCharacter character, int recoveredHealth){
        this.character = character;
        this.recoveredHealth = recoveredHealth;
    }

    public override void Execute(){
        // TODO: CombatEventManager.BroadcastEvent(new CombatEventHealthRecovered);
        this.character.CurHP += this.recoveredHealth;     // No need to do a max check, CurHealth's setter automatically checks for this.
    }
}