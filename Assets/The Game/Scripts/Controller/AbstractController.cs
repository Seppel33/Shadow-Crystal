using Mirror;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractController : NetworkBehaviour
{

    public enum SpellState
    {
        Up,
        InAction,
        Cooldown
    }
    
    protected Vector3 localCameraPosition = new Vector3(0, 14, -10);
    protected Vector3 oldCameraPosition = new Vector3(0,0,0);
    public float speed = 4f;
    protected Rigidbody rigidBody;
    protected Quaternion rotation;
    public int rotationSpeed = 720;
    public Animator animator;
    protected NetworkAnimator networkAnimator;
    protected Vector3 velocity;
    protected bool isOnGround;

    protected PlayerManager playerManager;
    public PlayerManager PlayerManager
    {
        get
        {
            return playerManager;
        }
        protected set
        {
            playerManager = value;
        }
    }
    protected PlayerService service;

    public bool _isLocalPlayer;

    protected PlayerService.Characters type;

    public PlayerService.Characters Type { get => type;protected set => type = value; }

    public GameManagerService gameManager;

    protected bool isIngame;
    public bool movementLocked;

    
    protected virtual void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gameObject, UnityEngine.SceneManagement.SceneManager.GetSceneAt(1));
        service = GameObject.Find("ServiceManager").GetComponent<PlayerService>();
        gameManager = GameObject.Find("ServiceManager").GetComponent<GameManagerService>();
        SetRagdollMode(false);
        rigidBody = transform.GetComponent<Rigidbody>();
        if(animator==null)animator = GetComponentInChildren<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();

        
    }

    protected void AddPlayer(PlayerService.Characters type)
    {
        this.type = type;     
        service.AddNewPlayer(gameObject, type);
    }

    protected virtual void Start()
    {
       
    }

    
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        _isLocalPlayer = hasAuthority;
        OnStartLocalPlayerCustom();
        playerManager = service.LocalPlayerManager.GetComponent<PlayerManager>();
        playerManager.player = gameObject;
        service.LocalPlayer = gameObject;

        if (isServer && networkAnimator != null) networkAnimator.clientAuthority = false;


        GetComponent<Rigidbody>().useGravity = true;
        Physics.IgnoreLayerCollision(2, 10, true);

        GetComponent<Rigidbody>().constraints = (RigidbodyConstraints)((int)RigidbodyConstraints.FreezeRotationX+ (int)RigidbodyConstraints.FreezeRotationY+ (int)RigidbodyConstraints.FreezeRotationZ);

       


    }

    protected virtual void Update()
    {
        if (gameManager.gameState == GameManagerService.GameState.InGame)
        {
            if (!isIngame)
            {
                isIngame = true;
                if (!service.connectedPlayers[(int)type])
                {
                    transform.position = new Vector3(200, 0, 200);
                    if(type != PlayerService.Characters.Monster)
                    {
                        MagesController controller = (MagesController)this;
                        controller.lightSphereQu.transform.position = new Vector3(200, 0, 200);
                    }
                }
            }
            
        }
        else
        {
            isIngame = false;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (this._isLocalPlayer&&isIngame&&!movementLocked)
        {

            MovementRoutine();
        }
    }

    /// <summary>
    /// Handles the movement for all Characters
    /// </summary>
    private void MovementRoutine()
    {
        Vector3 wasd = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (wasd.magnitude > 1) wasd = wasd.normalized;

        velocity = new Vector3(wasd.x * speed, rigidBody.velocity.y, wasd.z * speed);


        if (wasd.magnitude > 0)
        {
            rotation = Quaternion.LookRotation(wasd);
            transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, rotation.eulerAngles.y, rotationSpeed * Time.deltaTime);


        }
        rigidBody.velocity = velocity;
        animator.SetFloat("forward", rigidBody.velocity.magnitude / speed);
    }


    /// <summary>
    /// Locks or unlocks the movement of the given player and sets the velocity to zero.
    /// </summary>
    /// <param name="state">The new state of movement. True == Locked, False == Unlocked</param>
    public void LockMovement(bool state)
    {
        
        if (state)
        {
            rigidBody.velocity = Vector3.zero;
            animator.SetFloat("forward", rigidBody.velocity.magnitude / speed);
        }
        if (!isOnGround||state)
        {
            movementLocked = state;
        }
       
    }

    /// <summary>
    /// unused
    /// </summary>
    /// <param name="isDead"></param>
    private void SetRagdollMode(bool isDead)
    {
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            if (c.gameObject.name.StartsWith("mixamorig:"))
                c.enabled = isDead;
        }
        foreach (Rigidbody r in GetComponentsInChildren<Rigidbody>())
        {
            if (r.gameObject.name.StartsWith("mixamorig:"))
                r.isKinematic = !isDead;
        }
        GetComponent<Rigidbody>().isKinematic = isDead;
        GetComponent<Collider>().enabled = !isDead;
        GetComponentInChildren<Animator>().enabled = !isDead;
    }


    /// <summary>
    /// Starts for the Client that has the Authority of this GameObject
    /// </summary>
    protected virtual void OnStartLocalPlayerCustom()
    {
        if (Camera.main != null)
        {
            Camera.main.orthographic = false;
            //Camera.main.transform.SetParent(transform);
            Camera.main.transform.position = transform.position + localCameraPosition;


            Camera.main.transform.eulerAngles = new Vector3(90f, 0f, 0f);
        }
        

       
    }

    [Command]
    public void CmdRemoveClientAuthority()
    {
        gameObject.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        transform.position = new Vector3(200, 0, 200);
    }

    protected virtual void OnDisable()
    {
        if (_isLocalPlayer)
        {
            /*
            Camera.main.orthographic = true;
            Camera.main.transform.SetParent(null);
            Camera.main.transform.localPosition = new Vector3(0f, 70f, 0f);
            Camera.main.transform.localEulerAngles = new Vector3(90f, 0f, 0f);*/
        }
    }

    protected virtual void LateUpdate()
    {
        if (this._isLocalPlayer && isIngame)
        {
            
                float cameraHeight = Camera.main.transform.position.y + 0.05f * ((transform.position.y + localCameraPosition.y) - Camera.main.transform.position.y);
                Camera.main.transform.position = transform.position + localCameraPosition;
                Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, cameraHeight, Camera.main.transform.position.z);
                Camera.main.transform.rotation = Quaternion.identity;
                Camera.main.transform.rotation = Quaternion.Euler(60, 0, 0);
            
        }
    }
}