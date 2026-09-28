// World lit shader for every rift, town and legacy surface. The fog-of-war contract is unchanged:
// R = explored, G = in current sight; remembered terrain is drawn black until explored and grey while out of sight,
// everything else is clipped outside current sight (or as a whole through _ObjectVisibility >= 0).
// _FogEnabled = 0 skips all of that (town, legacy rifts, previews); RiftFogView.Resolve sets it to 1.
// _GlowColor is masked by vertex colour alpha; a mesh without a colour stream reads alpha 1 and glows all over.
// No colour here (or in WorldFx/WorldTelegraph) is [HDR]: every colour is a gamma value that Unity linearises like
// _BaseColor, values above 1 included. That keeps WorldView's emissive colours (URP Lit, converted by RiftFogView)
// at the brightness the old shader gave them; [HDR] would skip the conversion and wash them out.
Shader "HELLSCRIPT/Rift Terrain Visibility"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo (A = smoothness)", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        [Normal] _BumpMap("Normal", 2D) = "bump" {}
        _BumpScale("Normal scale", Float) = 1
        _Smoothness("Smoothness", Range(0,1)) = .15
        _Metallic("Metallic", Range(0,1)) = 0
        _EmissionColor("Emission", Color) = (0,0,0,1)
        _GlowColor("Glow (x vertex colour alpha)", Color) = (0,0,0,1)
        _RimColor("Rim", Color) = (0,0,0,1)
        _RimPower("Rim power", Range(.5,8)) = 3
        _FlashColor("Hit flash", Color) = (1,1,1,1)
        _Flash("Hit flash amount", Range(0,1)) = 0
        _Dissolve("Dissolve", Range(0,1)) = 0
        _DissolveEdgeColor("Dissolve edge", Color) = (1.37,.8,.42,1)
        _DetailTint("World macro tint (A = strength)", Color) = (1,1,1,0)
        _DetailScale("World macro scale", Float) = .12
        [ToggleUI] _FogEnabled("Fog of war", Float) = 0
        _FogTex("Exploration", 2D) = "black" {}
        _RememberTerrain("Remember terrain", Float) = 0
        _ObjectVisibility("Whole object visibility", Float) = -1
        [HideInInspector] _FogBounds("Fog bounds", Vector) = (0,0,1,1)
        [HideInInspector] _FogSize("Fog size", Vector) = (1,1,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
        TEXTURE2D(_FogTex); SAMPLER(sampler_FogTex);
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST, _BaseColor, _EmissionColor, _GlowColor, _RimColor, _FlashColor, _DissolveEdgeColor, _DetailTint, _FogBounds, _FogSize;
        float _BumpScale, _Smoothness, _Metallic, _RimPower, _Flash, _Dissolve, _DetailScale, _FogEnabled, _RememberTerrain, _ObjectVisibility;
        CBUFFER_END
        struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float4 tangentOS:TANGENT; float2 uv:TEXCOORD0; half4 color:COLOR; };
        float Hash(float3 p) { p=frac(p*.3183099+.1)*17;return frac(p.x*p.y*p.z*(p.x+p.y+p.z)); }
        float Noise(float3 p)
        {
            float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);
            return lerp(lerp(lerp(Hash(i),Hash(i+float3(1,0,0)),f.x),lerp(Hash(i+float3(0,1,0)),Hash(i+float3(1,1,0)),f.x),f.y),
                        lerp(lerp(Hash(i+float3(0,0,1)),Hash(i+float3(1,0,1)),f.x),lerp(Hash(i+float3(0,1,1)),Hash(i+1),f.x),f.y),f.z);
        }
        // Identical in every pass, so what the fog hides casts no shadow and writes no depth.
        void FogMask(float3 positionWS, out half known, out half current)
        {
            known=1;current=1;
            if (_FogEnabled<.5) return;
            float2 uv=(positionWS.xz-_FogBounds.xy)/_FogBounds.zw;
            half inside=step(0,uv.x)*step(0,uv.y)*step(uv.x,1)*step(uv.y,1);
            half2 fog=SAMPLE_TEXTURE2D_LOD(_FogTex,sampler_FogTex,uv,0).rg;
            known=smoothstep(.02,.98,fog.r)*inside;current=fog.g*known;
            if (_RememberTerrain<.5)
            {
                if (_ObjectVisibility>=0) {clip(_ObjectVisibility-.5);known=1;current=1;}
                else clip(current-.5);
            }
        }
        // Death dissolve in object-relative space so the pattern travels with the actor. Returns the glowing rim.
        half Dissolve(float3 positionWS)
        {
            if (_Dissolve<=0) return 0;
            float3 p=positionWS-GetObjectToWorldMatrix()._m03_m13_m23;
            float n=saturate((Noise(p*3.1)*.7+Noise(p*7.3)*.3)*1.4-.2);
            clip(n-_Dissolve);return 1-smoothstep(0,.08,n-_Dissolve);
        }
        half3 SurfaceNormal(float2 uv, half3 normalWS, half4 tangentWS)
        {
            half3 normalTS=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,uv),_BumpScale);
            half3 bitangent=tangentWS.w*cross(normalWS,tangentWS.xyz);
            // A mesh without a tangent stream gets a default tangent whose sign lights bumps from the wrong side along V
            // on an xz-mapped quad (seen in a render), so normal-mapped meshes need tangents (Mesh.RecalculateTangents).
            return SafeNormalize(TransformTangentToWorld(normalTS,half3x3(tangentWS.xyz,bitangent,normalWS)));
        }
        ENDHLSL
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog
            // No sky reflections: the world is lit by ambient, the moon and real point lights only.
            #define _ENVIRONMENTREFLECTIONS_OFF
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            // The coordinate URP Lit interpolates: screen space with screen-space shadows, else the cascade's shadow map.
            float4 ShadowCoord(float3 positionWS)
            {
                VertexPositionInputs v=(VertexPositionInputs)0;v.positionWS=positionWS;v.positionCS=TransformWorldToHClip(positionWS);
                return GetShadowCoord(v);
            }
            struct Varyings
            {
                float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; half4 tangentWS:TEXCOORD2;
                float2 uv:TEXCOORD3; half4 fogAndVertexLight:TEXCOORD4; half glow:TEXCOORD5;
            };
            Varyings Vert(Attributes i)
            {
                Varyings o;VertexPositionInputs p=GetVertexPositionInputs(i.positionOS.xyz);VertexNormalInputs n=GetVertexNormalInputs(i.normalOS,i.tangentOS);
                o.positionCS=p.positionCS;o.positionWS=p.positionWS;o.normalWS=n.normalWS;o.tangentWS=half4(n.tangentWS,i.tangentOS.w*GetOddNegativeScale());
                o.uv=TRANSFORM_TEX(i.uv,_BaseMap);o.fogAndVertexLight=half4(ComputeFogFactor(p.positionCS.z),VertexLighting(p.positionWS,n.normalWS));o.glow=i.color.a;return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                half known,current;FogMask(i.positionWS,known,current);half edge=Dissolve(i.positionWS);
                half4 base=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);half3 albedo=base.rgb*_BaseColor.rgb;
                // World-space macro variation breaks up the 4 m texture repeat on floors and walls.
                float3 macro=float3(i.positionWS.x,0,i.positionWS.z)*_DetailScale;
                if (_DetailTint.a>0) albedo*=lerp(half3(1,1,1),_DetailTint.rgb,_DetailTint.a*(Noise(macro)*.65+Noise(macro*2.9)*.35));
                // Glow parts carry no UVs of their own, so their albedo comes from the glow colour
                // instead of whatever atlas texel UV (0,0) lands on. Materials without a glow colour
                // (primitives, meshes without a colour stream) are left untouched.
                half glowing=i.glow*step(.0001,max(_GlowColor.r,max(_GlowColor.g,_GlowColor.b)));
                albedo=lerp(albedo,_GlowColor.rgb*.3,glowing);
                InputData d=(InputData)0;
                d.positionWS=i.positionWS;d.normalWS=SurfaceNormal(i.uv,normalize(i.normalWS),i.tangentWS);d.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.positionWS);
                d.shadowCoord=ShadowCoord(i.positionWS);
                d.fogCoord=i.fogAndVertexLight.x;d.vertexLighting=i.fogAndVertexLight.yzw;d.shadowMask=1;
                d.bakedGI=max(half3(.15,.15,.15),SampleSH(d.normalWS));d.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);
                SurfaceData s=(SurfaceData)0;
                s.albedo=albedo;s.alpha=1;s.metallic=_Metallic;s.smoothness=base.a*_Smoothness;s.occlusion=1;s.normalTS=half3(0,0,1);
                half rim=pow(1-saturate(dot(d.normalWS,d.viewDirectionWS)),max(_RimPower,.1));
                s.emission=_EmissionColor.rgb+_GlowColor.rgb*i.glow+_RimColor.rgb*rim+_DissolveEdgeColor.rgb*edge;
                half3 lit=MixFog(lerp(UniversalFragmentPBR(d,s).rgb,_FlashColor.rgb,_Flash),d.fogCoord);
                half gray=dot(lit,half3(.2126,.7152,.0722));
                return half4(lerp(gray.xxx*.32,lit,smoothstep(.05,.95,current))*known,1);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0
            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            float3 _LightDirection, _LightPosition;
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; };
            Varyings Vert(Attributes i)
            {
                Varyings o;o.positionWS=TransformObjectToWorld(i.positionOS.xyz);float3 normalWS=TransformObjectToWorldNormal(i.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 light=normalize(_LightPosition-o.positionWS);
                #else
                float3 light=_LightDirection;
                #endif
                o.positionCS=ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(o.positionWS,normalWS,light)));return o;
            }
            half4 Frag(Varyings i):SV_Target { half known,current;FogMask(i.positionWS,known,current);Dissolve(i.positionWS);return 0; }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On ColorMask R
            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; };
            Varyings Vert(Attributes i) { Varyings o;o.positionWS=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);return o; }
            half Frag(Varyings i):SV_Target { half known,current;FogMask(i.positionWS,known,current);Dissolve(i.positionWS);return i.positionCS.z; }
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }
            ZWrite On
            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; half4 tangentWS:TEXCOORD2; float2 uv:TEXCOORD3; };
            Varyings Vert(Attributes i)
            {
                Varyings o;VertexNormalInputs n=GetVertexNormalInputs(i.normalOS,i.tangentOS);o.positionWS=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);
                o.normalWS=n.normalWS;o.tangentWS=half4(n.tangentWS,i.tangentOS.w*GetOddNegativeScale());o.uv=TRANSFORM_TEX(i.uv,_BaseMap);return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                half known,current;FogMask(i.positionWS,known,current);Dissolve(i.positionWS);
                float3 normalWS=SurfaceNormal(i.uv,normalize(i.normalWS),i.tangentWS);
                #if defined(_GBUFFER_NORMALS_OCT)
                return half4(PackFloat2To888(saturate(PackNormalOctQuadEncode(normalWS)*.5+.5)),0);
                #else
                return half4(normalWS,0);
                #endif
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
