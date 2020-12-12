using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpCol : MonoBehaviour
{
    RaymarchShape shape = null;

    private void Start()
    {
        shape = GetComponent<RaymarchShape>();     
    }

    void Update()
    { 
        shape.color.r = Mathf.Sin(Time.time);
        shape.color.g = Mathf.Cos(Time.time);
        shape.color.b = Mathf.Cos(Time.time) * Mathf.Sin(Time.time) * 2;      
    }


}
