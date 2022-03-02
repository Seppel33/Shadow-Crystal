using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereHover : MonoBehaviour
{
    private float counter;
    private Vector3 localPositionFixed;
    public bool isAlive = true;
    // Start is called before the first frame update
    void Start()
    {
        counter = Random.value;
        if (counter == 1f) counter = 0f;
        localPositionFixed = transform.localPosition;
    }

    public void SetDead()
    {
        isAlive = false;
        transform.localPosition = localPositionFixed;
    }

    // Update is called once per frame
    /// <summary>
    /// Lets the sphere hover up and down.
    /// </summary>
    void FixedUpdate()
    {
        if (isAlive)
        {
            Vector3 sinusHeight = new Vector3(0, Mathf.Sin(counter * 2 * Mathf.PI) * 0.2f, 0);
            counter += 0.01f;
            if (counter == 1f) counter = 0f;
            transform.localPosition = localPositionFixed + sinusHeight;
        }
       
       
    }
}
