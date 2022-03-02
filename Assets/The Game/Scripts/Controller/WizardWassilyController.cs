using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardWassilyController : MagesController
{

    private const PlayerService.Characters constType = PlayerService.Characters.Wizard;
    public GameObject FlashParticleSystem;
    public float flashCooldown = 10;

    public float flashDuration = 5;

    [Client]
    protected override void OnEnable()
    {
        base.OnEnable();
        AddPlayer(constType);
    }

    protected override void Start()
    {
        base.Start();

        otherSpellCooldown = flashCooldown;
        otherSpellDuration = flashDuration;
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
            FlashSpell();
        }
    }

    /// <summary>
    /// Handles the cooldown, state and activation of the flash spell. On activation there is a servercall to synchronize the spell to everyone.
    /// </summary>
    private void FlashSpell()
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

        if (Input.GetKeyDown("e") && otherSpellCooldownTimer == 0&&otherState == SpellState.Up && !isOnGround)
        {
            CmdFlashSpell(flashDuration);
            otherState = SpellState.Cooldown;
            otherSpellCooldownTimer = flashCooldown;
        }
    }

    /// <summary>
    /// Call to server to trigger the flash.
    /// </summary>
    /// <param name="duration">Duration of the flash.</param>
    [Command]
    private void CmdFlashSpell(float duration)
    {
        RpcFlashSpell(duration);
    }

    /// <summary>
    /// Server call to clients to trigger the flash. If the Client is in controll of the monster the flash intensity is distinctly higher.
    /// </summary>
    /// <param name="duration">Duration of the flash.</param>
    [ClientRpc]
    private void RpcFlashSpell(float duration)
    {
        if(service.LocalPlayer.GetComponent<AbstractController>().Type == PlayerService.Characters.Monster)
        {
            lightSphereQuLight.range *= 2;
            lightSphereQuLight.intensity = stableLightIntesity * 15000;
            StartCoroutine(WaitSpellDuration(duration));
        }
        else
        {
            lightSphereQuLight.range *= 2;
            lightSphereQuLight.intensity = stableLightIntesity * 2;
            StartCoroutine(WaitSpellDuration(duration));
        }
        FlashParticleSystem.SetActive(true);
    }

    /// <summary>
    /// Coroutine to handle the end of the flash spell.
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator WaitSpellDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        FlashParticleSystem.SetActive(false);
        UpdateLight();
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
