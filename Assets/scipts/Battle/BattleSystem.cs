using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//states to change the current process of the battle
public enum BattleState {Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver, TrainerSwitch, MoveToForget}
public enum BattleAction {Move, SwitchMonster, UseItem, Run}

public class BattleSystem : MonoBehaviour
{
    //references for the battle objects
    [SerializeField] BattleUnit playerBattleUnit;
    [SerializeField] BattleUnit enemyBattleUnit;
    [SerializeField] BattleDialog dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject cageCaptureSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;

    //event to end the battle
    public event Action<bool> OnBattleOver;

    //state variable and ints to hold the users current ""
    BattleState state;
    BattleState? prevState;
    //current "_"
    int curAction;
    int curMove;
    int curMember;
    bool trainerSwitch = true;

    //empty variables for the player party and wild monsters
    Party playerMonsterParty;
    Party trainerMonsterParty;
    Monster wildMonster;
    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;
    MoveBase moveToLearn;

    // Start is called before the first frame update
    //starts the battle by passing in the player party and enemy monster
    public void StartBattle(Party playerMonsterParty, Monster wildMonster)
    {
        curMove = 0;
        this.playerMonsterParty = playerMonsterParty;
        this.wildMonster = wildMonster;
        player = playerMonsterParty.GetComponent<PlayerController>();
        isTrainerBattle = false;
        StartCoroutine(SetUpBattle());
    }
    public void StartTrainerBattle(Party playerMonsterParty, Party trainerParty)
    {
        curMove = 0;
        this.playerMonsterParty = playerMonsterParty;
        this.trainerMonsterParty = trainerParty;
        isTrainerBattle = true;
        player = playerMonsterParty.GetComponent<PlayerController>();
        trainer = trainerMonsterParty.GetComponent<TrainerController>();
        StartCoroutine(SetUpBattle());
    }

    //initialises battle with the first non fainted player monster and the enemy monster
    public IEnumerator SetUpBattle(){
        playerBattleUnit.Clear();
        enemyBattleUnit.Clear();
        if (!isTrainerBattle){
            //wild battle
            //sets up hud
            playerBattleUnit.SetUp(playerMonsterParty.GetHealthyMonster());
            enemyBattleUnit.SetUp(wildMonster);
            //sets up the moves in which the player monster knows
            dialogBox.SetMoveNames(playerBattleUnit.Monster.Moves);

            //displays to user what they are about to fight and then allows the user to choose an action
            yield return dialogBox.TypeDialog($"A wild {enemyBattleUnit.Monster.Base.Name} appeared");
        }
        else{
            //trainer battle
            //show trainer sprites
            playerBattleUnit.gameObject.SetActive(false);
            enemyBattleUnit.gameObject.SetActive(false);
            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle");
            //send out first monsters
            trainerImage.gameObject.SetActive(false);
            enemyBattleUnit.gameObject.SetActive(true);
            var enemyMonster = trainerMonsterParty.GetHealthyMonster();
            Debug.Log($"{enemyMonster.Base.Name}, {enemyMonster}");
            enemyBattleUnit.SetUp(enemyMonster);
            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {enemyMonster.Base.Name}");

            playerImage.gameObject.SetActive(false);
            playerBattleUnit.gameObject.SetActive(true);
            var playerMonster = playerMonsterParty.GetHealthyMonster();
            Debug.Log($"{playerMonster.Base.Name}, {playerMonster}");
            playerBattleUnit.SetUp(playerMonster);
            yield return dialogBox.TypeDialog($"Go {playerMonster.Base.Name}");
            
            dialogBox.SetMoveNames(playerBattleUnit.Monster.Moves);

        }
        partyScreen.Init();    
        escapeAttempts = 0;
        ActionSelection();
    }

    //battle over state to decide if the battle is over
    void BattleOver(bool won){
        state = BattleState.BattleOver;
        playerMonsterParty.Monsters.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    // state for the player to choose their action
    void ActionSelection(){
        state = BattleState.ActionSelection;
        StartCoroutine(dialogBox.TypeDialog("Choose an action"));
        //enables user input for action selection
        dialogBox.EnableActionSelector(true);
    }

    //state to open party screen
    void OpenPartyScreen(){
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerMonsterParty.Monsters);
        partyScreen.gameObject.SetActive(true);
    }

