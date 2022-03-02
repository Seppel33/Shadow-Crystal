using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereFollower: MonoBehaviour
{
    public enum FollowState
    {
        stationary, //Sphere moves to one fixed point and stays there
        autoFollow, //Smoothly follows after the game
        healMage,   //Follows and rotates around the mage that should be healed
        dead        //drop to the ground and does nothing
    }

    private Vector3 localPositionFixed;
    private Vector3 localPositionInterpolated;
    public GameObject playerObject;

    public float localHeight;

    public FollowState followState;

    private GameObject mageToHeal;

    private PlayerService service;
   
    Vector3 sourcePosition;
    Vector3 targetPosition;

    private float rotationCounter;

    float intensity;
    // Start is called before the first frame update
    void Start()
    {     
        playerObject = gameObject.transform.parent.gameObject;
        localHeight = transform.localPosition.y;
        localPositionFixed = transform.localPosition;
        localPositionInterpolated = transform.localPosition;
        gameObject.transform.parent = gameObject.transform.parent.parent;
 
        //playerObject.GetComponent<MagesController>().lightSphereQu = gameObject;
        followState = FollowState.autoFollow;
        GetComponent<SphereCollider>().enabled = true;
        service = GameObject.Find("ServiceManager").GetComponent<PlayerService>();
       
       
    }

    /// <summary>
    /// Handles the sphere movement according to the <see cref="followState"/>. See <seealso cref="FollowState"/>
    /// </summary>
    void FixedUpdate()
    {
        Vector3 playerPosition;
        Vector3 directionVector;
        if (playerObject == null) Deactivate();
        switch (followState)
        {
           

            case FollowState.stationary:

                transform.position = Vector3.Lerp(transform.position, targetPosition, 0.08f);
                GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                break;

            case FollowState.autoFollow:
                
                //localPositionInterpolated = localPositionInterpolated + 0.05f * (playerObject.transform.rotation * localPositionFixed - localPositionInterpolated);
                localPositionInterpolated = localPositionInterpolated + 0.05f * (playerObject.transform.rotation * localPositionFixed - localPositionInterpolated);
                playerPosition = playerObject.transform.position + (localPositionInterpolated);
                directionVector = 0.15f * (playerPosition - transform.position);


                if (directionVector.magnitude > 1f) directionVector = directionVector.normalized * 1f;

                transform.position = transform.position + directionVector;

                GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                break;

            case FollowState.healMage:     
                
                if (rotationCounter > 1) rotationCounter = 0;
                playerPosition = mageToHeal.transform.position +new Vector3(Mathf.Sin(rotationCounter*2*Mathf.PI)*0.5f,2, Mathf.Cos(rotationCounter * 2 * Mathf.PI) * 0.5f);
                rotationCounter += 0.01f;

                directionVector = 0.15f * (playerPosition - transform.position);


                if (directionVector.magnitude > 1f) directionVector = directionVector.normalized * 1f;

                transform.position = transform.position + directionVector;
                GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                break;
            case FollowState.dead:
                   //do nothing

                break;
        }
      
       
    }

    /// <summary>
    /// handles the movement for healing the given mage.
    /// </summary>
    /// <param name="type"></param>
    public void HealOrReviveMage(PlayerService.Characters type )
    {
        followState = FollowState.healMage;
        GetComponent<SphereCollider>().enabled = false;
        mageToHeal = service._allChars[(int)type].gameObject;
    }

    /// <summary>
    /// Sets the new position that the Sphere should move to.
    /// </summary>
    /// <param name="point"></param>
    public void MoveToPosition(Vector3 point)
    {
        sourcePosition = new Vector3(transform.position.x,transform.position.y,transform.position.z);
        targetPosition = new Vector3(point.x,point.y,point.z);
        followState = FollowState.stationary;
        GetComponent<SphereCollider>().enabled = true;
    }

    /// <summary>
    /// Sets the followstate to autofollow, so that the sphere moves back.
    /// </summary>
    public void MoveBack()
    {
        followState = FollowState.autoFollow;
        GetComponent<SphereCollider>().enabled = true;
    }

    /// <summary>
    /// Sets the followstate to dead and deactivates gravity.
    /// </summary>
    public void DropToGround()
    {
        
        transform.GetComponentInChildren<SphereHover>().SetDead();
        followState = FollowState.dead;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        GetComponent<Rigidbody>().useGravity = true;
        StartCoroutine(Deactivate());
    }

    /// <summary>
    /// Coroutine to remove the sphere out of the playing field after the given time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(10);
        transform.position = new Vector3(200, 0, 200);
    }
}
