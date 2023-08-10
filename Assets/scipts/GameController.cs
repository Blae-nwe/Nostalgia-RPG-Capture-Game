using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState {FreeRoam, Battle, Dialog, Cutscene}

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    GameState state;
    public static GameController Instance{get;private set;}

    public void Awake(){
        Instance = this;
        ConditionsDB.Init();
    }
    // Start is called before the first frame update
    private void Start()
    {
        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;

        playerController.OnEnterTrainerFOV += (Collider2D trainerCollider) =>{
            var trainer = trainerCollider.GetComponentInParent<TrainerController>();
            if(trainer != null){
                state = GameState.Cutscene;
                StartCoroutine(trainer.TriggerTrainerBattle(playerController));
            }
        };

        DialogManager.Instance.OnShowDialog += () =>{
            state = GameState.Dialog;
        };
        DialogManager.Instance.OnCloseDialog += () =>{
            if(state == GameState.Dialog)
                state = GameState.FreeRoam;
        };
    }

    void StartBattle(){
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<Party>();
        var wildMonster = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildMonster();

        var wildMonsterCopy = new Monster(wildMonster.Base, wildMonster.Level);

        battleSystem.StartBattle(playerParty, wildMonsterCopy);
    }
    TrainerController trainer;
    public void StartTrainerBattle(TrainerController trainer){
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        this.trainer = trainer;
        var playerParty = playerController.GetComponent<Party>();
        var trainerParty = trainer.GetComponent<Party>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }
    void EndBattle(bool won){
        if(trainer != null && won == true){
            trainer.LostBattle();
            trainer = null;
        }
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }
    // Update is called once per frame
    private void Update()
    {
        if(state == GameState.FreeRoam){
            playerController.HandleUpdate();
        }
        else if(state == GameState.Battle){
            battleSystem.HandleUpdate();
        }
        else if(state == GameState.Dialog){
            DialogManager.Instance.HandleUpdate();
        }
    }
}
