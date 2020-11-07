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
}

public struct OperationInfo
{
    public int operation;
    public int childCount;

    public static int GetSize()
    {
        return sizeof(int) * 2;
    }
}
