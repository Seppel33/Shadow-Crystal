using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FairyFylaController : MagesController
{

    private const PlayerService.Characters constType = PlayerService.Characters.Fairy;
    public float flyingCooldown = 10;
    public float flyingDuration = 4;

    public GameObject FylaRotation;
    public Animator FylaHoverAnim;
    public Animator FylaAnimator;
    public GameObject FlightParticleSystem;

    public Animator rightWing;
    public Animator leftWing;

    static float t = 0.0f;
    static float q = 0.0f;

    private float xRot;
    private float currentY;

    public bool isFlying = false;
    private bool isLanding;

    private List<GameObject> flyOverColliders;
    private List<GameObject> slideOverColliders;

    [Client]
    protected override void OnEnable()
    {
        base.OnEnable();
        AddPlayer(constType);
    }

    protected override void Start()
    {
        base.Start();
        otherSpellCooldown = flyingCooldown;
        otherSpellDuration = flyingDuration;
        GameObject[] flyOverGameObjects = GameObject.FindGameObjectsWithTag("FlyOverGameObjects");
        GameObject[] slideOverGameObjects = GameObject.FindGameObjectsWithTag("SlideOverGameObjects");
        
        flyOverColliders = new List<GameObject>(flyOverGameObjects);
        slideOverColliders = new List<GameObject>(slideOverGameObjects);

       
        //FylaHoverAnim.enabled = false;

        

        SetFlying(false);
    }
  

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void Update()
    {
        base.Update();
        if (_isLocalPlayer && isIngame)
        {
           FlyingSpell();

            if (isLanding)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(rigidBody.velocity, Vector3.up), Time.deltaTime * 100f);
                Vector3 direction = new Vector3(0, -1, 0);
                Ray ray = new Ray(transform.position, direction);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 2))
                {

                    if (hit.collider.tag == "Ground")
                    {
                        FairyLanding();


                    }
                }
            }
        }
        AnimatorRotation();
    }

    /// <summary>
    /// Due to the flying animation beeing rotated by 90 degrees, on animation transitions to different animations the GameObject has to be rotated in the opposite direction
    /// </summary>
    private void AnimatorRotation()
    {
        if (isFlying||isLanding)
        {
            xRot = Mathf.Lerp(0, -54, t);
            FylaRotation.transform.localRotation = Quaternion.Euler(xRot, 0, 0);
            //float yval = Mathf.Lerp(currentY, 0, t);
            //FylaHoverAnim.gameObject.transform.localPosition = new Vector3(0, yval,0);         

        }
        else// if (firstPress)
        {
            xRot = Mathf.Lerp(-54, 0, q);
            FylaRotation.transform.localRotation = Quaternion.Euler(xRot, 0, 0);
            float yval = Mathf.Lerp(currentY, 0, q);
            FylaHoverAnim.gameObject.transform.localPosition = new Vector3(0, yval, 0);
        }

        t += 3.5f * Time.deltaTime;
        q += 2.5f * Time.deltaTime;
    }

    /// <summary>
    /// Keeps track of the FlyingSpell cooldown, state and activation. Starts a coroutine on activation.  
    /// </summary>
    private void FlyingSpell()
    {
        if (otherSpellCooldownTimer > 0)
        {
            otherSpellCooldownTimer -= Time.deltaTime;
        }
        if (otherSpellCooldownTimer <= 0)
        {
            if (otherState == SpellState.Cooldown)
            {
                otherState = SpellState.Up;
                otherSpellCooldownTimer = 0;
            }
               
        }
        if (Input.GetKeyDown("e") && otherSpellCooldownTimer == 0 && !isCharging&&!isOnGround&&otherState == SpellState.Up)
        {
            otherState = SpellState.InAction;
            StartCoroutine(StartFlying());
            
        }
    }

    /// <summary>
    /// Coroutine for the Flying-Spell.
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartFlying()
    {
        if (!isFlying)
        {

            LockMovement(false);
            SetFlying(true);
            CmdSetFlying(true);


            Physics.IgnoreLayerCollision(2, 9, true);
            foreach (GameObject collider in flyOverColliders)
            {
                collider.SetActive(true);
            }


            yield return new WaitForSeconds(flyingDuration);
      
            Physics.IgnoreLayerCollision(2, 9, false);
            foreach (GameObject collider in flyOverColliders)
            {
                collider.SetActive(false);
            }
            Vector3 direction = new Vector3(0, -1, 0);
            Ray ray = new Ray(transform.position, direction);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 2))
            {
                
                if (hit.collider.tag == "SlideOverGameObjects")
                {
                    isLanding = true;
                    movementLocked = true;
                    transform.position = new Vector3(transform.position.x, transform.position.y + 0.001f, transform.position.z);
                    
                }
                else
                {
                    FairyLanding();
                }
            }
            else
            {
                FairyLanding();
            }
            otherSpellCooldownTimer = flyingCooldown;
            otherState = SpellState.Cooldown;

        }
    }

    /// <summary>
    /// Handles the fairy landing.
    /// </summary>
    private void FairyLanding()
    {
        movementLocked = false;
        isLanding = false;
        SetFlying(false);
        CmdSetFlying(false);

        foreach (GameObject collider in flyOverColliders)
        {
            collider.SetActive(true);
        }

    }

    /// <summary>
    /// Call to server to synchronize the flying state.
    /// </summary>
    /// <param name="isFlying">true => flying, false => not flying</param>
    [Command]
    private void CmdSetFlying(bool isFlying)
    {
        RpcSetFlying(isFlying);
    }

    /// <summary>
    /// Server call to all Clients to synchronize the flying state.
    /// </summary>
    /// <param name="isFlying">true => flying, false => not flying</param>
    [ClientRpc]
    private void RpcSetFlying(bool isFlying)
    {
        if(!_isLocalPlayer)SetFlying(isFlying);
    }


    /// <summary>
    /// Sets the animationstate and particle system.
    /// </summary>
    /// <param name="isFlying"></param>
    private void SetFlying(bool isFlying)
    {
        t = 0;
        q = 0;
        this.isFlying = isFlying;
        if (isFlying)
        {
            FylaAnimator.SetBool("flyingBoi", true);
            //FylaHoverAnim.enabled = true;
            FylaHoverAnim.SetBool("Hover", true);
            currentY = FylaHoverAnim.gameObject.transform.position.y;
            leftWing.speed = 2;
            rightWing.speed = 2;
            FlightParticleSystem.SetActive(true);
        }
        else
        {
            FylaAnimator.SetBool("flyingBoi", false);
            //FylaHoverAnim.enabled = false;
            //FylaHoverAnim.StopPlayback();
            FylaHoverAnim.SetBool("Hover", false);
            currentY = FylaHoverAnim.gameObject.transform.position.y;
            leftWing.speed = 1;
            rightWing.speed = 1;
            FlightParticleSystem.SetActive(false);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("FairyPassThrough")||collision.transform.CompareTag("Ground"))
        {
            if(isLanding) FairyLanding();
        }
    }
}
