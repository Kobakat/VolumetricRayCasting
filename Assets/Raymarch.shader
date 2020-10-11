Shader "Custom/Raymarch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            uniform float4x4 _Frustum;
            uniform float4x4 _CamMatrix;
            uniform float4x4 _MainColor;
            

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
            };

            //Describes the distance from a sphere centered on P
            float sdSphere(float3 p, float r)
            {
                //HACK
                //length uses sqrt function, far too expensive in iterative multipixel function
                return length(p) - r;
            }


            //This function will later be adjusted to handle more shapes & different kinds
            //For now it will just draw the distance from a sphere
            float SurfaceDistance(float3 p)
            {
                //HACK
                //radius should not be a magic number
                return(sdSphere(p, 1));
                
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
                    float3 pos = r.origin + r.direction * dst;                  
                    float surfDist = SurfaceDistance(pos);

                    
                    //If the distance is sufficently small...
                    if (surfDist < epsilon)
                    {
                        //We "hit" the surface. Color the pixel.
                        pixelColor = fixed4(1, 1, 1, 1);

                        break;
                    }

                    //If the distance is not sufficently small, we missed.
                    //Move the ray's position forward and try again
                    dst += surfDist;
                    
                    
                    //If the distance is very large, we give up and break early
                    if (dst > maxDist)
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

                o.ray = mul(_CamMatrix, o.ray);
                return o;
            }
            
            //Runs for every pixel on the screen
            fixed4 frag(v2f i) : SV_Target
            {
                ray r;
                
                //https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
                r.direction = normalize(i.ray.xyz);
                r.origin = _WorldSpaceCameraPos;           
                
                fixed4 col = raymarch(r);
             
                return col;
            }
            ENDCG
        }
    }
}
