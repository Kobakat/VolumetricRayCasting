using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public float speed;
    public Vector3 axis = Vector3.up;

    void Update()
    {
        this.transform.Rotate(axis, speed * Time.deltaTime);
    }
}
