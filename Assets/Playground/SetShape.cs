using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetShape : MonoBehaviour
{
    public float swapDelay = .25f;

    float currentTime = 0;
    void Update()
    {
        if(Time.time > currentTime + swapDelay)
        {
            this.GetComponent<RaymarchShape>().shape = (RaymarchShape.Shape)Random.Range(0, 5);
            currentTime = Time.time;
        }
        
    }
}
