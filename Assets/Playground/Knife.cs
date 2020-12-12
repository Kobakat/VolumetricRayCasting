using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knife : MonoBehaviour
{
    public Vector2 move = Vector2.zero;
    public float dst = 300;
    void Update()
    {
        this.transform.position = new Vector3(
            this.transform.position.x + move.x * Mathf.Sin(Time.time) * dst * Time.deltaTime,
            this.transform.position.y + move.y * Mathf.Sin(Time.time) * dst * Time.deltaTime, 
            this.transform.position.z);
    }
}
