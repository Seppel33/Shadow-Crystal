using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CrystalScriptLocal : NetworkBehaviour
{


    private PlayerService playerService;
    private Vector3 position;
    private CrystalScriptServer serverScript;
    private Light lightSource;

    private GameObject crystal;

    private Color color;

    private bool fullyCharged;
    private bool chargingState;

    public UITracker monsterUITracker;
    public UITracker magesUITracker;

    private Animator animator;

    [SerializeField]
    private int lightIntensityMin = 5172; //candela
    [SerializeField]
    private int lightIntensityMax = 15915;

    [SerializeField]
    private int lightRangeMin = 6;
    [SerializeField]
    private int lightRangeMax = 20;

    private MagesController magesController;

    public GameObject chargeSliderPrefab;
    private Slider chargeSlider;

    /// <summary>
    /// Chargingstate. If chargingstate changes to true, it adds a charging Player to the server, if charginstate
    /// changed to false, it removes a charging player.
    /// </summary>
    private bool ChargingState
    {
        get => chargingState;
        set
        {
            if (chargingState != value)
            {
                chargingState = value;
                if (chargingState)
                {
                    playerService.LocalPlayerManager.GetComponent<PlayerManager>().CmdAddChargingPlayer(gameObject);
                }
                else
                {
                    playerService.LocalPlayerManager.GetComponent<PlayerManager>().CmdRemoveChargingPlayer(gameObject);
                }
            }
        }
    }


    // Start is called before the first frame update
    [Client]
    void Start()
    {
        playerService = GameObject.Find("ServiceManager").GetComponent<PlayerService>();
        playerService.EventInitialisedMage += InitializedMage;
        position = new Vector3(transform.position.x, 0, transform.position.z);

        lightSource = transform.Find("CrystalLight").gameObject.GetComponent<Light>();
        color = lightSource.color;
        serverScript = GetComponent<CrystalScriptServer>();
        chargingState = false;
        fullyCharged = false;

        animator = GetComponent<Animator>();

        chargeSlider = Instantiate(chargeSliderPrefab, playerService.GetComponent<UITrackerService>().parent).GetComponent<Slider>();
    }

    /// <summary>
    /// Gets called when all mages have been initialised. 
    /// </summary>
    private void InitializedMage()
    {
       if(playerService.LocalPlayer.GetComponent<AbstractController>().Type != PlayerService.Characters.Monster) magesController = playerService.LocalPlayer.GetComponent<MagesController>();
        if (magesController != null) magesController.EventMageOnGround += StopCharging;
    }


    /// <summary>
    /// Handles the charging state, animation and crystal speed. The code runs in LateUpdate, so that things like LookAt are working properly.
    /// </summary>
    [Client]
    private void LateUpdate()
    {
        if (magesController != null)
        {
                       
            if (!fullyCharged)
            {
                Vector3 lengthVec = position - new Vector3(playerService.LocalPlayer.transform.position.x, 0, playerService.LocalPlayer.transform.position.z);
                if (Vector3.Magnitude(lengthVec) < 6f)
                {
                    
                    if (magesController.otherState != AbstractController.SpellState.InAction)
                    {
                        if (Input.GetKeyDown("f"))

                        {
                            HandlePlayerCharge(magesController);
                            magesController.isCharging = true;
                            chargeSlider.gameObject.SetActive(true);
                            chargeSlider.value = serverScript.Progress / 100;
                        }
                        if (magesController.animator.GetCurrentAnimatorStateInfo(0).IsName("ChargeMidLoop") && magesController.isCharging)
                        {
                            if (ChargingState == false)
                            {

                                ChargingState = true;
                                magesController.SetLaser(true, transform.position);
                                magesController.CmdSetLaser(true, transform.position);
                            }
                        }
                        if (Input.GetKeyUp("f"))
                        {
                            chargeSlider.gameObject.SetActive(false);
                            StopCharging();
                            
                        }
                    }
                }
                else
                {
                    ChargingState = false;
                }

            }
            else
            {
                chargeSlider.gameObject.SetActive(false);
            }
           

            
        }



            float t = serverScript.Progress/100;
        chargeSlider.value = t;
        animator.SetFloat("AnimSpeed", Mathf.Lerp(0.2f, 1, t));
        lightSource.intensity = Mathf.Lerp(lightIntensityMin, lightIntensityMax, t);
        lightSource.range = Mathf.Lerp(lightRangeMin, lightRangeMax, t);
        //crytalmat.EnableKeyword("_EmissiveIntensity");
        //crytalmat.SetFloat("_EmissiveIntensity", 300);
    }

    /// <summary>
    /// Handles the start of the animation and the particle, laser placement. 
    /// </summary>
    /// <param name="magesController">The GameObject of the mage-charakter of the given client.</param>
    private void HandlePlayerCharge(MagesController magesController)
    {
        magesController.LockMovement(true);
        magesController.animator.SetBool("Charging", true);

        magesController.transform.LookAt(transform);
        magesController.transform.eulerAngles = new Vector3(0, magesController.transform.eulerAngles.y, 0);
        Vector3 crystalPos = new Vector3(transform.position.x, 0, transform.position.z);
        float dist = (crystalPos - magesController.transform.position).magnitude;
        magesController.laser.GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, 1.8f, dist - 1.6f));
        magesController.laser.GetComponentInChildren<ParticleSystem>().transform.localPosition = new Vector3(0, 1.6f, dist - 1.9f);
       // magesController.laser.GetComponentInChildren<ParticleSystem>().simulationSpace = ParticleSystemSimulationSpace.World;

    }

    /// <summary>
    /// Stops the Charging animation and deactivates the laser and particleSystem.
    /// </summary>
    private void StopCharging()
    {
        magesController.LockMovement(false);
        magesController.animator.SetBool("Charging", false);

        magesController.SetLaser(false, transform.position);
        magesController.CmdSetLaser(false, transform.position);
        
        ChargingState = false;
        magesController.isCharging = false;
    }

    /// <summary>
    /// Call to all Clients if a crystal is fully charged.
    /// </summary>
    [ClientRpc]
    public void RpcFullyCharged()
    {
        fullyCharged = true;
        if(chargingState==true)
        {
            StopCharging();
        }

        StartCoroutine(ShowCrystalIcon());
    }

    /// <summary>
    /// Shows the crystal Icon for a short period of time after fully charging the crystal.
    /// The onscreen part of the Icon remains, even after the short period of time, so that everyone know that the crystal they are seeing onscreen is fully charged.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowCrystalIcon()
    {
        monsterUITracker.visible = true;
        monsterUITracker.visibleOnScreen = true;

        magesUITracker.visible = true;
        magesUITracker.visibleOnScreen = true;
        yield return new WaitForSeconds(8);
        monsterUITracker.visible = false;
        magesUITracker.visible = false;
    }
}
