﻿Shader "Custom/Raymarch"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
        SubShader
    {
        // No culling or depth
        Cull Off ZWrite On ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0

            #include "UnityCG.cginc"
            #include "SDFunc.cginc"

            //Scene info
            sampler2D _MainTex;
            uniform fixed4 _MainColor;
            uniform float4x4 _Frustum;
            uniform float4x4 _CamMatrix;
            uniform float3 _Light;
            uniform int _OperationCount;


            struct shape
            {
                float3 position;
                int shape;
                float3 color;

                float sphereRadius;

                float3 boxDimensions;

                float3 roundBoxDimensions;
                float roundBoxFactor;

                float torusInnerRadius;
                float torusOuterRadius;

                float coneHeight;
                float2 coneRatio;
            };

            struct operation
            {
                int operation;
                int childCount;

                float blendStrength;
            };



            StructuredBuffer<operation> operations;

            StructuredBuffer<shape> shapes;

            //How many times each ray is marched
            //Higher values give higher resolution (and potentially longer draw distances) but lower performance
            static const int maxSteps = 100;

            //How close does a ray have to get to be consider a hit
            //Higher values give a sharper definition of shape but lower performance
            static const float epsilon = 0.01;

            //The maximum distance we want a ray to be from the nearest surface before giving up
            //Higher values give a longer draw distance but lower performance
            static const float maxDist = 100;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ray : TEXCOORD1;
            };

            struct ray
            {
                float3 origin;
                float3 direction;
                float3 position;
                float depth;
            };

            float4 GetShape(float3 p, int index)
            {
                shape s = shapes[index];

                s.position = p - s.position;

                float3 col = s.color;
                float dst = 1;

                switch (s.shape)
                {
                    case 0:
                        dst = sdSphere(s.position, s.sphereRadius);
                        break;
                    case 1:
                        dst = sdBox(s.position, s.boxDimensions);
                        break;
                    case 2:
                        dst = sdTorus(s.position, s.torusInnerRadius, s.torusOuterRadius);
                        break;
                    case 3:
                        dst = sdCone(s.position, s.coneRatio, s.coneHeight);
                        break;
                    case 4:
                        dst = sdRoundBox(s.position, s.roundBoxDimensions, s.roundBoxFactor);
                        break;
                }

                return float4(col, dst);
            }

            float4 GetOperation(float3 p, int index)
            {
                operation o = operations[index];

                int startIndex = 0;

             
                for (int i = 0; i < index; i++)
                {
                    startIndex += operations[i].childCount;
                }
                          
                float4 shapeValue = GetShape(p, startIndex);

                for (int j = 1; j < o.childCount; j++) 
                {
                    switch (o.operation)
                    {
                        case 0:
                            shapeValue = opAdd(shapeValue, GetShape(p, startIndex + j));
                            break;
                        case 1:
                            shapeValue = opSubtract(shapeValue, GetShape(p, startIndex + j));
                            break;
                        case 2:
                            shapeValue = opIntersect(shapeValue, GetShape(p, startIndex + j));
                            break;
                        case 3:
                            shapeValue = opBlend(shapeValue, GetShape(p, startIndex + j), o.blendStrength);
                            break;
                    }
                }
                
                return shapeValue;
            }

            
            float4 SurfaceDistance(float3 p)
            {
                float4 surfValue = GetOperation(p, 0);
             
                for (int i = 1; i < _OperationCount; i++)
                {                 
                    surfValue = opAdd(surfValue, GetOperation(p, i));
                }
                    
                return surfValue;
            }

            //For a signed distances field, the normal of any given point is defined as the gradient of the distance field
            //As such, subtracting the distance field of a slight smaller value by a slight large value produces a good approximation
            //This function is exceptionally expensive as it requires 6 more calls of a sign distance function PER PIXEL hit
            float3 CalculateNormal(float3 p)
            {

                float x = SurfaceDistance(float3(p.x + epsilon, p.y, p.z)).w - SurfaceDistance(float3(p.x - epsilon, p.y, p.z)).w;
                float y = SurfaceDistance(float3(p.x, p.y + epsilon, p.z)).w - SurfaceDistance(float3(p.x, p.y - epsilon, p.z)).w;
                float z = SurfaceDistance(float3(p.x, p.y, p.z + epsilon)).w - SurfaceDistance(float3(p.x, p.y, p.z - epsilon)).w;

                return normalize(float3(x,y,z));
            }

            //For each pixel on the screen
            fixed4 raymarch(ray r)
            {
                //Start with a completely transparent pixel
                fixed4 pixelColor = fixed4(0, 0, 0, 0);
                //Cast out a ray at the pixel's UV coordinate
                float dst = 0;

                //For a maximum of <maxStep> times,
                for (int i = 0; i < maxSteps; i++)
                {
                    //Determine the distance from the nearest shape in the scene
                    r.position = r.origin + r.direction * dst;
                    float4 surfDist = SurfaceDistance(r.position);

                    //If the distance is sufficently small...
                    if (surfDist.w < epsilon)
                    {
                        //We "hit" the surface. Calculate the normal vector of the pixel and shade it based on the angle from the rays of light
                        float3 n = CalculateNormal(r.position);

                        //This uses the lambertian model of lighting https://en.wikipedia.org/wiki/Lambertian_reflectance
                        float light = dot(-_Light.xyz, n);

                        pixelColor = fixed4(surfDist.rgb * light, 1);
                        break;
                    }

                    //If the distance is not sufficently small, we missed.
                    //Move the ray's position forward and try again
                    dst += surfDist.w;


                    //If the distance is very large or a mesh is in the way
                    //we give up and break early

                    if (dst > maxDist || dst >= r.depth)
                        break;
                }

                //Give the frag function the color we want the pixel to be
                return pixelColor;

            }

            v2f vert(appdata v)
            {
                v2f o;

                half index = v.vertex.z;
                v.vertex.z = 0;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                o.ray = _Frustum[(int)index].xyz;

                //Normalize along the z-axis
                //Absval function prevents scene from inverting
                o.ray /= abs(o.ray.z);

                //Places ray in worldspace so the depth buffer is calculated properly
                o.ray = mul(_CamMatrix, o.ray);
                return o;
            }

            uniform sampler2D _CameraDepthTexture;

            //Runs for every pixel on the screen
            fixed4 frag(v2f i) : SV_Target
            {
                ray r;

                //https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
                r.direction = normalize(i.ray.xyz);
                r.origin = _WorldSpaceCameraPos;

                r.depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
                r.depth *= length(i.ray.xyz);


                //The color of the pixel before any post processing done by the raymarch shader
                fixed3 base = tex2D(_MainTex, i.uv);

                //The color of the pixel after the raymarch function
                fixed4 col = raymarch(r);

                //Alpha blending function, derived via https://en.wikipedia.org/wiki/Alpha_compositing#Alpha_blending
                return fixed4(base * (1.0 - col.w) + col.xyz * col.w, 1.0);
            }
        
        ENDCG
    }
    
}

}