          Shader "Instanced/InstancedBinMesh" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        _WaterColor ("Water Color", color) = (1,1,1,1)
        _WoodColorNew ("Wood Color (Thriving)", color) = (1,1,1,1)
        _WoodColorOld ("Wood Color (Dying)", color) = (1,1,1,1)
        _FireColor ("Fire Color", color) = (1,1,1,1)
        _SeedColor ("Seed Color", color) = (1,1,1,1)
        _EarthColor ("Earth Color", color) = (1,1,1,1)
        _MetalColor ("Metal Color", color) = (1,1,1,1)
    }
    SubShader {
        Tags { "RenderType"="Transparent" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Unlit alpha:fade 
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup
        
        #include "UnityPBSLighting.cginc"

        sampler2D _MainTex;
        fixed4 _WaterColor;
        fixed4 _WoodColorNew;
        fixed4 _WoodColorOld;
        fixed4 _FireColor;
        fixed4 _EarthColor;
        fixed4 _MetalColor;
        fixed4 _SeedColor;

        struct Input {
            float2 uv_MainTex;
        };
        
         half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten) {
           half4 c;
           c.rgb = s.Albedo;
           c.a = s.Alpha;
           return c;
         }

    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        StructuredBuffer<int> _BinCountBuffer;
    #endif
    
        float _BinSize;
        float _BoundsSize;
        
        //https://fgiesen.wordpress.com/2009/12/13/decoding-morton-codes/
// "Insert" two 0 bits after each of the 10 low bits of x
uint Part1By2(uint x)
{
  x &= 0x000003ff;                  // x = ---- ---- ---- ---- ---- --98 7654 3210
  x = (x ^ (x << 16)) & 0xff0000ff; // x = ---- --98 ---- ---- ---- ---- 7654 3210
  x = (x ^ (x <<  8)) & 0x0300f00f; // x = ---- --98 ---- ---- 7654 ---- ---- 3210
  x = (x ^ (x <<  4)) & 0x030c30c3; // x = ---- --98 ---- 76-- --54 ---- 32-- --10
  x = (x ^ (x <<  2)) & 0x09249249; // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
  return x;
}

// Inverse of Part1By2 - "delete" all bits not at positions divisible by 3
uint Compact1By2(uint x)
{
  x &= 0x09249249;                  // x = ---- 9--8 --7- -6-- 5--4 --3- -2-- 1--0
  x = (x ^ (x >>  2)) & 0x030c30c3; // x = ---- --98 ---- 76-- --54 ---- 32-- --10
  x = (x ^ (x >>  4)) & 0x0300f00f; // x = ---- --98 ---- ---- 7654 ---- ---- 3210
  x = (x ^ (x >>  8)) & 0xff0000ff; // x = ---- --98 ---- ---- ---- ---- 7654 3210
  x = (x ^ (x >> 16)) & 0x000003ff; // x = ---- ---- ---- ---- ---- --98 7654 3210
  return x;
}


uint EncodeMorton3(uint x, uint y, uint z)
{
  return (Part1By2(z) << 2) + (Part1By2(y) << 1) + Part1By2(x);
}


/*
uint EncodeMorton3(uint x, uint y, uint z) {
    float cellSize = _BinSize;
    float lengthPerAxis = _BoundsSize * 2.0;
    uint binsPerAxis = ceil(lengthPerAxis / cellSize);
    
    return x * binsPerAxis * binsPerAxis + y * binsPerAxis + z;
}
*/

uint DecodeMorton3X(uint code)
{
  return Compact1By2(code >> 0);
}

uint DecodeMorton3Y(uint code)
{
  return Compact1By2(code >> 1);
}

uint DecodeMorton3Z(uint code)
{
  return Compact1By2(code >> 2);
}

        void rotate2D(inout float2 v, float r)
        {
            float s, c;
            sincos(r, s, c);
            v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
        }

        void setup()
        {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            //GPUParticle data = _ParticleBuffer[unity_InstanceID];
            
            int count = _BinCountBuffer[unity_InstanceID];
            
            //float3 positiveShiftedPosition = particles[id].position + float3(boundsSize, boundsSize, boundsSize);
            //int3 binCoords = int3(floor(positiveShiftedPosition.x / cellSize), floor(positiveShiftedPosition.y / cellSize), floor(positiveShiftedPosition.z / cellSize));
            //uint binIndex = EncodeMorton3(binCoords.x, binCoords.y, binCoords.z);
            
            uint3 binCoords = uint3(DecodeMorton3X(unity_InstanceID), DecodeMorton3Y(unity_InstanceID), DecodeMorton3Z(unity_InstanceID));
            float3 pos = binCoords * float3(_BinSize, _BinSize, _BinSize) - float3(_BoundsSize, _BoundsSize, _BoundsSize) + float3(0.5,0.5,0.5) * _BinSize;

            //float rotation = data.w * data.w * _Time.y * 0.5f;
            //rotate2D(data.xz, rotation);
            
            float scale = saturate((float)count / 32.0);
            scale = _BinSize;

            unity_ObjectToWorld._11_21_31_41 = float4(scale, 0, 0, 0);
            unity_ObjectToWorld._12_22_32_42 = float4(0, scale, 0, 0);
            unity_ObjectToWorld._13_23_33_43 = float4(0, 0, scale, 0);
            unity_ObjectToWorld._14_24_34_44 = float4(pos, 1);
            unity_WorldToObject = unity_ObjectToWorld;
            unity_WorldToObject._14_24_34 *= -1;
            unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
        #endif
        }

        half _Glossiness;
        half _Metallic;

        void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = 1.0;
            
            o.Alpha = 1.0;
            
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            o.Alpha = 0.0 + saturate((float)_BinCountBuffer[unity_InstanceID] / 1024.0) * 1.0;
        #endif
            
            
        }
        ENDCG
    }
    FallBack "Diffuse"
}