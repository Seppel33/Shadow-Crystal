using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerService : NetworkBehaviour
{

    public List<GameObject> _playerPrefabs;
    public bool[] connectedPlayers;
    public string[] connectedPlayersName;
    private GameObject localPlayer;
    public GameObject LocalPlayer
    {
        get { return localPlayer; }
        set
        {
            localPlayer = value;
            EventInitialisedMage?.Invoke();
        }
    }

    private GameObject localPlayerManager;
    public GameObject LocalPlayerManager
    {
        get { return localPlayerManager; }
        set
        {
            localPlayerManager = value;
            EventInitialisedPlayerManager?.Invoke();
        }
    }
    public NetworkBondedPlayer[] _allChars = new NetworkBondedPlayer[5];


    [HideInInspector]
    public GameObject monster;
    [HideInInspector]
    public GameObject hero;
    [HideInInspector]
    public GameObject wizard;
    [HideInInspector]
    public GameObject rogue;
    [HideInInspector]
    public GameObject fairy;
    [HideInInspector]
    public ShadowMonsterController monsterController;
    [HideInInspector]
    public HeroHaraldController heroController;
    [HideInInspector]
    public WizardWassilyController wizardController;
    [HideInInspector]
    public RogueReyController rogueController;
    [HideInInspector]
    public FairyFylaController fairyController;

    public delegate void VoidDelegate();


    public event VoidDelegate EventInitialisedMage;

    public event VoidDelegate EventInitialisedPlayerManager;

    private GameManagerService gameManagerService;

    public enum Characters
    {
        Monster = 0,
        Hero = 1,
        Wizard = 2,
        Rogue = 3,
        Fairy = 4
    }

    public struct NetworkBondedPlayer
    {

        public GameObject gameObject;
        public Characters type;
    }

    public GameObject spawn;

    private void Awake()
    {
        connectedPlayers = new bool[5];
        connectedPlayersName = new string[5];

    }



    // Start is called before the first frame update
    /// <summary>
    /// Spawns and saves all playerChars.
    /// </summary>
    [Server]
    void Start()
    {
        gameManagerService = GetComponent<GameManagerService>();
        connectedPlayersName = new string[5];
        Vector3[] spawns = GetSpawns(); 

        for (int i = 0; i < 5; i++)
        {
            GameObject newPlayer = Instantiate(_playerPrefabs[i], spawns[i], Quaternion.identity);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(newPlayer, UnityEngine.SceneManagement.SceneManager.GetSceneAt(1));
            _allChars[i].gameObject = newPlayer;

            NetworkServer.Spawn(_allChars[i].gameObject);
            _allChars[i].type = (Characters)i;

            switch ((Characters)i)
            {
                case (Characters.Monster):
                    monster = newPlayer;
                    monsterController = newPlayer.GetComponent<ShadowMonsterController>();
                    break;
                case (Characters.Hero):
                    hero = newPlayer;
                    heroController = newPlayer.GetComponent<HeroHaraldController>();
                    break;
                case (Characters.Wizard):
                    wizard = newPlayer;
                    wizardController = newPlayer.GetComponent<WizardWassilyController>();
                    break;
                case (Characters.Rogue):
                    rogue = newPlayer;
                    rogueController = newPlayer.GetComponent<RogueReyController>();
                    break;
                case (Characters.Fairy):
                    fairy = newPlayer;
                    fairyController = newPlayer.GetComponent<FairyFylaController>();
                    break;
            }
        }
        RemoveAllPlayerCollision();
        for (int i = 1; i < 5; i++)
        {
            _allChars[(int)Characters.Monster].gameObject.GetComponent<MonstersController>().OtherColliderBodys.Add(_allChars[i].gameObject.GetComponent<CapsuleCollider>());
        }



    }

   
    /// <summary>
    /// Randomly selects the spawns out of the spawn pool and returns an array with the spawns for the given players. 
    /// </summary>
    /// <returns></returns>
    [Server]
    private Vector3[] GetSpawns()
    {
        Vector3[] spawnPositions = new Vector3[5];
        UnityEngine.Random ran = new UnityEngine.Random();
        
        //MonsterSpawn
        

        int randomIndex = UnityEngine.Random.Range(0, spawn.transform.Find("MonsterSpawns").childCount-1);

        spawnPositions[0] = spawn.transform.Find("MonsterSpawns").GetChild(randomIndex).position;

        List<int> randomIndexes = new List<int>();

        for (int i = 0; i < 4; i++)
        {
            bool newIndex = false;
            while (!newIndex)
            {
                randomIndex = UnityEngine.Random.Range(0, spawn.transform.Find("MageSpawns").childCount - 1);
                if (!randomIndexes.Contains(randomIndex))
                {
                    randomIndexes.Add(randomIndex);
                    newIndex = true;
                }
            }
            

            spawnPositions[i+1] = spawn.transform.Find("MageSpawns").GetChild(randomIndexes[i]).position;
        }

        return spawnPositions;
    }

    public void AddNewPlayer(GameObject gameObject, Characters type)
    {
        _allChars[(int)type].gameObject = gameObject;
        _allChars[(int)type].type = type;
        RemovePlayerCollisionForNewPlayer((int)type);


        switch (type)
        {
            case (Characters.Monster):
                monster = gameObject;
                monsterController = gameObject.GetComponent<ShadowMonsterController>();
                break;
            case (Characters.Hero):
                hero = gameObject;
                heroController = gameObject.GetComponent<HeroHaraldController>();
                break;
            case (Characters.Wizard):
                wizard = gameObject;
                wizardController = gameObject.GetComponent<WizardWassilyController>();
                break;
            case (Characters.Rogue):
                rogue = gameObject;
                rogueController = gameObject.GetComponent<RogueReyController>();
                break;
            case (Characters.Fairy):
                fairy = gameObject;
                fairyController = gameObject.GetComponent<FairyFylaController>();
                break;
        }
    }

    [Server]
    public void SetSelectedPlayer(int index, string name)
    {
        connectedPlayers[index] = true;
        connectedPlayersName[index] = name;
        RpcSetSelectedPlayer(index, name);
    }

    [ClientRpc]
    private void RpcSetSelectedPlayer(int index, string name)
    {
        connectedPlayers[index] = true;
        connectedPlayersName[index] = name;
    }

    [Server]
    public void GetSelectedPlayers(NetworkConnection netConnection)
    {
        TargetRpcReturnSelectedPlayers(netConnection, connectedPlayers, connectedPlayersName);
    }

    [TargetRpc]
    public void TargetRpcReturnSelectedPlayers(NetworkConnection netConnection, bool[] selectedPlayers, string[] selectedPlayersName)
    {
        this.connectedPlayers = selectedPlayers;
        this.connectedPlayersName = selectedPlayersName;
    }

    /// <summary>
    /// Removes the collision between characters.
    /// </summary>
    private void RemoveAllPlayerCollision()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = i + 1; j < 5; j++)
            {
                if(_allChars!=null) Physics.IgnoreCollision(_allChars[i].gameObject.GetComponent<CapsuleCollider>(), _allChars[j].gameObject.GetComponent<CapsuleCollider>(), true);

            }
        }
    }

    /// <summary>
    /// Removes the collision between characters.
    /// </summary>
    /// <param name="index"></param>
    private void RemovePlayerCollisionForNewPlayer(int index)
    {
        for (int j = 0; j < 5; j++)
        {
            if (_allChars[j].gameObject != null && j!=index) Physics.IgnoreCollision(_allChars[index].gameObject.GetComponent<CapsuleCollider>(), _allChars[j].gameObject.GetComponent<CapsuleCollider>(), true);

        }
    }
}
