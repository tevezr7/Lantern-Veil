using UnityEngine;

public class NPCInteract : Interactable
{
    [SerializeField] private Dialogue dialogue;
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private MusicController music;


    [Header("Dialogue Sets")]
    public string[] goblinDialogue;   
    public string[] spiderDialogue;   
    public string[] ogreDialogue;     
    public string[] finalDialogue;    

    private bool isTalking = false;
    private bool spidersSpawned = false;
    private bool ogreSpawned = false;

    public NPCState currentState = NPCState.Default;

    public enum NPCState
    {
        Default,    
        Goblins,    
        Spiders,    
        Ogre,       
        Final       
    }

    private void Start()
    {
        spawner.CheckForExistingEnemies();
        if (spawner.EnemiesAlive > 0 && spawner.CurrentWave == 1)
        {
            music?.PlayCombat();
        }
        else
        {
            music?.PlayAmbient();
        }

        if (spawner != null)
        {
            spawner.OnWaveCompleted += HandleWaveCompleted;
            spawner.OnAllWavesCompleted += HandleAllWavesCompleted;
        }

        dialogue.OnDialogueFinished += ResetDialogueState;
        dialogue.OnDialogueFinished += CheckForWin;

    }

    private void ResetDialogueState()
    {
        isTalking = false;
    }

    protected override void Interact()
    {
        
        if (!isTalking)
        {
            isTalking = true;

            dialogue.Begin(GetDialogueForState());

            TriggerWaveForState();
        }
        else
        {
            dialogue.ContinueDialogue();
        }
    }

    private string[] GetDialogueForState()
    {
        switch (currentState)
        {
            case NPCState.Goblins:
                return goblinDialogue;

            case NPCState.Spiders:
                return spiderDialogue;

            case NPCState.Ogre:
                return ogreDialogue;

            case NPCState.Final:
                return finalDialogue;

            default:
                return goblinDialogue;
        }
    }

    private void CheckForWin()
    {
        if (currentState == NPCState.Final)
        {
            var win = FindObjectOfType<WinScreenController>(true);
            if (win != null)
            {
                win.ShowWinScreen();
            }
        }
    }

    private void TriggerWaveForState()
    {
        switch (currentState)
        {
            case NPCState.Default:
                currentState = NPCState.Goblins;
                break;

            case NPCState.Goblins:
                break;

            case NPCState.Spiders:
                if (!spidersSpawned)
                {
                    music?.PlayCombat();
                    spidersSpawned = true;
                    spawner.StartWave(2);
                }
                break;

            case NPCState.Ogre:
                if (!ogreSpawned)
                {
                    music?.PlayCombat();
                    ogreSpawned = true;
                    spawner.StartWave(3);
                }
                break;

            case NPCState.Final:
                break;
        }
    }

    private void HandleWaveCompleted(int wave)
    {
        if (wave == 1)
        {
            music?.PlayAmbient();
            currentState = NPCState.Spiders;
        }
        else if (wave == 2)
        {
            music?.PlayAmbient();
            currentState = NPCState.Ogre;
        }
    }

    private void HandleAllWavesCompleted()
    {
        music?.PlayAmbient();
        currentState = NPCState.Final;
    }

    private void OnDisable()
    {
        isTalking = false;
    }
}
