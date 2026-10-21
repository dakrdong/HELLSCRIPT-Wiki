Shader "HELLSCRIPT/Title Vista"
{
    Properties
    {
        [PerRendererData] _MainTex ("Painting", 2D) = "black" {}
        _SceneTime ("Unscaled scene clock", Float) = 0
        _Core ("Rift core UV (xy), halo radius in UV height (z), texture aspect (w)", Vector) = (.5,.5,.08,1.78)
        _Glow ("Rift light", Float) = 1
        _Rush ("Radial smear toward the core", Range(0,.3)) = 0
        _Defocus ("Foreground defocus in screen pixels", Float) = 0
        _Fade ("Bottom fade from (x) to (y) in UV height; off when y is 0", Vector) = (0,0,0,0)
        _Shade ("Logo shade centre (xy) and radii (zw) in screen UV", Vector) = (.5,.8,.4,.1)
        _ShadeStrength ("Logo shade strength", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
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
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;float4 screen:TEXCOORD1;};
            sampler2D _MainTex;float4 _MainTex_TexelSize,_Core,_Fade,_Shade;float _SceneTime,_Glow,_Rush,_ShadeStrength,_Defocus;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;o.screen=ComputeScreenPos(o.vertex);return o;}
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            fixed4 frag(v2f i):SV_Target
            {
                float t=_SceneTime;float2 d=i.uv-_Core.xy;
                fixed4 col=tex2D(_MainTex,i.uv);
                // The login flight smears the painting toward the rift; the extra taps only run during the flight.
                if(_Rush>.001)
                {
                    fixed4 sum=col;
                    [unroll] for(int k=1;k<8;k++)sum+=tex2D(_MainTex,i.uv-d*_Rush*k/7);
                    col=sum/8;
                }
                // Alpha-weighted taps keep transparent cutout edges free of dark fringes. Each tap reads the mip level that
                // covers its spacing, so a wide defocus stays smooth instead of showing the 5x5 grid as ghost copies.
                if(_Defocus>.001)
                {
                    float2 dx=ddx(i.uv)*_Defocus*.5,dy=ddy(i.uv)*_Defocus*.5;
                    float lod=log2(max(1,max(length(dx),length(dy))*_MainTex_TexelSize.z));
                    float4 sum=0;
                    [unroll] for(int y=-2;y<=2;y++)
                    [unroll] for(int x=-2;x<=2;x++)
                    {
                        float4 sample=tex2Dlod(_MainTex,float4(i.uv+dx*x+dy*y,0,lod));
                        float weight=(x==0?6:(abs(x)==1?4:1))*(y==0?6:(abs(y)==1?4:1));
                        sum+=float4(sample.rgb*sample.a,sample.a)*weight;
                    }
                    col=float4(sum.rgb/max(sum.a,.00001),sum.a/256);
                }
                // The cliff layers share the shader without rift light.
                if(_Glow>0)
                {
                    float2 q=d*float2(_Core.w,1)/_Core.z;float r2=dot(q,q);
                    float breath=.78+.16*sin(t*1.9)+.06*sin(t*5.3);
                    col.rgb+=float3(.62,.16,.86)*exp(-r2)*breath*_Glow*.42+float3(1,.86,1)*exp(-r2*9)*_Glow*.3;
                    // Sparks drift up out of the crack: hashed cells, no particle objects or allocations.
                    float2 p=float2(q.x*3.2,q.y*2.2-t*.55),f=frac(p);float seed=hash(floor(p));
                    float spark=step(.9,seed)*(1-smoothstep(.04,.13,length(f-float2(.5,.5+.25*sin(t*1.3+seed*30)))));
                    col.rgb+=float3(1,.55,1)*spark*saturate(_Glow)*(1-smoothstep(.5,4.5,r2))*(.55+.45*sin(t*3+seed*40));
                }
                // The bright sky dims softly behind the cream logo so its small lines stay readable.
                if(_ShadeStrength>0){float2 s=(i.screen.xy/i.screen.w-_Shade.xy)/_Shade.zw;col.rgb*=1-_ShadeStrength*exp(-dot(s,s));}
                // A cliff lifted above the portrait buttons dissolves into the valley mist instead of ending in a cut.
                if(_Fade.y>0)col.a*=smoothstep(_Fade.x,_Fade.y,i.uv.y);
                return col*i.color;
            }
            ENDCG
        }
    }
}
