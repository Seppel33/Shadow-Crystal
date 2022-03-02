using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class ShadowMonsterController : MonstersController
{
    private const PlayerService.Characters constType = PlayerService.Characters.Monster;

    private float autoAttackCooldown = 3;
    private float autoAttackCooldownDelta;

    public float shadowVisionCooldown = 60;
    public float shadowVisionDuration = 10;
    public float shadowVisionTimer;

    public SpellState shadowVisionState = SpellState.Up;

    public SphereCollider hitCollider;

    public Volume volumeStandard;
    public Volume volumeRedVision;

    private List<ParticleSystemRenderer> playerTrails;


    private float volumeLerp = 0;
    private bool volumeLerpActive;

    public GameObject meshOpaque;
    public GameObject meshTransparent;

    private float animationDelta;


    [Client]
    protected override void OnEnable()
    {
        base.OnEnable();
        AddPlayer(constType);
    }

    /// <summary>
    /// Gets called if the Charakter was picked by the local client.
    /// </summary>
    protected override void OnStartLocalPlayerCustom()
    {
        base.OnStartLocalPlayerCustom();

        volumeStandard.gameObject.SetActive(true);
        volumeRedVision.gameObject.SetActive(true);
        volumeStandard.enabled = true;
        volumeRedVision.enabled = true;

        volumeRedVision.weight = 0;


        meshOpaque.SetActive(true);
        meshTransparent.SetActive(false);
        

        playerTrails = new List<ParticleSystemRenderer>();

        foreach(PlayerService.NetworkBondedPlayer mage in service._allChars) //Activates the particleSystem of the mages playerTrails. 
        {
            if(mage.type != PlayerService.Characters.Monster)
            {
                ParticleSystemRenderer particleSystemRenderer = mage.gameObject.GetComponent<MagesController>().playerTrailRenderer;
                particleSystemRenderer.gameObject.SetActive(true);
                playerTrails.Add(particleSystemRenderer);
                particleSystemRenderer.renderingLayerMask = 0;
            }
        }

    }

    protected override void Start()
    {
        base.Start();
        autoAttackCooldownDelta = -autoAttackCooldown;
        
    }
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
    }

    protected override void Update()
    {
        base.Update();
        if (this._isLocalPlayer && isIngame)
        {
           
            CheckStrikeEnemys();

            PlayerTrails();

            VolumeLerp();

            Dancing();
           
        }
        

    }

    /// <summary>
    /// A really special method, that lets the monster dance :3
    /// </summary>
    private void Dancing()
    {
        bool dancing = false;
        if (Input.GetKeyDown("o"))
        {
            dancing = true;
           
        }
        animator.SetBool("dancing", dancing);
    }

    /// <summary>
    /// Handles the cooldown, state and activation of the Monster-spell. Starts coroutine if it gets activated.
    /// </summary>
    private void PlayerTrails()
    {
        if(shadowVisionTimer > 0)
        {
            shadowVisionTimer -= Time.deltaTime;
        }
        if (shadowVisionTimer <= 0)
        {
            if (shadowVisionState == SpellState.Cooldown)
            {
                shadowVisionTimer = 0;
                shadowVisionState = SpellState.Up;
            }
        }
            if (Input.GetKeyDown("q"))
        {
            if (shadowVisionTimer <= 0)
            {
                StartCoroutine(PlayerTrailsCoroutine());


            }
        }
    }

    /// <summary>
    /// Handles the activated Monsterspell. Sets the playerTrails to active and starts the lerp to the Volume for the monster-spell
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayerTrailsCoroutine()
    {
        shadowVisionState = SpellState.InAction;
                      
        volumeLerpActive = true;
        foreach (ParticleSystemRenderer particle in playerTrails)
        {
            particle.renderingLayerMask = 1;
        }

        yield return new WaitForSeconds(shadowVisionDuration);

        foreach (ParticleSystemRenderer particle in playerTrails)
        {
            particle.renderingLayerMask = 0;
        }
        
        volumeLerpActive = false;
        shadowVisionTimer = shadowVisionCooldown;
        shadowVisionState = SpellState.Cooldown;
    }


    /// <summary>
    /// Lerps between the standard monster volume and the monsterSpellVolume.
    /// </summary>
    private void VolumeLerp()
    {
        if (volumeLerpActive)
        {
            volumeLerp += Time.deltaTime;
            if (volumeLerp > 1) volumeLerp = 1;
            volumeRedVision.weight = volumeLerp;
        }
        else
        {
            volumeLerp -= Time.deltaTime;
            if (volumeLerp < 0) volumeLerp = 0;
            volumeRedVision.weight = volumeLerp;
        }
    }


    /// <summary>
    /// Handles the cooldown, state, and the activation of the strike.
    /// </summary>
    private void CheckStrikeEnemys()
    {
        animationDelta += Time.deltaTime;
        if(animationDelta >1 )animator.SetBool("attack", false);
        if (Time.time - autoAttackCooldownDelta > autoAttackCooldown)
        {
            if (Input.GetMouseButtonDown(0) && !animator.GetCurrentAnimatorStateInfo(0).IsName("attack"))
            {
                animator.SetBool("attack", true);
                animationDelta = 0;

                autoAttackCooldownDelta = Time.time;
            }
        }
    }


    //Gets called on an animation event, that gets raised in the attackanimation of the monster.
    public void CheckStrikeHitbox()
    {
        if(_isLocalPlayer) base.CmdCheckForCollision();

    }

    

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
    }

    /// <summary>
    /// Handles the root of the monster on collision with the trap.
    /// </summary>
    /// <param name="trap"></param>
    public void TrapRoot(GameObject trap)
    {
        LockMovement(true);
        StartCoroutine(WaitAnimation(trap));
    }

    /// <summary>
    /// Coroutine to destroy trap and unlock monstermovement after trapduration.
    /// </summary>
    /// <param name="trap">GameObject of the particular trap.</param>
    /// <returns></returns>
    private IEnumerator WaitAnimation(GameObject trap)
    {
        yield return new WaitForSeconds(trapRootDuration);
        CmdDestroyTrap(trap);
        LockMovement(false);
    }

   
    /// <summary>
    /// Server call to destory the triggered trap.
    /// </summary>
    /// <param name="trap"></param>
    [Command]
    public void CmdDestroyTrap(GameObject trap)
    {
        Destroy(trap);
    }
}