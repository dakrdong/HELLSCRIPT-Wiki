// Hostile ground telegraph decal. Mesh contract: UV.x = normalised radius (circle, sector, ring) or distance along a line,
// UV.y = angle (0..1 around) or side (0..1 across). The whole footprint shows faintly from the start and a brighter fill
// grows outward with _Progress (0..1 windup), so imminence reads by shape as well as colour. _Edges.x draws an inner
// edge at UV.x = 0 (rings, the near end of lines), _Edges.y draws side edges at UV.y = 0 and 1 (sectors, lines).
// Draws after friendly ground effects (Transparent+10) without depth writes, faded by the fog of war like WorldFx.
Shader "HELLSCRIPT/World Telegraph"
{
    Properties
    {
        _TintColor("Tint", Color) = (1.37,.44,.31,1)
        _Progress("Windup progress", Range(0,1)) = 0
        _Edges("Edges: x inner, y sides", Vector) = (0,0,0,0)
        [ToggleUI] _FogEnabled("Fog of war", Float) = 0
        _FogTex("Exploration", 2D) = "black" {}
        [HideInInspector] _FogBounds("Fog bounds", Vector) = (0,0,1,1)
        [HideInInspector] _FogSize("Fog size", Vector) = (1,1,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+10" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Pass
        {
            Name "WorldTelegraph"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_FogTex); SAMPLER(sampler_FogTex);
            CBUFFER_START(UnityPerMaterial)
            float4 _TintColor, _Edges, _FogBounds, _FogSize;
            float _Progress, _FogEnabled;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float3 positionWS:TEXCOORD1; };
            Varyings Vert(Attributes i) { Varyings o;o.positionWS=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);o.uv=i.uv;return o; }
            half FogCurrent(float3 positionWS)
            {
                if (_FogEnabled<.5) return 1;
                float2 uv=(positionWS.xz-_FogBounds.xy)/_FogBounds.zw;
                half inside=step(0,uv.x)*step(0,uv.y)*step(uv.x,1)*step(uv.y,1);
                half2 fog=SAMPLE_TEXTURE2D_LOD(_FogTex,sampler_FogTex,uv,0).rg;
                return fog.g*smoothstep(.02,.98,fog.r)*inside;
            }
            // 1 on a line about two pixels wide at distance d (in UV units, whose pixel size is w).
            half Line(float d, float w) { return 1-saturate(d/(w*2)); }
            half4 Frag(Varyings i):SV_Target
            {
                float2 uv=i.uv,w=max(fwidth(uv),1e-5);float t=_Time.y,progress=saturate(_Progress);
                half pulse=.8+.2*sin(t*lerp(5,16,progress));
                half edge=max(Line(1-uv.x,w.x),smoothstep(.8,1,uv.x)*.45);
                edge=max(edge,_Edges.x*Line(uv.x,w.x));
                edge=max(edge,_Edges.y*Line(min(uv.y,1-uv.y),w.y));
                edge*=pulse*(.85+.15*sin((uv.y*24-t*1.5)*6.2831853));
                half fill=(1-smoothstep(progress-w.x*2,progress+w.x*2,uv.x))*step(.001,progress);
                half front=Line(abs(uv.x-progress),w.x)*step(.001,progress)*step(progress,.999);
                half pattern=step(.72,frac(uv.x*7-t*.6))*.05;
                half alpha=saturate(.1+pattern+fill*(.22+.18*progress)+front*.7+edge*.9)*_TintColor.a*FogCurrent(i.positionWS);
                return half4(_TintColor.rgb*(.7+fill*.3+front+edge*1.6),alpha);
            }
            ENDHLSL
        }
    }
}
