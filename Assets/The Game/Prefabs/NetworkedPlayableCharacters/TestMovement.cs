using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovement : MonoBehaviour
{

    private Vector3 velocity;
    private Rigidbody rigidBody;
    public Animator animator;
    public float speed = 5f;
    protected Quaternion rotation;
    public int rotationSpeed = 720;
    protected virtual void OnEnable()
    {

        SetRagdollMode(false);
        rigidBody = transform.GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    


    }

    protected virtual void FixedUpdate()
    {
       
            Vector3 wasd = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (wasd.magnitude > 1) wasd = wasd.normalized;
            velocity = new Vector3(
                Mathf.Lerp(rigidBody.velocity.x, wasd.x * speed, 1),
                rigidBody.velocity.y,
                Mathf.Lerp(rigidBody.velocity.z, wasd.z * speed, 1)
                );
            velocity = new Vector3(wasd.x * speed, rigidBody.velocity.y, wasd.z * speed);


            if (wasd.magnitude > 0)
            {
                rotation = Quaternion.LookRotation(wasd);
                transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, rotation.eulerAngles.y, rotationSpeed * Time.deltaTime);
                

            }
        rigidBody.velocity = velocity;
        animator.SetFloat("forward", rigidBody.velocity.magnitude / speed);
            //Debug.Log("Velo" + rb.velocity.magnitude/maxSpeed);
        
    }

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
}
