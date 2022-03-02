using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerService : NetworkBehaviour
{

    private PlayerService playerService;
    private bool[] connectedPlayers;
    private bool[] readyPlayers;


    private int deadCounter;
    private int onGroundCounter;
    public delegate void VoidDelegate();

    public delegate void LobbyTimerStartDelegate();
   

    public delegate void LobbyTimerResetDelegate();
    [SyncVar]
    public bool lobbyTimerStarted; 

    public delegate void GameStartDelegate();
    

    public delegate void GameEndDelegate();

    
    public event VoidDelegate EventGameStart;

    public event VoidDelegate EventGameEndMagesWon;

    public event VoidDelegate EventGameEndMonsterWon;
    
    public event VoidDelegate EventLobbyTimerStart;
    
    public event VoidDelegate EventLobbyTimerReset;

    private bool timerStarted;
    private float lobbyTimer;
    [SyncVar]
    public GameState gameState;

    private int oldCountConnectedPlayers;

    public enum GameState
    {
        Lobby,
        InGame,
        PostGame
    }


    /// <summary>
    /// Server call to clients that the game starts.
    /// </summary>
    [ClientRpc]
    public void RpcGameStart()
    {
        EventGameStart();
    }

    /// <summary>
    /// Server call to clients that the game ends.
    /// </summary>
    /// <param name="magesWon">true -> mages won/ false -> monster won</param>
    [ClientRpc]
    public void RpcGameEnd(bool magesWon)
    {
        if (magesWon)
        {
            EventGameEndMagesWon();

        }
        else
        {
            EventGameEndMonsterWon();
        }
    }

    /// <summary>
    /// Server call to clients, that the lobby timer should start.
    /// </summary>
    [ClientRpc]
    public void RpcLobbyTimerStart()
    {
        EventLobbyTimerStart();
    }

    /// <summary>
    /// Server call to clients to reset the lobby timer.
    /// </summary>
    [ClientRpc]
    public void RpcLobbyTimerReset()
    {
        EventLobbyTimerReset();
    }




   

    // Start is called before the first frame update
    [Server]
    void Start()
    {
        playerService = GameObject.Find("ServiceManager").GetComponent<PlayerService>();
        

        readyPlayers = new bool[5];
        
        gameState = GameState.Lobby;

        lobbyTimer = 10;
    }

    /// <summary>
    /// Server call for synchronization.
    /// </summary>
    /// <param name="index">Index of player</param>
    [Server]
    public void SetReady(int index)
    {
        readyPlayers[index] = true;
    }

    /// <summary>
    /// Server call for synchronization.
    /// </summary>
    /// <param name="index">Index of player.</param>
    [Server]
    public void SetUnready(int index)
    {
        readyPlayers[index] = false;
    }

    // Update is called once per frame
    [ServerCallback]
    void Update()
    {
        switch (gameState)
        {
            case GameState.Lobby:
                LobbyRoutine();
                break;
            case GameState.InGame:

                break;
            case GameState.PostGame:
                break;
        }
        
    }    

    [Server]
    public void SetTimerStarted(bool value)
    {
        if (value != timerStarted)
        {

            timerStarted = value;
            if (timerStarted == true)
            {
                RpcLobbyTimerStart();

            }
            else
            {
                RpcLobbyTimerReset();

            }
        }
    }



    /// <summary>
    /// Handles the ready state of the players in the lobby and counts down the timer, if everyone is ready. 
    /// If timer hits zero, the game will start.
    /// </summary>
    [Server]
    private void LobbyRoutine()
    {
        connectedPlayers = playerService.connectedPlayers;
        int countReady=0;
        int countConnected=0;

        for(int i = 0; i < connectedPlayers.Length; i++)
        {
            if (connectedPlayers[i] == true) countConnected++;
            if (readyPlayers[i] == true) countReady++;
        }

        
        if (readyPlayers[0] == true && countReady > 1 && countReady == countConnected)
        {
            SetTimerStarted(true);
            lobbyTimer = lobbyTimer - Time.deltaTime;
        }
        else
        {
            SetTimerStarted(false);
            
            lobbyTimer = 10;
        }

        if (lobbyTimer < 0)
        {
            lobbyTimer = 0;
            StartGame();
        }
    }

    /// <summary>
    /// Adds a dead player to the count. If all players are dead the game ends.
    /// </summary>
    [Server]
    public void AddDeadPlayer()
    {
        deadCounter++;
        int countConnected = 0;
        for (int i = 0; i < connectedPlayers.Length; i++)
        {
            if (connectedPlayers[i] == true) countConnected++;
        }

        if (deadCounter == countConnected-1)
        {
            EndGame(false);
        }
    }
    /// <summary>
    /// Adds a on ground player to the count. If all players are on ground the game ends, because no player can be revived.
    /// </summary>
    [Server]
    public void AddOnGroundPlayer()
    {
        onGroundCounter++;
        int countConnected = 0;
        for (int i = 0; i < connectedPlayers.Length; i++)
        {
            if (connectedPlayers[i] == true) countConnected++;
        }

        if (onGroundCounter == countConnected - 1)
        {
            EndGame(false);
        }
    }

   
    [Server]
    public void RemoveOnGroundPlayer()
    {
        onGroundCounter--;
        
    }

    [Server]
    public void StartGame()
    {
        gameState = GameState.InGame;
        RpcGameStart();
    }

    [Server]
    public void EndGame(bool magesWon)
    {
        gameState = GameState.PostGame;
        RpcGameEnd(magesWon);
    }

}
