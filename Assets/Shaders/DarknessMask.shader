Shader "Echo/DarknessMask"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _RevealTex ("Reveal Mask", 2D) = "black" {}
        _TemporalTex ("Temporal Mask", 2D) = "black" {}
        _DarknessColor ("Darkness Color", Color) = (0,0,0,1)
        _EdgeSoftness ("Edge Softness", Range(0.001, 0.5)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _RevealTex;
            sampler2D _TemporalTex;
            fixed4 _DarknessColor;
            float _EdgeSoftness;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // i.vertex.xy contiene la posición en píxeles de pantalla
                // (0..width, 0..height). La normalizamos a 0..1.
                float2 screenUV = i.vertex.xy / _ScreenParams.xy;

                // En D3D, la Y está invertida respecto a OpenGL.
                // Unity lo maneja con UNITY_UV_STARTS_AT_TOP
                #if UNITY_UV_STARTS_AT_TOP
                    screenUV.y = 1.0 - screenUV.y;
                #endif

                float perm = tex2D(_RevealTex, screenUV).r;
                float temp = tex2D(_TemporalTex, screenUV).r;
                float reveal = saturate(perm + temp);

                reveal = smoothstep(0.5 - _EdgeSoftness, 0.5 + _EdgeSoftness, reveal);

                float alpha = (1.0 - reveal) * _DarknessColor.a;
                return fixed4(_DarknessColor.rgb, alpha);
            }
            ENDCG
        }
    }
}