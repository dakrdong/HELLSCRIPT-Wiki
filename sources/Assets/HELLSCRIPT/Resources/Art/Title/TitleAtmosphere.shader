Shader "HELLSCRIPT/Title Atmosphere"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sanctuary", 2D) = "black" {}
        _SceneTime ("Unscaled scene clock", Float) = 0
        _Crop ("Cover crop", Vector) = (0,0,1,1)
        _EntryFade ("Entry fade", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            sampler2D _MainTex;float4 _Crop;float _SceneTime,_EntryFade;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float noise(float2 p)
            {
                float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);
            }
            float mist(float2 p){return noise(p)*.57+noise(p*2.13+7)*.28+noise(p*4.17+19)*.15;}
            fixed4 frag(v2f i):SV_Target
            {
                float t=_SceneTime;
                float2 uv=_Crop.xy+i.uv*_Crop.zw;
                // Tiny camera drift has overscan, so it never reveals an empty border.
                uv=(uv-.5)*.985+.5+float2(sin(t*.075)*.002,cos(t*.061)*.0015);
                fixed3 col=tex2D(_MainTex,uv).rgb;
                float farMist=mist(uv*float2(5,12)+float2(t*.021,-t*.007));
                float nearMist=mist(uv*float2(8,15)+float2(-t*.034,t*.009));
                float valley=exp(-pow((uv.y-.27)*5.6,2));
                float veil=saturate((farMist-.24)*.26+(nearMist-.4)*.28)*valley;
                col=lerp(col,float3(.39,.44,.48),veil);
                float clouds=mist(uv*float2(7,9)+float2(t*.012,0));
                col*=1-smoothstep(.62,.9,uv.y)*clouds*.17;
                float gate=exp(-dot((uv-float2(.503,.49))*float2(20,9),(uv-float2(.503,.49))*float2(20,9)));
                float breath=.5+.28*sin(t*.83)+.13*sin(t*1.73);
                col+=float3(.25,.053,.012)*gate*breath;
                // Sparse rising ember points. No per-frame particle GameObjects or allocations.
                float2 p=float2(uv.x*88,uv.y*52-t*.22),cell=floor(p),f=frac(p);
                float seed=hash(cell);
                float ember=smoothstep(.956,.996,seed)*(1-smoothstep(.018,.08,length((f-.5)*float2(1,1.8))));
                col+=float3(1,.29,.045)*ember*(.5+.5*sin(t*1.3+seed*20))*smoothstep(.05,.2,uv.y)*(1-smoothstep(.55,.8,uv.y));
                float vignette=1-.34*pow(saturate(length((i.uv-.5)*float2(1.2,1.05))),1.6);
                col*=vignette;
                col*=lerp(.47,1,smoothstep(0,.45,i.uv.y));
                col*=1-_EntryFade;
                return fixed4(col,1)*i.color;
            }
            ENDCG
        }
    }
}
