// Unlit particles and FX meshes. One shader covers additive (SrcAlpha One) and alpha (SrcAlpha OneMinusSrcAlpha)
// blending through _SrcBlend/_DstBlend, so blending needs no keyword variants. Fades act on alpha only.
// Flipbooks come from the particle system's Texture Sheet Animation (it rewrites UV0) or from _MainTex_ST on meshes.
// _FogEnabled = 1 fades FX outside current sight so nothing draws over fogged terrain; RiftFogView.Resolve sets it.
// Soft fade against the scene needs _SoftFade > 0 on the material and the global keyword _WORLDFX_SOFT_DEPTH, which the
// camera owner enables only for a pipeline asset with a depth texture (PC). Mobile_RPAsset has none, so it stays off there
// and the default variant never touches _CameraDepthTexture.
Shader "HELLSCRIPT/World FX"
{
    Properties
    {
        [MainTexture] _MainTex("Texture", 2D) = "white" {}
        _TintColor("Tint", Color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Destination blend", Float) = 1
        [ToggleUI] _ZWrite("Depth write", Float) = 0
        _SoftFade("Soft fade distance (needs global keyword _WORLDFX_SOFT_DEPTH)", Float) = 0
        [ToggleUI] _FogEnabled("Fog of war", Float) = 0
        _FogTex("Exploration", 2D) = "black" {}
        [HideInInspector] _FogBounds("Fog bounds", Vector) = (0,0,1,1)
        [HideInInspector] _FogSize("Fog size", Vector) = (1,1,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Pass
        {
            Name "WorldFx"
            Tags { "LightMode"="UniversalForward" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ _WORLDFX_SOFT_DEPTH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #if defined(_WORLDFX_SOFT_DEPTH)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #endif
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_FogTex); SAMPLER(sampler_FogTex);
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST, _TintColor, _FogBounds, _FogSize;
            float _SrcBlend, _DstBlend, _ZWrite, _SoftFade, _FogEnabled;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; float3 positionWS:TEXCOORD1; half fog:TEXCOORD2; };
            Varyings Vert(Attributes i)
            {
                Varyings o;o.positionWS=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);
                o.uv=TRANSFORM_TEX(i.uv,_MainTex);o.color=i.color;o.fog=ComputeFogFactor(o.positionCS.z);return o;
            }
            half FogCurrent(float3 positionWS)
            {
                if (_FogEnabled<.5) return 1;
                float2 uv=(positionWS.xz-_FogBounds.xy)/_FogBounds.zw;
                half inside=step(0,uv.x)*step(0,uv.y)*step(uv.x,1)*step(uv.y,1);
                half2 fog=SAMPLE_TEXTURE2D_LOD(_FogTex,sampler_FogTex,uv,0).rg;
                return fog.g*smoothstep(.02,.98,fog.r)*inside;
            }
            half4 Frag(Varyings i):SV_Target
            {
                half4 color=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*_TintColor*i.color;
                color.a*=FogCurrent(i.positionWS);
                #if defined(_WORLDFX_SOFT_DEPTH)
                if (_SoftFade>0)
                {
                    float2 screen=GetNormalizedScreenSpaceUV(i.positionCS);float raw=SampleSceneDepth(screen);float scene;
                    // The world camera is orthographic, where LinearEyeDepth does not apply.
                    if (unity_OrthoParams.w>.5)
                    {
                        #if UNITY_REVERSED_Z
                        raw=1-raw;
                        #endif
                        scene=lerp(_ProjectionParams.y,_ProjectionParams.z,raw);
                    }
                    else scene=LinearEyeDepth(raw,_ZBufferParams);
                    color.a*=saturate((scene+TransformWorldToView(i.positionWS).z)/_SoftFade);
                }
                #endif
                // Additive FX fade to black in distance fog, blended FX to the fog colour.
                color.rgb=MixFogColor(color.rgb,abs(_DstBlend-1)<.5?half3(0,0,0):unity_FogColor.rgb,i.fog);
                return color;
            }
            ENDHLSL
        }
    }
}
