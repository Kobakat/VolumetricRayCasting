using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]

public class RaymarchController : SceneViewFilter
{
    
    Material _material;
    Camera _cam;
    Transform _light;

    List<ComputeBuffer> disposeBuffers;
    List<Operation> operations;
    List<RaymarchShape> shapes;
    int operationCount;

    #region Exposed Props
    [SerializeField] Shader _Shader = null;

    public bool darkMode = false;
    public bool useLighting = true;
    #endregion

    #region Filter Props
    public Color emissiveColor = Color.white;

    public int highlightGradient = 20;
    public float highlightStrength = 3.0f;
    public float nonHighlightStrength = 0.5f;

    public enum Filter { None, Highlight }
    public enum HighlightType { ShapeColor, SingleColor }


    public Filter filter = Filter.None;
    public HighlightType highlightType = HighlightType.ShapeColor;
    #endregion

    #region Light Props

    public enum LightMode { Lambertian, CelShaded }

    public LightMode lightMode = LightMode.Lambertian;

    public float unlitMultiplier = 0.5f;
    public float litMultiplier = 1.0f;
    public float flipAngle = 90.0f;

    public bool customAngle = false;
    #endregion

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

        SetMaterialProperties();
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
        operations = new List<Operation>(FindObjectsOfType<Operation>());

        operationCount = operations.Count;

        shapes = new List<RaymarchShape>(FindObjectsOfType<RaymarchShape>());
        
        for (int i = 0; i < operations.Count; i++)
        {
            operations[i].childCount = operations[i].transform.childCount;
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
                color = new Vector3(s.color.r, s.color.g, s.color.b),

                sphereRadius = s.sphereRadius,

                boxDimensions = s.boxDimensions,

                roundBoxDimensions = s.roundBoxDimensions,
                roundBoxFactor = s.roundBoxFactor,

                torusInnerRadius = s.torusInnerRadius,
                torusOuterRadius = s.torusOuterRadius,

                coneHeight = s.coneHeight,
                coneRatio = s.coneRatio,
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

        operations.Clear();
        shapes.Clear();    
    }

    void SetMaterialProperties()
    {
        #region Scene
        Material.SetMatrix("_Frustum", GetFrustum(Cam));
        Material.SetMatrix("_CamMatrix", Cam.cameraToWorldMatrix);
        Material.SetVector("_Light", Light ? Light.forward : Vector3.down);
        Material.SetInt("_OperationCount", operationCount);
        #endregion

        #region Filter
        Material.SetVector("_EmissiveColor", emissiveColor);
        Material.SetInt("_UseLight", Convert.ToInt32(useLighting));
        Material.SetInt("_DarkMode", Convert.ToInt32(darkMode));
        Material.SetInt("_HighlightGradient", highlightGradient);
        Material.SetInt("_Filter", (int)filter);
        Material.SetInt("_Highlight", (int)highlightType);
        Material.SetFloat("_HighlightStrength", highlightStrength);
        Material.SetFloat("_NonHighlightStrength", nonHighlightStrength);
        #endregion

        #region Lighting
        Material.SetInt("_LightMode", (int)lightMode);
        Material.SetFloat("_FlipAngle", flipAngle);
        Material.SetFloat("_LitMultiplier", litMultiplier);
        Material.SetFloat("_UnlitMultiplier", unlitMultiplier);
        Material.SetInt("_CustomAngle", Convert.ToInt32(customAngle));
        #endregion
    }
}
