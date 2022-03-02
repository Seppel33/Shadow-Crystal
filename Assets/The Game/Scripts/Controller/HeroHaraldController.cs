using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroHaraldController : MagesController
{
    public GameObject trapPrefab;

    private const PlayerService.Characters constType = PlayerService.Characters.Hero;

    public float trapCooldown = 10;
    public float trapDuration = 2.3f;

    private float animationDelta;


    [Client]
    protected override void OnEnable()
    {
        base.OnEnable();
        AddPlayer(constType);
    }

    protected override void Start()
    {
        base.Start();
        otherSpellCooldown = trapCooldown;
        otherSpellDuration = trapDuration;
    }
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
    }

    protected override void Update()
    {
        base.Update();
        if (_isLocalPlayer && isIngame)
        {
            TrapSpell();
        }

    }

    /// <summary>
    /// Keeps track of the Trap-spell cooldown, state and activation. Starts a coroutine on activation.  
    /// </summary>
    private void TrapSpell()
    {
        animationDelta += Time.deltaTime;
        if(animationDelta >1)animator.SetBool("SetTrap", false);
        if (otherSpellCooldownTimer > 0)
        {
            otherSpellCooldownTimer -= Time.deltaTime;
        }
        if (otherSpellCooldownTimer <= 0)
        {
            if (otherState == SpellState.Cooldown)
            {
                otherSpellCooldownTimer = 0;
                otherState = SpellState.Up;
            }
            
        }

        if(Input.GetKeyDown("e") && otherSpellCooldownTimer == 0 &&!isCharging&&otherState==SpellState.Up&&!isOnGround)
        {

            animator.SetBool("SetTrap", true);
            animationDelta = 0;
            
            LockMovement(true);
            otherState = SpellState.InAction;
            StartCoroutine(WaitAnimation());

            
        }
    }

    /// <summary>
    /// Coroutine for the trap spell
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitAnimation()
    {
        yield return new WaitForSeconds(trapDuration);
        CmdHaraldPlaceTrap(transform.position);
        otherSpellCooldownTimer = trapCooldown;
        LockMovement(false);
        otherState = SpellState.Cooldown;
    }

    /// <summary>
    /// Call to server that a new trap should me spawned on all Clients.
    /// </summary>
    /// <param name="position"></param>
    [Command]
    public void CmdHaraldPlaceTrap(Vector3 position)
    {

        GameObject newTrap = Instantiate(trapPrefab, position, transform.rotation);
        NetworkServer.Spawn(newTrap);
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
}
