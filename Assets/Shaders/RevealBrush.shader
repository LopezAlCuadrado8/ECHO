Shader "Echo/RevealBrush"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Softness ("Softness", Range(0, 1)) = 0.35
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off ZTest Always
        Blend One One   // ADITIVO: acumula blobs sin sobrescribir

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };

            fixed4 _Color;
            float _Softness;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 d = i.uv - 0.5;
                float dist = length(d) * 2.0; // 0 en centro, 1 en borde
                float mask = 1.0 - smoothstep(1.0 - _Softness, 1.0, dist);
                return _Color * mask;
            }
            ENDCG
        }
    }
}