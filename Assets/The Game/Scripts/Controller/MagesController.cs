using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MagesController : AbstractController
{
    public float testVar;

    [SerializeField]
    private float QuCooldown = 5f;
    [HideInInspector]
    public float quCooldownTimer;

    public float otherSpellCooldown;
    [HideInInspector]
    public float otherSpellCooldownTimer;
    public float otherSpellDuration;

    public float healingTime = 10;
    public float revivingTime = 10;

    public float onGroundDuration = 20;
    [HideInInspector]
    public float onGroundTimer;
    [HideInInspector]
    private float healingTimer;
    [HideInInspector]
    private float revivingTimer;

    public SpellState quState;
    public SpellState otherState;

    public int lifePoints = 2;

    public float visionTransition = 2f;

    public GameObject lightSphereQu;

    public Light lightSphereQuLight;
    public SphereFollower quScript;

    public float stableLightIntesity;
    public float stableLightRange;

    private CrystalService crystalService;

    private List<Collider> otherColliderBodys;

    private bool revivable = true;

    public bool isHealing;
    public bool isReviving;

    private bool getsRevivedOrHealed;

    private int typeOfTargetedPlayer;
    private GameObject targetedPlayer;

    public List<SkinnedMeshRenderer> meshRendererList;

    private bool isDead;
    

    public float deathFadeOutTime = 4;
    private float deathFadeOutTimer;

    public delegate void VoidDelegate();

    public event VoidDelegate EventMageOnGround;

    public ParticleSystem playerTrails;
    public ParticleSystemRenderer playerTrailRenderer;

    public GameObject laser;

    public bool isCharging;

    public Volume magesVolumeHeartBeat;
    public Volume magesVolumeHit;

    private bool volumeHitLerpActive;
    private float volumeHitLerp;

    private float heartbeatDelta;
    private bool fadedOut;

    public UITracker uiTrackerForMages;
    public UITracker uiTrackerForMonster;

    public GameObject healBarPrefab;
    public GameObject reviveBarPrefab;

    private Slider healBar;
    private Slider reviveBar;

    public bool anyOneInRange;

    public bool IsDead { get => isDead;
        set
        {
            if (value != isDead)
            {
                isDead = value;
                if (isDead)
                {
                    StartCoroutine(WaitDeathFadeOUt());
                }
            }
                
           
        }

    }

    private bool deathFadeOut;

    [Client]
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void Start()
    {
        base.Start();
        stableLightIntesity = lightSphereQuLight.intensity;
        stableLightRange = lightSphereQuLight.range;
        quScript = lightSphereQu.GetComponent<SphereFollower>();

        crystalService = GameObject.Find("ServiceManager").GetComponent<CrystalService>();

        quState = SpellState.Up;
        otherState = SpellState.Up;

       

    }
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        magesVolumeHeartBeat.gameObject.SetActive(true);
        magesVolumeHeartBeat.enabled = true;

        magesVolumeHit.gameObject.SetActive(true);
        magesVolumeHit.enabled = true;

        magesVolumeHeartBeat.weight = 0;
        magesVolumeHit.weight = 0;

        healBar = Instantiate(healBarPrefab, service.GetComponent<UITrackerService>().parent).GetComponent<Slider>();
        reviveBar = Instantiate(reviveBarPrefab, service.GetComponent<UITrackerService>().parent).GetComponent<Slider>();

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

    }

    protected override void Update()
    {
        base.Update();
        if (this._isLocalPlayer && isIngame)
        {
            SpellMoveQu();
            ReviveOrHeal();

            if (lifePoints == 0) CheckOnGround();

            


            VolumeLerp();
   
        }
        if (isIngame&& deathFadeOut) DeathFadeOut();

    }

    /// <summary>
    /// Lerps and combines two volumes if the mage gets hit or healed.
    /// </summary>
    private void VolumeLerp()
    {
        if (volumeHitLerpActive)
        {
            volumeHitLerp += Time.deltaTime;
            if (volumeHitLerp > 0.65f) volumeHitLerp = 0.65f;
            //chromaticAberration.intensity.SetValue(new NoInterpMinFloatParameter(chromaLerp, 0, true));
            magesVolumeHit.weight = volumeHitLerp;
        }
        else
        {
            volumeHitLerp -= Time.deltaTime;
            if (volumeHitLerp < 0) volumeHitLerp = 0;
            //chromaticAberration.intensity.SetValue(new NoInterpMinFloatParameter(chromaLerp, 0, true));
            magesVolumeHit.weight = volumeHitLerp;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        if (this._isLocalPlayer && isIngame)
        {
            CheckMonsterTranparency();
            HearBeat();
        }
    }

    /// <summary>
    /// Routine to calculate the heartbeat
    /// </summary>
    private void HearBeat()
    {
        GameObject monster = service.monster;

        Vector3 monsterPos = new Vector3(monster.transform.position.x, 0, monster.transform.position.z);

        float magnitude = Vector3.Magnitude(transform.position - monsterPos);
        bool startPhase = false;
        if(magnitude<26)
        {

            startPhase = true;
            
        }

        //if a phase of the heartbeat has started, it will run until its finished, even if the mage runs out of the range of the monster.
        if (startPhase || heartbeatDelta > 0)
        {
            float heartBeatSpeed = 1;
            if (magnitude > 8)
            {
                heartBeatSpeed = (-2f / 45f) * magnitude + (61f / 45f);
                
            }

            float weight = 0;

            float sinX = heartbeatDelta * Mathf.PI * 2;

            float sinOne = Mathf.Sin(sinX) * 2 - 1;
            float sinTwo = Mathf.Sin(sinX - 1.2f) * 2 - 1;

            if (sinOne < 0) sinOne = 0;
            if (sinTwo < 0) sinTwo = 0;


            if (sinOne >= sinTwo)
            {
                weight = sinOne;
            }
            else
            {
                weight = sinTwo;
            }

            magesVolumeHeartBeat.weight = weight;

            if (heartBeatSpeed < 0.2f) heartBeatSpeed = 0.2f;
            heartbeatDelta += Time.deltaTime * heartBeatSpeed;
            if (heartbeatDelta > 1) heartbeatDelta = 0;
        }
        

    }

    /// <summary>
    /// Method to keep track of the Qu-spell cooldown, state and activation. 
    /// </summary>
    private void SpellMoveQu()
    {
        if (quCooldownTimer > 0)
        {
            quCooldownTimer -= Time.deltaTime;
        }
        if (quCooldownTimer <= 0)
        {
            if (quState == SpellState.Cooldown)
            {
                quCooldownTimer = 0;
                quState = SpellState.Up;
            }
            
        }
        if (Input.GetKeyDown("q")&& !isOnGround)
        {
            if (quScript.followState != SphereFollower.FollowState.stationary)
            {
              
                if (quCooldownTimer == 0) 
                {
                    if (isHealing || isReviving)
                    {
                        ResetHealingReviving();                      
                    }
                    
                    //create a ray cast and set it to the mouses cursor position in game
                    Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit screenRayHit;
                    //Moves the sphere to the given Position.If anything is in the way of the sphere, the sphere will be stopped there. The new position is calculated via raycast.
                    if (Physics.Raycast(screenRay, out screenRayHit, 40))
                    {
                        
                        Vector3 point = screenRayHit.point;
                        point.y += quScript.localHeight;
                        //localHeight wird benutzt um die Höhe auch auf höherem Boden zu behalten
                        Vector3 direction = new Vector3(point.x, point.y, point.z) - lightSphereQu.transform.position;
                        Ray ray = new Ray(lightSphereQu.transform.position, direction);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, Vector3.Magnitude(direction)))
                        {
                            
                            CmdMoveSphereToPosition(hit.point);
                            quScript.MoveToPosition(hit.point);
                        }
                        else
                        {
                            
                            CmdMoveSphereToPosition(point);
                            quScript.MoveToPosition(point);
                        }

                        
                        quState = SpellState.InAction;

                    }
                }               
            }
            else
            {              
                CmdMoveSphereBack();
                quState = SpellState.Cooldown;
                quCooldownTimer = QuCooldown;
            }       
        }
    }

    /// <summary>
    /// Call on server to synchronize the new position to all clients.
    /// </summary>
    /// <param name="point">Vector3 of the new point the sphere should move to.</param>
    [Command]
    private void CmdMoveSphereToPosition(Vector3 point)
    {       
            RpcMoveSphereToPosition(point);               
    }

    /// <summary>
    /// Server call to all clients except the calling client to synchronize the new Qu position.
    /// </summary>
    /// <param name="point">Vector3 of the new point the sphere should move to.</param>
    [ClientRpc]
    private void RpcMoveSphereToPosition(Vector3 point)
    {
        if(!_isLocalPlayer)quScript.MoveToPosition(point);
    }

    /// <summary>
    /// Server call to move Qu back to the mage.
    /// </summary>
    [Command]
    public void CmdMoveSphereBack()
    {
        RpcMoveSphereBack();
    }

    /// <summary>
    /// Server all to clients to move Qu back to the mage.
    /// </summary>
    [ClientRpc]
    private void RpcMoveSphereBack()
    {
        quScript.MoveBack();
    }

    /// <summary>
    /// Server call to clients to remove one life from the given mage. Method handles the healthstate of the mage.
    /// </summary>
    [ClientRpc]
    public void RpcRemoveLife()
    {

        if (lifePoints > 0)
        {
            lifePoints--;
            
                if (lifePoints < 2) volumeHitLerpActive = true;

                if (lifePoints == 0)
                {

                    if(_isLocalPlayer)EventMageOnGround?.Invoke();

                    OnOnGround();
                // if(getsRevivedOrHealed)

                if (_isLocalPlayer)
                {
                    ResetHealingReviving();

                    if (!revivable) IsDead = true;
                }
            }
            UpdateLight();
        }
    }

    /// <summary>
    /// Handles everything that has to happen, if the mage drops to the ground.
    /// </summary>
    private void OnOnGround()
    {
        animator.SetBool("onGround", true);

         if(_isLocalPlayer)CmdAddOnGroundPlayer();

        isOnGround = true;

        LockMovement(true);
        if (revivable)
        {
            onGroundTimer = 0;

            uiTrackerForMages.visible = true;
            uiTrackerForMages.progressionVisible = true;
            uiTrackerForMages.Progression = 0;
            uiTrackerForMages.visibleOnScreen = true;
        }      
    }

    /// <summary>
    /// Handles everything that has to happen, if the mage gets revived.
    /// </summary>
    private void OnRevive()
    {
        animator.SetBool("onGround", false);

        CmdRemoveOnGroundPlayer();

        revivable = false;
        isOnGround = false;
        LockMovement(false);


        uiTrackerForMages.visible = false;
        uiTrackerForMages.visibleOnScreen = false;
        uiTrackerForMages.progressionVisible = true;

    }
    /// <summary>
    /// Call to server that a mage should be healed.
    /// </summary>
    /// <param name="mageType"></param>
    [Command]
    public void CmdAddLife(int mageType)
    {
        service._allChars[mageType].gameObject.GetComponent<MagesController>().RpcAddLife();

    }
    /// <summary>
    /// Call from the server to add one life to the player.
    /// </summary>
    [ClientRpc]
    public void RpcAddLife()
    {
        if (lifePoints == 0&&revivable) OnRevive();
        if(lifePoints<2)lifePoints++;
        if (lifePoints == 2 && _isLocalPlayer) volumeHitLerpActive = false;
        UpdateLight();

    }



    /// <summary>
    /// Updates the light to the given lifestate.
    /// </summary>
    public void UpdateLight()
    {
        if(lifePoints > 0)
        {
            lightSphereQuLight.intensity = (stableLightIntesity * lifePoints) / 2;
            lightSphereQuLight.range = stableLightRange;
        }
        else
        {
            lightSphereQuLight.intensity = (stableLightIntesity) / 2;
            lightSphereQuLight.range = stableLightRange*0f;
        }
       
    }

    /// <summary>
    /// Checks how long the Mage has left before it cant be revived and is dead.
    /// </summary>
    private void CheckOnGround()
    {
        if (onGroundTimer < onGroundDuration&&!getsRevivedOrHealed)
        {
            onGroundTimer += Time.deltaTime;
            uiTrackerForMages.Progression = onGroundTimer / onGroundDuration;
        }
        if (onGroundTimer >= onGroundDuration)
        {
            IsDead = true;
        }
    }

    /// <summary>
    /// Coroutine for the death-fadeout.
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitDeathFadeOUt()
    {
        uiTrackerForMages.visibleOnScreen = true;
        uiTrackerForMages.visible = true;
        uiTrackerForMages.isXIconVisible = true;
        uiTrackerForMages.progressionVisible = false;


        uiTrackerForMonster.visibleOnScreen = true;
        uiTrackerForMonster.visible = true;
        uiTrackerForMonster.isXIconVisible = true;
        uiTrackerForMonster.progressionVisible = false;
        CmdSetXIconVisible();
        yield return new WaitForSeconds(6);

        CmdDeathFadeOut();
    }

    [Command]
    private void CmdSetXIconVisible()
    {
        RpcSetXIconVisible();
    }

    [ClientRpc]
    private void RpcSetXIconVisible()
    {
        if (!_isLocalPlayer)
        {
            uiTrackerForMages.visibleOnScreen = true;
            uiTrackerForMages.visible = true;
            uiTrackerForMages.isXIconVisible = true;
            uiTrackerForMages.progressionVisible = false;

            uiTrackerForMonster.visibleOnScreen = true;
            uiTrackerForMonster.visible = true;
            uiTrackerForMonster.isXIconVisible = true;
            uiTrackerForMonster.progressionVisible = false;
        }
    }

   
    /// <summary>
    /// Synchronization call to server.
    /// </summary>
    [Command]
    private void CmdDeathFadeOut()
    {
        RpcDeathFadeOut();
    }

    /// <summary>
    /// Synchronization call from server to all clients.
    /// </summary>
    [ClientRpc]
    private void RpcDeathFadeOut()
    {
        deathFadeOut = true;

    }

    /// <summary>
    /// Handles the fading out of a dead Charakter. Adds a dead player to the server if Charakter faded out.
    /// </summary>
    private void DeathFadeOut()
    {
        float alphaValue =1- deathFadeOutTimer / deathFadeOutTime;
        if (alphaValue < 0) alphaValue = 0;

        foreach(SkinnedMeshRenderer meshRenderer in meshRendererList)
        {
            Material[] materialsList = meshRenderer.materials;
            foreach (Material mat in materialsList)
            {
                mat.SetColor("_BaseColor", new Color(mat.GetColor("_BaseColor").r, mat.GetColor("_BaseColor").g, mat.GetColor("_BaseColor").b, alphaValue));
            }
        }
       

        if (alphaValue == 0)
        {
            lightSphereQuLight.intensity = 0;
            quScript.DropToGround();
            if (_isLocalPlayer&&!fadedOut)
            {

                fadedOut = true;
                GameObject.Find("MenuService").GetComponent<MenuService>().GameEndMonsterWonHandler();
                CmdAddDeadPlayer();
            }
            uiTrackerForMages.visible = false;
            uiTrackerForMages.visibleOnScreen = false;
            uiTrackerForMages.progressionVisible = false;
            uiTrackerForMages.isXIconVisible = false;

            uiTrackerForMonster.visible = false;
            uiTrackerForMonster.visibleOnScreen = false;
            uiTrackerForMonster.progressionVisible = false;
            uiTrackerForMonster.isXIconVisible = false;

            gameObject.transform.position = new Vector3(200,0,200);
        }
        deathFadeOutTimer += Time.deltaTime;
    }

    /// <summary>
    /// Synchronization call to server.
    /// </summary>
    [Command]
    private void CmdAddDeadPlayer()
    {
        
       
        gameManager.AddDeadPlayer();
        CmdRemoveClientAuthority();
    }


    [Command]
    public void CmdAddDeadPlayerAfterLeavingGame()
    {
        uiTrackerForMonster.visible = false;
        uiTrackerForMonster.visibleOnScreen = false;
        uiTrackerForMonster.progressionVisible = false;
        uiTrackerForMonster.isXIconVisible = false;

        lightSphereQu.transform.position = new Vector3(200, 0, 200);

     
        gameManager.AddDeadPlayer();

        CmdRemoveClientAuthority();
    }

    /// <summary>
    ///  Synchronization call to server.
    /// </summary>
    [Command]
    private void CmdAddOnGroundPlayer()
    {
        gameManager.AddOnGroundPlayer();
    }

    /// <summary>
    ///  Synchronization call to server.
    /// </summary>
    [Command]
    private void CmdRemoveOnGroundPlayer()
    {
        gameManager.RemoveOnGroundPlayer();
    }

    /// <summary>
    /// Handles the reviving/healing of other mages. 
    /// </summary>
    private void ReviveOrHeal()
    {

        anyOneInRange = false;
        PlayerService.Characters typeOfNearestPlayer = PlayerService.Characters.Monster;
        float minDistance = 3;
        GameObject targetPlayer = gameObject;// = new GameObject();
        MagesController controller;
        if (!isHealing && !isReviving)
        {

            foreach (PlayerService.NetworkBondedPlayer player in service._allChars)
            {
                if (player.type != PlayerService.Characters.Monster && player.type != type)
                {
                    controller = player.gameObject.GetComponent<MagesController>();
                    int lifePoints = controller.lifePoints;
                    if (!controller.getsRevivedOrHealed&&(lifePoints == 1 || (lifePoints == 0 && controller.revivable)))
                    {
                        float distance = Vector3.Magnitude(controller.transform.position - transform.position);
                        if (distance < minDistance)
                        {
                            anyOneInRange = true;
                            typeOfNearestPlayer = player.type;
                            targetPlayer = player.gameObject;
                        }
                    }
                }
            }
        }

          

        if (anyOneInRange|| isHealing || isReviving)
        {
            if (Input.GetKeyDown("r"))
            {
                if (!isHealing&&!isReviving)
                {
                    typeOfTargetedPlayer = (int)typeOfNearestPlayer;
                    targetedPlayer = targetPlayer;
                    CmdHealRevivePlayer(typeOfTargetedPlayer, true);
                    if (quState == SpellState.InAction)
                    {

                        quState = SpellState.Cooldown;
                        quCooldownTimer = QuCooldown;
                    }
                    targetedPlayer.GetComponent<MagesController>().EventMageOnGround += ResetHealingReviving;

                    if (targetPlayer.GetComponent<MagesController>().lifePoints == 1)
                    {
                        isHealing = true;
                        healBar.gameObject.SetActive(true);
                        healBar.value = 0;

                    }
                    else if(targetPlayer.GetComponent<MagesController>().lifePoints == 0)
                    {
                        isReviving = true;
                        reviveBar.gameObject.SetActive(true);
                        reviveBar.value = 0;

                    } 
                }
                else
                {
                    ResetHealingReviving();

                }
            }
        }

        if (isHealing)
        {
            if (healingTimer < healingTime)
            {
                healingTimer += Time.deltaTime;
                healBar.value += Time.deltaTime/healingTime;
            }
            if (healingTimer >= healingTime)
            {
                ResetHealingReviving();
                CmdAddLife(typeOfTargetedPlayer);
            }
        }
        else if (isReviving)
        {
            if (revivingTimer < revivingTime)
            {
                revivingTimer += Time.deltaTime;
                reviveBar.value += Time.deltaTime / revivingTime;
            }
            if (revivingTimer >= revivingTime)
            {
                ResetHealingReviving();
                CmdAddLife(typeOfTargetedPlayer);

            }
        }
    }

    /// <summary>
    /// Resets the healing or reviving of other players.
    /// </summary>
    private void ResetHealingReviving()
    {
        if (_isLocalPlayer)
        {
            if(targetedPlayer != null&&targetedPlayer.GetComponent<MagesController>().EventMageOnGround!=null) targetedPlayer.GetComponent<MagesController>().EventMageOnGround -= ResetHealingReviving;
            if(isHealing||isReviving)CmdHealRevivePlayer(typeOfTargetedPlayer, false);
            isHealing = false;
            isReviving = false;
            healingTimer = 0;
            revivingTimer = 0;
            healBar.gameObject.SetActive(false);
            reviveBar.gameObject.SetActive(false);
        }
       
    }

    /// <summary>
    ///  Synchronization call to server.
    /// </summary>
    /// <param name="type">Type of charakter.</param>
    /// <param name="state">True if heal/revive was successful, false if not.</param>
    [Command]
    private void CmdHealRevivePlayer(int type, bool state)
    {
        RpcHealRevivePlayer(type, state);
    }

    [ClientRpc]
    private void RpcHealRevivePlayer(int type, bool state)
    {
        if (state)
        {
            quScript.HealOrReviveMage((PlayerService.Characters)type);
            service._allChars[type].gameObject.GetComponent<MagesController>().getsRevivedOrHealed = true;
        }
        else
        {
            quScript.MoveBack();
            service._allChars[type].gameObject.GetComponent<MagesController>().getsRevivedOrHealed = false;
        }
        
    }





    /// <summary>
    /// Checks and calculates the monsters trancparency based on distance and visibility from the lightsources. Visibility is checked via Raycast from the closest and brightest Lightsource to the monster. If it hits,
    /// the monster is visible. If not, the next closest lightsource will be evaluated.
    /// </summary>
    private void CheckMonsterTranparency()
    {
        GameObject monster = service.monster;

        Vector3 monsterPos = new Vector3(monster.transform.position.x, 0, monster.transform.position.z);

        
       

        List<Tuple<float, Vector3>> positionDistanceList = new List<Tuple<float, Vector3>>();
        
       
        for (int i = 1; i < 5; i++) //evaluation of the lightSpheres
        {
            GameObject lightSphereQu = service._allChars[i].gameObject.GetComponent<MagesController>().lightSphereQu;
            Vector3 lightSpherePos = new Vector3(lightSphereQu.transform.position.x, 0, lightSphereQu.transform.position.z);
            float distance = Vector3.Magnitude(monsterPos - lightSpherePos);

            float distanceFromLightEdge = distance - lightSphereQu.transform.GetChild(0).GetComponent<Light>().range;

           
            InsertNewTuple(distanceFromLightEdge, lightSphereQu.transform.position, ref positionDistanceList);          

        }

        List<CrystalScriptLocal> crystalList = crystalService.crystalScriptList;


        for (int i = 0; i < crystalList.Count; i++) //evaluation of the crystals
        {
            
            Vector3 crystalPos = new Vector3(crystalList[i].transform.position.x, 0, crystalList[i].transform.position.z);
            float distance = Vector3.Magnitude(monsterPos - crystalPos);

            float distanceFromLightEdge = distance - crystalList[i].transform.GetComponentInChildren<Light>().range;
            crystalPos.y += 1.5f;
            InsertNewTuple(distanceFromLightEdge, crystalPos, ref positionDistanceList);
       
        }
        Material[] mats = monster.transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().materials;

        float alphaValue =0;


        for(int i = 0;i< positionDistanceList.Count; i++) //evalution of the distance List and raycasting
        {
            Tuple<float, Vector3> positionDistanceTuple = positionDistanceList[i];
            float lightDistance = positionDistanceList[i].Item1;
            Vector3 lightPosition = positionDistanceList[i].Item2;
            monsterPos = new Vector3(monster.transform.position.x, monster.transform.position.y+1.5f, monster.transform.position.z);//new Vector3(monsterPos.x, 1.5f, monsterPos.z);

            Vector3 direction = monsterPos - lightPosition;
            Ray ray = new Ray(lightPosition, direction);
            Debug.DrawRay(lightPosition, direction);
            RaycastHit hit;
            if (Vector3.Magnitude(direction) < 0.5f)
            {
                alphaValue = 1;
            }
            else
            {
                if (!Physics.Raycast(ray, out hit, Vector3.Magnitude(direction)))
                {
                    if (lightDistance >= 0)
                    {
                        alphaValue = 0;
                    }
                    else if (lightDistance > -visionTransition)
                    {
                        alphaValue = ((-lightDistance) / visionTransition)*0.9f;
                    }
                    else
                    {
                        alphaValue = 1*0.9f;
                    }
                    break;
                }
            }
        }  
        foreach (Material mat in mats)
        {
            mat.SetColor("_BaseColor", new Color(mat.GetColor("_BaseColor").r, mat.GetColor("_BaseColor").g, mat.GetColor("_BaseColor").b, alphaValue));
        }
    }

    /// <summary>
    /// Inserts a new tuple to the list. The tuple is inserted at an index, so that the floats from the tuples in the list are ordered
    /// </summary>
    /// <param name="distanceFromLightEdge">Distance from the Edge of the light.</param>
    /// <param name="position">Position of the lightsource.</param>
    /// <param name="positionDistanceList">The list holding all relevant position and distance information. All distances that are irrelevant for calculation are not in this list.</param>
    private void InsertNewTuple(float distanceFromLightEdge, Vector3 position, ref List<Tuple<float, Vector3>> positionDistanceList)
    {
        float minimumDistanceFromLightEdge = 0;
        if (positionDistanceList.Count >0) minimumDistanceFromLightEdge = positionDistanceList[0].Item1;

        if (distanceFromLightEdge < minimumDistanceFromLightEdge)
        {
            minimumDistanceFromLightEdge = distanceFromLightEdge;
            positionDistanceList.Insert(0, new Tuple<float, Vector3>(minimumDistanceFromLightEdge, position));
        }
        else if (distanceFromLightEdge < 0)
        {
            if (positionDistanceList[positionDistanceList.Count - 1].Item1 < distanceFromLightEdge)
            {
                positionDistanceList.Add(new Tuple<float, Vector3>(minimumDistanceFromLightEdge, position));
            }
            else
            {
                for (int j = 1; j < positionDistanceList.Count; j++)
                {
                    if (positionDistanceList[j].Item1 > distanceFromLightEdge)
                    {
                        positionDistanceList.Insert(j, new Tuple<float, Vector3>(minimumDistanceFromLightEdge, position));
                        break;
                    }
                }
            }
        }
    }



    /// <summary>
    ///  Synchronization call to server.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="lookAtPos">Position the GameObject should be rotated to.</param>
    [Command]
    public void CmdSetLaser(bool state, Vector3 lookAtPos)
    {
        RpcSetLaser(state, lookAtPos);

    }

    /// <summary>
    ///  Synchronization call from server to all clients.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="lookAtPos">Position the GameObject should be rotated to.</param>
    [ClientRpc]
    public void RpcSetLaser(bool state, Vector3 lookAtPos)
    {
        if(!_isLocalPlayer)SetLaser(state, lookAtPos);
    }

    /// <summary>
    /// Handles rotation of GameObject and the de-/activation of the LineRenderer and ParticleSystem component.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="lookAtPos">Position the GameObject should be rotated to.</param>
    public void SetLaser(bool state, Vector3 lookAtPos)
    {
        if (state)
        {
            laser.GetComponent<LineRenderer>().enabled = true;
            laser.GetComponentInChildren<ParticleSystem>().Play();

            transform.LookAt(lookAtPos);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            Vector3 crystalPos = new Vector3(lookAtPos.x, 0, lookAtPos.z);
            float dist = (crystalPos - transform.position).magnitude;
            laser.GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, 1.8f, dist - 1.6f));
            laser.GetComponentInChildren<ParticleSystem>().transform.localPosition = new Vector3(0, 1.6f, dist - 1.9f);

        }
        else
        {
            laser.GetComponent<LineRenderer>().enabled = false;
            laser.GetComponentInChildren<ParticleSystem>().Stop();
        }
        
    }

    public bool GetIsDead()
    {
        return isDead;
    }
}