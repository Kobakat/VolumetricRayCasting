using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Operation : MonoBehaviour
{
    public enum OpFunction
    {
        None,
        Subtract,
        Intersect,
        Blend
    }

    public OpFunction operation;
    public int childCount;

    public float blendStrength;
}

public struct OperationInfo
{
    public int operation;
    public int childCount;

    public float blendStrength;

    public static int GetSize()
    {
        return sizeof(int) * 2 + sizeof(float) * 1;
    }
}