    //state to disable action selection and turn on the move selection
    void MoveSelection(){
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        //disables the dialog text ~ "Choose an action"
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator TrainerSwitch(Monster newMonster){
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to send out {newMonster.Base.Name}. Do you change monster?");
        state = BattleState.TrainerSwitch;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseToForgetMove(Monster monster, MoveBase newMove){
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose a move you want to forget");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(monster.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;
        state = BattleState.MoveToForget;
    }
    IEnumerator RunTurn(BattleAction playerActionInput){
        state = BattleState.RunningTurn;
        if(playerActionInput == BattleAction.Move){
            playerBattleUnit.Monster.CurrentMove = playerBattleUnit.Monster.Moves[curMove];
            enemyBattleUnit.Monster.CurrentMove = enemyBattleUnit.Monster.GetRandomMove();

            int playerMovePriority = playerBattleUnit.Monster.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyBattleUnit.Monster.CurrentMove.Base.Priority;
            //Check who gets the first move
            bool playerGoesFirst = true;
            if(enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerBattleUnit.Monster.Speed >= enemyBattleUnit.Monster.Speed;

            var firstBattleUnit = (playerGoesFirst) ? playerBattleUnit : enemyBattleUnit;
            var secondBattleUnit = (playerGoesFirst) ? enemyBattleUnit : playerBattleUnit;

            var secondMonster = secondBattleUnit.Monster;

            //first turn
            yield return RunMove(firstBattleUnit, secondBattleUnit, firstBattleUnit.Monster.CurrentMove);
            yield return RunAfterTurn(firstBattleUnit);
            if(state == BattleState.BattleOver) yield break;

            if (secondMonster.HP > 0){
                //second turn
                yield return RunMove(secondBattleUnit, firstBattleUnit, secondBattleUnit.Monster.CurrentMove);
                yield return RunAfterTurn(secondBattleUnit);
                if(state == BattleState.BattleOver) yield break;
            }
            
        }
        else{
            if (playerActionInput == BattleAction.SwitchMonster){
                var selectedMember = playerMonsterParty.Monsters[curMember];
                state = BattleState.Busy;
                yield return SwitchMonster(selectedMember);
            }

            else if(playerActionInput == BattleAction.UseItem){
                dialogBox.EnableActionSelector(false);
                yield return ThrowCaptureCage();
            }
            else if(playerActionInput == BattleAction.Run){
                yield return TryEscape();
            }

            //enemy turn
            var enemyMove = enemyBattleUnit.Monster.GetRandomMove();
            yield return RunMove(enemyBattleUnit, playerBattleUnit, enemyMove);
            yield return RunAfterTurn(enemyBattleUnit);
            if(state == BattleState.BattleOver) yield break;

        }
        if (state != BattleState.BattleOver)
            ActionSelection();
    }

    IEnumerator HandleFaintedMonster(BattleUnit faintedUnit){
        yield return dialogBox.TypeDialog($"{faintedUnit.Monster.Base.Name} fainted");
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit){
            //xp gain
            int xpYield = faintedUnit.Monster.Base.XPYield;
            int enemylvl = faintedUnit.Monster.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;
            int xpGain = Mathf.FloorToInt(xpYield * enemylvl * trainerBonus) / 7;
            playerBattleUnit.Monster.Xp += xpGain;
            yield return dialogBox.TypeDialog($"{playerBattleUnit.Monster.Base.Name} gained {xpGain} xp");
            playerBattleUnit.Hud.SetXP();
            //check xp for lvl up
            while(playerBattleUnit.Monster.CheckForLvlUp()){
                playerBattleUnit.Hud.Setlvl();
                yield return dialogBox.TypeDialog($"{playerBattleUnit.Monster.Base.Name} leveled up to {playerBattleUnit.Monster.Level}");
                
                //learn new move
                var newMove = playerBattleUnit.Monster.GetLearnableMoveAtLevel();
                if(newMove != null){
                    if (playerBattleUnit.Monster.Moves.Count < 4){
                        playerBattleUnit.Monster.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerBattleUnit.Monster.Base.Name} learned {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerBattleUnit.Monster.Moves);
                    }
                    else{
                        yield return dialogBox.TypeDialog($"{playerBattleUnit.Monster.Base.Name} is trying to learn {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"{newMove.Base.Name} cannot learn more than 4 moves, does it forget a move?");
                        yield return ChooseToForgetMove(playerBattleUnit.Monster, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }

                playerBattleUnit.Hud.SetXP(true);
            }
            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
    }

    // checks to see if the battle is over by checking the available healthy monsters remaining for the fainted monster
    void CheckForBattleOver(BattleUnit faintedUnit){
        //if player monster fainted
        if (faintedUnit.IsPlayerUnit){
            //checks to see if the is healthy monsters remaining
            var nextMonster = playerMonsterParty.GetHealthyMonster();
            //opens party screen if there is monsters available
            if(nextMonster != null){
                OpenPartyScreen();
            }
            else{
                BattleOver(false);
            }
        }
        //battle is over if enemy fainted
        else{
            if(!isTrainerBattle){
                BattleOver(true);
            }
            else{
                var nextMonster = trainerMonsterParty.GetHealthyMonster();
                if(nextMonster != null){
                    StartCoroutine(TrainerSwitch(nextMonster));
                }
                else{
                    BattleOver(true);
                }
            }
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move){
        bool canRunMove = sourceUnit.Monster.OnBeforeMove();
        Debug.Log($"{sourceUnit.Monster.Base.Name} {canRunMove}");
        if(!canRunMove){
            yield return ShowStatChanges(sourceUnit.Monster);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatChanges(sourceUnit.Monster);

        move.PP--;

        yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name} used {move.Base.Name}");

        if(CheckIfMovesHits(move, sourceUnit.Monster,targetUnit.Monster)){
            if(move.Base.Category == MoveCategory.Status){
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Monster, targetUnit.Monster, move.Base.Target);
            }
            else{
                var damageDetails = targetUnit.Monster.TakeDamage(move, sourceUnit.Monster);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if(move.Base.SecEffects != null && move.Base.SecEffects.Count > 0 && targetUnit.Monster.HP > 0){
                foreach (var secondary in move.Base.SecEffects){
                    var rnd = UnityEngine.Random.Range(1,101);
                    if(rnd <= secondary.Chance){
                        yield return RunMoveEffects(secondary, sourceUnit.Monster, targetUnit.Monster, secondary.Target);
                    }
                }
            }

            if (targetUnit.Monster.HP <= 0){
                yield return HandleFaintedMonster(targetUnit);
            }
        }
        else{
            yield return dialogBox.TypeDialog($"{sourceUnit.Monster.Base.Name}'s move missed");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Monster source, Monster target, MoveTarget moveTarget){
        // stat boosting
        if(effects.Boosts != null){
            //if stat boost effects self
            if(moveTarget == MoveTarget.Self){
                source.ApplyBoost(effects.Boosts);
            }
            // otherwise stat boost effects target
            else{
                target.ApplyBoost(effects.Boosts);
            }
            // if move sets a status condition
            if (effects.Status != ConditionID.none){
                target.SetStatus(effects.Status);
            }
            // if move sets a volatile status condition
            if (effects.VolatileStatus != ConditionID.none){
                target.SetVolatileStatus(effects.VolatileStatus);
            }

            yield return ShowStatChanges(source);
            yield return ShowStatChanges(target);
        }
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit){
        if(state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);
        //status like burn and poison hurt the monster after the turn
        sourceUnit.Monster.OnAfterTurn();
        yield return ShowStatChanges(sourceUnit.Monster);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Monster.HP <= 0){
            yield return HandleFaintedMonster(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    bool CheckIfMovesHits(Move move, Monster source, Monster target){
        if(move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] {1f, 4f / 3f, 5f / 3f , 2f , 7f / 3f, 8f / 3f, 3f};

        if(accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else
        {
            moveAccuracy /= boostValues[-accuracy];
        }

        if(evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else
        {
            moveAccuracy *= boostValues[-evasion];
        }

        Debug.Log(moveAccuracy);

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatChanges(Monster monster){
        while(monster.StatusChanges.Count > 0){
            var message = monster.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails){
        if(damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");

        if(damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It was super effective");
        else if(damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It was not very effective");
        
    }
    // Update is called once per frame
    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection){
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection){
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen){
            HandlePartySelection();
        }
        else if (state == BattleState.TrainerSwitch){
            HandleTrainerSwitch();
        }
        else if (state == BattleState.MoveToForget){
            Action<int> onMoveSelected = (moveIndex) =>{
                moveSelectionUI.gameObject.SetActive(false);
                if(moveIndex == 4){
                    //dont learn
                    StartCoroutine(dialogBox.TypeDialog($"{playerBattleUnit.Monster.Base.Name} did not learn {moveToLearn.Name}"));
                }
                else{
                    //forget move
                    var selectedMove = playerBattleUnit.Monster.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerBattleUnit.Monster.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}"));
                    playerBattleUnit.Monster.Moves[moveIndex] = new Move(moveToLearn);
                }
                moveToLearn = null;
                state = BattleState.RunningTurn;
            };
            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }

        if(Input.GetKeyDown(KeyCode.T))
            StartCoroutine(ThrowCaptureCage());
    }

    // Handles the fight bag party run selection
    void HandleActionSelection(){
        if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A)){
            --curAction;  
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.D)){
            ++curAction;
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow)||Input.GetKeyDown(KeyCode.W)){
            curAction -= 2;
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown(KeyCode.S)){
            curAction += 2;
        }
        // prevents going over/under the limit
        curAction = Mathf.Clamp(curAction, 0, 3);

        dialogBox.UpdateActionSelection(curAction);

        if(Input.GetKeyDown(KeyCode.Z)){
            if(curAction == 0){
                //fight
                MoveSelection();
            }
            else if(curAction == 1){
                //Bag
                StartCoroutine(RunTurn(BattleAction.UseItem));
            }  
            else if(curAction == 2){
                //Party
                prevState = state;
                OpenPartyScreen();
            }  
            else if(curAction == 3){
                //run
                StartCoroutine(RunTurn(BattleAction.Run));
            }  
        }
    }

    // handles the selection for the fight moves
    void HandleMoveSelection(){

        if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A)){
            --curMove;  
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.D)){
            ++curMove;
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow)||Input.GetKeyDown(KeyCode.W)){
            curMove -= 2;
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown(KeyCode.S)){
            curMove += 2;
        }
        
        // prevents going over/under the limit
        curMove = Mathf.Clamp(curMove, 0, playerBattleUnit.Monster.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(curMove, playerBattleUnit.Monster.Moves[curMove]);

        if (Input.GetKeyDown(KeyCode.Z)){
            var move = playerBattleUnit.Monster.Moves[curMove];
            if(move.PP == 0) return;

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurn(BattleAction.Move));
        }
        else if(Input.GetKeyDown(KeyCode.X)){
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }
    void HandlePartySelection(){
        if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A)){
            --curMember;  
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.D)){
            ++curMember;
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow)||Input.GetKeyDown(KeyCode.W)){
            curMember -= 2;
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown(KeyCode.S)){
            curMember += 2;
        }
        // prevents going over/under the limit
        curMember = Mathf.Clamp(curMember, 0, playerMonsterParty.Monsters.Count - 1);

        partyScreen.UpdateMemberSelection(curMember);

        if (Input.GetKeyDown(KeyCode.Z)){
            var selectedMember = playerMonsterParty.Monsters[curMember];
            if (selectedMember.HP <= 0){
                partyScreen.SetMessageText("You cant send out a fainted Monster");
                return;
            }
            if (selectedMember == playerBattleUnit.Monster){
                partyScreen.SetMessageText("You cant switch with the same Monster");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            if (prevState == BattleState.ActionSelection){
                prevState = null;
                StartCoroutine(RunTurn(BattleAction.SwitchMonster));
            }
            else{
                state = BattleState.Busy;
                StartCoroutine(SwitchMonster(selectedMember));
            }
            
        }
        else if(Input.GetKeyDown(KeyCode.X)){
            if(playerBattleUnit.Monster.HP <= 0){
                partyScreen.SetMessageText("You have to select a monster to continue");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            if(prevState == BattleState.TrainerSwitch){
                prevState = null;
                StartCoroutine(SendNextTrainerMonster());
            }
            else
                ActionSelection();
        }
    }
    void HandleTrainerSwitch(){
        if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S)){
            trainerSwitch = !trainerSwitch;
        }
        dialogBox.UpdateChoiceBox(trainerSwitch);
        if(Input.GetKeyDown(KeyCode.Z)){
            dialogBox.EnableChoiceBox(false);
            if(trainerSwitch == true){
                //yes option
                prevState = BattleState.TrainerSwitch;
                OpenPartyScreen();
            }
            else{
                //no option
                StartCoroutine(SendNextTrainerMonster());
            }
        }
        else if(Input.GetKeyDown(KeyCode.X)){
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerMonster());
        }
    }
    IEnumerator SwitchMonster(Monster newMonster){
        if(playerBattleUnit.Monster.HP > 0 ){
            yield return dialogBox.TypeDialog($"Come back {playerBattleUnit.Monster.Base.Name}");
            yield return new WaitForSeconds(2f);
        }

        playerBattleUnit.SetUp(newMonster);
        dialogBox.SetMoveNames(newMonster.Moves);

        yield return dialogBox.TypeDialog($"Go {newMonster.Base.Name}!");

        if(prevState == null){
            state = BattleState.RunningTurn;
        }
        else if(prevState == BattleState.TrainerSwitch){
            prevState = null;
            StartCoroutine(SendNextTrainerMonster());
        }
        
    }

    IEnumerator SendNextTrainerMonster(){
        state = BattleState.Busy;
        var nextMonster = trainerMonsterParty.GetHealthyMonster();
        enemyBattleUnit.SetUp(nextMonster);
        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {nextMonster.Base.Name}");
        state = BattleState.RunningTurn;
    }
    IEnumerator ThrowCaptureCage(){
        state = BattleState.Busy;

        if(isTrainerBattle){
            yield return dialogBox.TypeDialog($"You cannot steal a trainers monster");
            state = BattleState.RunningTurn;
            yield break;
        }
        yield return dialogBox.TypeDialog($"{player.Name} used the capture cage");

        var captureCageObject = Instantiate(cageCaptureSprite, enemyBattleUnit.transform.position, Quaternion.identity);

        int capCount = TryCapture(enemyBattleUnit.Monster);
        Debug.Log($"{capCount}");
        // add anims
        if (capCount == 4){
            // monster is caught
            yield return dialogBox.TypeDialog($"{enemyBattleUnit.Monster.Base.Name} was caught");
            playerMonsterParty.AddMonster(enemyBattleUnit.Monster);
            yield return dialogBox.TypeDialog($"{enemyBattleUnit.Monster.Base.Name} has been added to the party");
            Destroy(captureCageObject);
            BattleOver(true);
        }
        else{
            //monster broke out 
            yield return new WaitForSeconds(1f);
            
            if (capCount < 2)
                yield return dialogBox.TypeDialog($"{enemyBattleUnit.Monster.Base.Name} burst free");
            if (2 <= capCount || capCount < 4)
                yield return dialogBox.TypeDialog($"{enemyBattleUnit.Monster.Base.Name} broke free at the last second");

            Destroy(captureCageObject);
            state = BattleState.RunningTurn;
        }
    }

    int TryCapture(Monster monster){
        float a = (3 * monster.MaxHP - 2 * monster.HP) * monster.Base.CaptureRate * ConditionsDB.GetStatusBonus(monster.Status) / (3 * monster.MaxHP);

        if(a >= 255)
            return 4;
        
        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int capCount = 0;
        while(capCount < 4){
            if(UnityEngine.Random.Range(0, 65535) >= b)
                break;

            ++capCount;
        }
        return capCount;
    }

    IEnumerator TryEscape(){
        state = BattleState.Busy;
        if(isTrainerBattle){
            yield return dialogBox.TypeDialog($"You cant run from a trainer battle");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int pSpeed = playerBattleUnit.Monster.Speed;
        int eSpeed = enemyBattleUnit.Monster.Speed;

        if(eSpeed < pSpeed){
            yield return dialogBox.TypeDialog($"You manage to run away");
            BattleOver(true);
        }
        else{
            float f = (pSpeed * 128) / eSpeed + 30 * escapeAttempts;
            f = f % 256;

            if(UnityEngine.Random.Range(0, 256) < f){
                yield return dialogBox.TypeDialog($"You manage to run away");
                BattleOver(true);
            }
            else{
                yield return dialogBox.TypeDialog($"You couldnt escape");
                state = BattleState.RunningTurn;
            }
        }
    }
}
