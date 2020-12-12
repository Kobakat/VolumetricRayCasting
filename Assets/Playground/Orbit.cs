using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public float speed;  

    void Update()
    {
        this.transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }
}
