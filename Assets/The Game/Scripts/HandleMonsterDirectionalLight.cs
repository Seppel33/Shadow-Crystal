using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleMonsterDirectionalLight : MonoBehaviour
{
    public Vector3 rotation;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
    }
}
