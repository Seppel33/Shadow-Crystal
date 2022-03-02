using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalService : NetworkBehaviour
{
    [SyncVar]
    private int chargedCrystals;


    public List<CrystalScriptLocal> crystalScriptList;


    GameManagerService gameService;

    // Start is called before the first frame update
    [Server]
    void OnEnable()
    {
        gameService = GameObject.Find("ServiceManager").GetComponent<GameManagerService>();
        chargedCrystals = 0;
    }

    /// <summary>
    /// Handles charged crystals and ends the game if enough crystals are charged.
    /// </summary>
    [Server]
    public void AddChargedCrystal()
    {
        chargedCrystals++;

        if(chargedCrystals== crystalScriptList.Count-1)
        {
            gameService.EndGame(true);
        }
    }

    public int GetChargedCrystals()
    {
        return chargedCrystals;
    }
}
