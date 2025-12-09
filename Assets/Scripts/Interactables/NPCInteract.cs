using UnityEngine;

public class NPCInteract : Interactable
{
    [SerializeField] private Dialogue dialogue;
    [SerializeField] private EnemySpawner spawner;

    [Header("Dialogue Sets")]
    public string[] goblinDialogue;   
    public string[] spiderDialogue;   
    public string[] ogreDialogue;     
    public string[] finalDialogue;    

    private bool isTalking = false;

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
        if (spawner != null)
        {
            spawner.OnWaveCompleted += HandleWaveCompleted;
            spawner.OnAllWavesCompleted += HandleAllWavesCompleted;
        }

        dialogue.OnDialogueFinished += ResetDialogueState;
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
                spawner.StartWave(2);
                break;

            case NPCState.Ogre:
                spawner.StartWave(3);
                break;

            case NPCState.Final:
                break;
        }
    }

    private void HandleWaveCompleted(int wave)
    {
        if (wave == 1)
        {
            currentState = NPCState.Spiders;
        }
        else if (wave == 2)
        {
            currentState = NPCState.Ogre;
        }
    }

    private void HandleAllWavesCompleted()
    {
        currentState = NPCState.Final;
    }

    private void OnDisable()
    {
        isTalking = false;
    }
}
