          Shader "Instanced/InstancedSurfaceShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        _WaterColor ("Water Color", color) = (1,1,1,1)
        _WoodColor ("Wood Color", color) = (1,1,1,1)
        _FireColor ("Fire Color", color) = (1,1,1,1)
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model
        #pragma surface surf Standard addshadow fullforwardshadows
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup

        sampler2D _MainTex;
        fixed4 _WaterColor;
        fixed4 _WoodColor;
        fixed4 _FireColor;

        struct Input {
            float2 uv_MainTex;
        };
        
        /*
        struct GPUParticle {
            float3 position;
            float  density;
            float3 velocity;
            float  pressure;
            float3 force;
            //float  temperature;
            uint   type; // 0 = earth, 1 = water, 2 = fire, 3 = wood, 4 = metal
        };
        */
        
        struct GPUParticle {
            float3 position;
            float  density;
            float3 velocity;
            float  pressure;
            float3 force;
            uint   type; // 0 = earth, 1 = water, 2 = fire, 3 = wood, 4 = metal
            float  temperature;
            float  remainingLifetime;
            float  pad0;
            float  pad1;
        }; 

    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        StructuredBuffer<GPUParticle> _ParticleBuffer;
    #endif

        void rotate2D(inout float2 v, float r)
        {
            float s, c;
            sincos(r, s, c);
            v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
        }

        void setup()
        {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            GPUParticle data = _ParticleBuffer[unity_InstanceID];

            //float rotation = data.w * data.w * _Time.y * 0.5f;
            //rotate2D(data.xz, rotation);

            unity_ObjectToWorld._11_21_31_41 = float4(1, 0, 0, 0);
            unity_ObjectToWorld._12_22_32_42 = float4(0, 1, 0, 0);
            unity_ObjectToWorld._13_23_33_43 = float4(0, 0, 1, 0);
            unity_ObjectToWorld._14_24_34_44 = float4(data.position, 1);
            unity_WorldToObject = unity_ObjectToWorld;
            unity_WorldToObject._14_24_34 *= -1;
            unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
        #endif
        }

        half _Glossiness;
        half _Metallic;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            GPUParticle data = _ParticleBuffer[unity_InstanceID];
            if (data.type == 0) {
                o.Albedo = _WaterColor;
            } else if (data.type == 1) { 
                o.Albedo = _FireColor;
            } else if (data.type == 2) {
                o.Albedo = _WoodColor;
            }
        #endif
            
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}