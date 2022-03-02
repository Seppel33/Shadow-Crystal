using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RogueReyController : MagesController
{

    private const PlayerService.Characters constType = PlayerService.Characters.Rogue;

    [SyncVar]
    private bool speedyBoi = false;
    public float speedBoostCooldown = 10;

    public float speedBoostDuration = 5;

    public ParticleSystem runningParticles;
    
    public float speedIncrement = 2;
    private int rotationSpeedMultiplier = 2;
    [Client]
    protected override void OnEnable()
    {
        base.OnEnable();
        AddPlayer(constType);
    }

    protected override void Start()
    {
        base.Start();
        otherSpellCooldown = speedBoostCooldown;
        otherSpellDuration = speedBoostDuration;
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
            SpeedSpell();
        }

        RunningParticles();
       
    }

    /// <summary>
    /// Handles cooldown, state and activation of the speed-spell. Starts coroutine if the spell is activated.
    /// </summary>
    private void SpeedSpell()
    {
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

        if (Input.GetKeyDown("e") && otherSpellCooldownTimer == 0&&otherState==SpellState.Up&&!isOnGround)
        {
            Debug.Log("Speedy");
            otherState = SpellState.InAction;
            StartCoroutine(BecomeSpeedyBoi());
        }      
    }

    /// <summary>
    /// Handles the particle system for the running spell.
    /// </summary>
    private void RunningParticles()
    {
        if (rigidBody.velocity.magnitude > 0 && speedyBoi)
        {
            runningParticles.Play();
        }
        else if (runningParticles.isPlaying)
        {
            runningParticles.Stop();
        }
    }

    /// <summary>
    /// Coroutine for the speed-spell. 
    /// </summary>
    /// <returns></returns>
    private IEnumerator BecomeSpeedyBoi()
    {
        if (!speedyBoi)
        {
            speedyBoi = true;
            speed += speedIncrement;
            rotationSpeed *= rotationSpeedMultiplier;
            animator.SetBool("speedyBoi", true);
            runningParticles.Play();

            yield return new WaitForSeconds(speedBoostDuration); //Waits for the speedBoostDuration in seconds. Then it turns everything back to normal.
            speedyBoi = false;
            speed -= speedIncrement;
            rotationSpeed /= rotationSpeedMultiplier;
            animator.SetBool("speedyBoi", false);
            runningParticles.Stop();
            otherState = SpellState.Cooldown;
            otherSpellCooldownTimer = speedBoostCooldown;

        }
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
