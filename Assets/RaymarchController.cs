﻿using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]

public class RaymarchController : SceneViewFilter
{
    public Color _MainColor;

    [SerializeField] Shader _Shader = null;

    Material _material;
    Camera _cam;
    Transform _light;

    List<ComputeBuffer> disposeBuffers;
    List<Operation> operations;
    List<RaymarchShape> shapes;

    int operationCount;
    public Material Material
    {
        get
        {
            if (!_material && _Shader)
                _material = new Material(_Shader);         
            return _material;
        }
    }

    public Camera Cam
    {
        get
        {
            if (!_cam)
                _cam = GetComponent<Camera>();
            //Sometimes unity cameras don't render depth texture by default?
            //I spent a good hour and a half figuring that out...
            _cam.depthTextureMode = DepthTextureMode.Depth;
            return _cam;
        }
    }

    public Transform Light
    {
        get
        {
            Light l;

            if (!_light)
            {
                l = (Light)FindObjectOfType(typeof(Light));
                
                if(!l)
                {
                    return _light;
                }

                _light = l.transform;
            }
                
            return _light;
        }
    }

    static void Blit(RenderTexture source, RenderTexture destination, Material mat, int pass)
    {
        RenderTexture.active = destination;
        mat.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho();
        mat.SetPass(pass);

        GL.Begin(GL.QUADS);

        //Bottom Left
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f);

        //Bottom Right
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f);

        //Top Right
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f);

        //Top Left
        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);

        GL.End();
        GL.PopMatrix();
    }

    //Unity event function, called when an image is done rendering to apply post processing effects
    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        disposeBuffers = new List<ComputeBuffer>();

        if (!Material)
        {
            Graphics.Blit(source, destination);
            return;
        }

        //Passing Scene Values to raymarch shader
        Material.SetMatrix("_Frustum", GetFrustum(Cam));
        Material.SetMatrix("_CamMatrix", Cam.cameraToWorldMatrix);
        Material.SetColor("_MainColor", _MainColor);
        Material.SetVector("_Light", Light ? Light.forward : Vector3.down);
        Material.SetInt("operationCount", operationCount);

        FillBuffer();

        Blit(source, destination, Material, 0);

        foreach (ComputeBuffer buffer in disposeBuffers)
        {
            buffer.Dispose();
        }
    }

    //Returns a matrix containing the corner positions of the camera's view frustum
    Matrix4x4 GetFrustum(Camera cam)
    {
        Matrix4x4 corners = Matrix4x4.identity;

        float camFOV = cam.fieldOfView;
        float camAr = cam.aspect;

        float camRatio = Mathf.Tan(camFOV * .5f * Mathf.Deg2Rad);

        Vector3 right = Vector3.right * camRatio * camAr;
        Vector3 up = Vector3.up * camRatio;

        Vector3 TL = (-Vector3.forward - right + up);
        Vector3 TR = (-Vector3.forward + right + up);
        Vector3 BR = (-Vector3.forward + right - up);
        Vector3 BL = (-Vector3.forward - right - up);

        corners.SetRow(0, TL);
        corners.SetRow(1, TR);
        corners.SetRow(2, BR);
        corners.SetRow(3, BL);

        return corners;
    }

    void FillBuffer()
    {
        //operations = new List<Operation>();
        //shapes = new List<RaymarchShape>();

        operations = new List<Operation>(FindObjectsOfType<Operation>());
        shapes = new List<RaymarchShape>(FindObjectsOfType<RaymarchShape>());

        operationCount = operations.Count;

        for(int i = 0; i < operations.Count; i++)
        {
            operations[i].childCount = shapes.Count;
            
            for(int j = 0; j < operations[i].childCount; j++)
            {
                shapes[j].index = i;
            }
        }

        OperationInfo[] opInfo = new OperationInfo[operations.Count];

        for (int i = 0; i < operations.Count; i ++)
        {
            Operation o = operations[i];

            opInfo[i] = new OperationInfo()
            {
                operation = (int)o.operation,
                childCount = o.childCount,
                blendStrength = o.blendStrength
            };            
        }

        ShapeInfo[] shapeInfo = new ShapeInfo[shapes.Count];

        for (int i = 0; i < shapes.Count; i++)
        {
            RaymarchShape s = shapes[i];

            shapeInfo[i] = new ShapeInfo()
            {
                position = s.transform.position,
                shape = (int)s.shape,

                sphereRadius = s.sphereRadius,

                boxDimensions = s.boxDimensions,

                roundBoxDimensions = s.roundBoxDimensions,
                roundBoxFactor = s.roundBoxFactor,

                torusInnerRadius = s.torusInnerRadius,
                torusOuterRadius = s.torusOuterRadius,

                coneHeight = s.coneHeight,
                coneRatio = s.coneRatio,

                index = s.index
            };
        }        

        ComputeBuffer opBuffer = new ComputeBuffer(opInfo.Length, OperationInfo.GetSize());
        ComputeBuffer shapeBuffer = new ComputeBuffer(shapeInfo.Length, ShapeInfo.GetSize());

        opBuffer.SetData(opInfo);
        shapeBuffer.SetData(shapeInfo);

        Material.SetBuffer("operations", opBuffer);
        Material.SetBuffer("shapes", shapeBuffer);

        disposeBuffers.Add(opBuffer);
        disposeBuffers.Add(shapeBuffer);

        
    }

}
