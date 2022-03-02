using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalScriptServer : NetworkBehaviour
{
    [SyncVar]
    private float progress;
    private bool fullyCharged;

    public float chargingTime = 50;

    private List<GameObject> chargingPlayers;

    public float Progress { get
        {
            return progress;
        }
        private set
        {
            progress = value;
        }
    }

    private CrystalService service;

    // Start is called before the first frame update
    [Server]
    void Start()
    {
        Progress = 0;
        chargingPlayers = new List<GameObject>();
        service = GameObject.FindGameObjectWithTag("Service").GetComponent<CrystalService>();

    }   
    
    /// <summary>
    /// Runs only on the server. Handles the charging of the crystal and the given efficiency for every added charging player.
    /// </summary>
    [ServerCallback]
    void FixedUpdate()
    {
        if (!fullyCharged)
        {
            float efficiency = 0;
            switch (chargingPlayers.Count)
            {
                case 0:
                    efficiency = 0;
                    break;
                case 1:
                    efficiency = 1;
                    break;
                case 2:
                    efficiency = 1.7f;
                    break;
                case 3:
                    efficiency = 2.2f;
                    break;
                case 4:
                    efficiency = 2.5f;
                    break;

            }
            progress = progress + efficiency * (1f / 50f)*100f/chargingTime;
            if (progress >= 100)
            {
                fullyCharged = true;
                progress = 100;
                service.AddChargedCrystal();
                gameObject.GetComponent<CrystalScriptLocal>().RpcFullyCharged();
            }
        }
        
    }

    /// <summary>
    /// Adds a player to the <see cref="chargingPlayers"/> list.
    /// </summary>
    /// <param name="chargingPlayer">New charging Player.</param>
    [Server]
    public void AddChargingPlayer(GameObject chargingPlayer)
    {
        if(!chargingPlayers.Contains(chargingPlayer)) chargingPlayers.Add(chargingPlayer);
    }

    /// <summary>
    /// Removes a player to the <see cref="chargingPlayers"/> list.
    /// </summary>
    /// <param name="chargingPlayer">Player to remove.</param>
    [Server]
    public void RemoveChargingPlayer(GameObject chargingPlayer)
    {
        if(chargingPlayers.Contains(chargingPlayer)) chargingPlayers.Remove(chargingPlayer);
    }

}
