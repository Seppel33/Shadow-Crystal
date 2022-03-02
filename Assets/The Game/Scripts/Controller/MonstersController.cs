using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class MonstersController : AbstractController
{
    public float trapRootDuration = 3;

    [SerializeField]
    private SkinnedMeshRenderer meshRenderer;

    protected List<Collider> otherColliderBodys = new List<Collider>();

    public List<Collider> OtherColliderBodys { get => otherColliderBodys; set => otherColliderBodys = value; }

    public ParticleSystem hitSystem;
    public ParticleSystem missSystem;

    private List<Collider> activeColliderBodys = new List<Collider>();
    [Client]
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void Start()
    {
        base.Start();
    }
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
    }
    protected override void Update()
    {
        base.Update();          
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
    /// Call to server to check if the hitCollider of the monster collided with a mage. Calls the <see cref="MagesController.RpcRemoveLife"/> script if a mage was hit.
    /// </summary>
    [Command]
    protected void CmdCheckForCollision()
    {
        bool hitted = false;
        if (activeColliderBodys.Count > 0)
        {
            foreach (Collider collider in activeColliderBodys)
            {
                bool hit = true;
                if(collider.GetComponent<AbstractController>().Type == PlayerService.Characters.Fairy)
                {
                    if (collider.GetComponent<FairyFylaController>().isFlying) hit = false;
                }
                if (hit)
                {
                    collider.gameObject.GetComponent<MagesController>().RpcRemoveLife();
                    hitted = true;
                }
            }
        }

        RpcHitOrMiss(hitted);
    }

    /// <summary>
    /// Plays the hit or miss particle system on all clients.
    /// </summary>
    /// <param name="isHit"></param>
    [ClientRpc]
    private void RpcHitOrMiss(bool isHit)
    {
        if (isHit)
        {
            hitSystem.Stop();
            hitSystem.Play();
        }
        else
        {
            missSystem.Stop();
            missSystem.Play();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

    }
    private void OnCollisionExit(Collision collision)
    {

    }

    [Server]
    private void OnTriggerEnter(Collider other)
    {
       
        if (otherColliderBodys.Contains(other))
        {
            activeColliderBodys.Add(other);          
        }
    }
    [Server]
    private void OnTriggerExit(Collider other)
    {
        if (otherColliderBodys.Contains(other))
        {
            activeColliderBodys.Remove(other);            
        }
    }

}