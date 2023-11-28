using Godot;

public class RecoverPoiseAction : AbstractAction {

    private AbstractCharacter character;
    private int recoveredPoise;

    public RecoverPoiseAction(AbstractCharacter character, int recoveredPoise){
        this.character = character;
        this.recoveredPoise = recoveredPoise;
    }

    public override void Execute(){
        // TODO: CombatEventManager.BroadcastEvent(new CombatEventPoiseRecovered);
        this.character.CurPoise += this.recoveredPoise;     // No need to do a max check, CurPoise's setter automatically checks for this.
    }
}