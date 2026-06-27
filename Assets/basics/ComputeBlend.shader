Shader "Hidden/ComputeBlend"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _EffectTex ("EffectTex", 2D) = "black" {}
        _Blend ("Blend", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZTest Always Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _EffectTex;
            float _Blend;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 src = tex2D(_MainTex, i.uv);
                fixed4 fx = tex2D(_EffectTex, i.uv);
                return lerp(src, fx, saturate(_Blend));
            }
            ENDCG
        }
    }
}
