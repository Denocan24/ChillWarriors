Shader "Custom/TerrainDiffuseOnly"
{
    Properties
    {
        [HideInInspector] _Control("AlphaMap", 2D) = "red" {}
        [HideInInspector] _Splat0("Layer 0", 2D) = "grey" {}
        [HideInInspector] _Splat1("Layer 1", 2D) = "grey" {}
        [HideInInspector] _Splat2("Layer 2", 2D) = "grey" {}
        [HideInInspector] _Splat3("Layer 3", 2D) = "grey" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry-100" }
        Pass
        {
            Name "TerrainDiffuse"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes { float4 posOS : POSITION; float2 uv : TEXCOORD0; float3 normalOS : NORMAL; };
            struct Varyings { float4 posCS : SV_POSITION; float2 uv : TEXCOORD0; float3 normalWS : TEXCOORD1; };

            TEXTURE2D(_Control); SAMPLER(sampler_Control);
            TEXTURE2D(_Splat0); SAMPLER(sampler_Splat0); float4 _Splat0_ST;
            TEXTURE2D(_Splat1); SAMPLER(sampler_Splat1); float4 _Splat1_ST;
            TEXTURE2D(_Splat2); SAMPLER(sampler_Splat2); float4 _Splat2_ST;
            TEXTURE2D(_Splat3); SAMPLER(sampler_Splat3); float4 _Splat3_ST;

            Varyings vert(Attributes IN)
            {
                Varyings o;
                o.posCS = TransformObjectToHClip(IN.posOS.xyz);
                o.uv = IN.uv;
                o.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 ctrl = SAMPLE_TEXTURE2D(_Control, sampler_Control, IN.uv);
                half4 col0 = SAMPLE_TEXTURE2D(_Splat0, sampler_Splat0, IN.uv * _Splat0_ST.xy + _Splat0_ST.zw);
                half4 col1 = SAMPLE_TEXTURE2D(_Splat1, sampler_Splat1, IN.uv * _Splat1_ST.xy + _Splat1_ST.zw);
                half4 col2 = SAMPLE_TEXTURE2D(_Splat2, sampler_Splat2, IN.uv * _Splat2_ST.xy + _Splat2_ST.zw);
                half4 col3 = SAMPLE_TEXTURE2D(_Splat3, sampler_Splat3, IN.uv * _Splat3_ST.xy + _Splat3_ST.zw);
                half3 albedo = col0.rgb * ctrl.r + col1.rgb * ctrl.g + col2.rgb * ctrl.b + col3.rgb * ctrl.a;

                Light mainLight = GetMainLight();
                half NdotL = saturate(dot(IN.normalWS, mainLight.direction));
                half3 diffuse = albedo * (mainLight.color * NdotL + unity_AmbientSky.rgb);
                return half4(diffuse, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
