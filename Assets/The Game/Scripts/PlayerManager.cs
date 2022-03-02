using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;


public class PlayerManager : NetworkBehaviour
{
    public GameObject player;
    private PlayerService service;
    private CrystalService crystalService;
    private bool[] selectedPlayers;


    private void Awake()
    {
        service = GameObject.FindGameObjectWithTag("Service").GetComponent<PlayerService>();
        crystalService = GameObject.FindGameObjectWithTag("Service").GetComponent<CrystalService>();
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gameObject, UnityEngine.SceneManagement.SceneManager.GetSceneAt(1));
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }


    

    public override void OnStartLocalPlayer()
    {

        base.OnStartLocalPlayer();
        
        service.LocalPlayerManager = gameObject;

        
    }
   

    public void SpawnPlayer(int index, string name)
    {
        if (isLocalPlayer) CmdRequestPlayerAuthority(index, name);

        ////Instantiate(_playerPrefabs[0]);
        //player = Instantiate(_playerPrefabs[index%_playerPrefabs.Count]);    

        //NetworkServer.Spawn(player, GetComponent<NetworkIdentity>().connectionToClient);
    }

    [Command]
    public void CmdAddChargingPlayer(GameObject crystal)
    {
        crystal.GetComponent<CrystalScriptServer>().AddChargingPlayer(gameObject);
    }

    [Command]
    public void CmdRemoveChargingPlayer(GameObject crystal)
    {
        crystal.GetComponent<CrystalScriptServer>().RemoveChargingPlayer(gameObject);
    }

    /// <summary>
    /// Request the authority from the server for the characterGameObject with the given index.
    /// </summary>
    /// <param name="index">Index of the character.</param>
    /// <param name="name">Name of the player.</param>
    [Command]
    public void CmdRequestPlayerAuthority(int index, string name)
    {
        if(service.connectedPlayers[index] == false)
        {
            service._allChars[index].gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(GetComponent<NetworkIdentity>().connectionToClient);
            service.SetSelectedPlayer(index, name);
        }           
    }

    [Command]
    public void CmdGetSelectedPlayers()
    {
        service.GetSelectedPlayers(GetComponent<NetworkIdentity>().connectionToClient);
    }


    [Command]
    public void CmdRequestAuthority(NetworkIdentity netIdent)
    {
        netIdent.AssignClientAuthority(GetComponent<NetworkIdentity>().connectionToClient);
    }
    // Update is called once per frame
    void Update()
    {
        if (base.hasAuthority && isLocalPlayer&&false) //for debug purposes
        {
            if (Input.GetKeyDown("1")) SpawnPlayer(0,"test");
            if (Input.GetKeyDown("2")) SpawnPlayer(1, "test");
            if (Input.GetKeyDown("3")) SpawnPlayer(2, "test");
            if (Input.GetKeyDown("4")) SpawnPlayer(3, "test");
            if (Input.GetKeyDown("5")) SpawnPlayer(4, "test");
        }
    }



    [Command]
    public void CmdReadyClick(int index)
    {
        GameObject.Find("ServiceManager").GetComponent<GameManagerService>().SetReady(index);
    }

    [Command]
    public void CmdUnreadyClick(int index)
    {
        GameObject.Find("ServiceManager").GetComponent<GameManagerService>().SetUnready(index);
    }

    [Command]
    public void CmdRemoveClientAuthority()
    {

    }
    [Command]
    public void CmdTriggerEndGame()
    {
        service.GetComponent<GameManagerService>().EndGame(true);

    }

}
