Shader "Custom/UIBlur"
{
    Properties
    {
        _Size  ("Blur Radius", Range(0, 30)) = 8
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "RenderType"      = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType"     = "Plane"
        }

        Cull     Off
        Lighting Off
        ZWrite   Off
        Blend SrcAlpha OneMinusSrcAlpha

        // 현재 화면을 텍스처로 캡처 (Screen Space Overlay 포함)
        GrabPass { "_UIBlurGrab" }

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 vertex  : SV_POSITION;
                float4 color   : COLOR;
                float4 grabPos : TEXCOORD0;
            };

            sampler2D _UIBlurGrab;
            float4    _UIBlurGrab_TexelSize;
            float     _Size;
            fixed4    _Color;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex  = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.color   = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.grabPos.xy / i.grabPos.w;
                float2 ts = _UIBlurGrab_TexelSize.xy * _Size;

                // 3x3 Gaussian — _Size로 샘플 간격 조절 (클수록 더 흐림)
                fixed4 col = fixed4(0, 0, 0, 0);
                col += tex2D(_UIBlurGrab, uv + float2(-ts.x, -ts.y)) * 0.0625;
                col += tex2D(_UIBlurGrab, uv + float2(  0.0, -ts.y)) * 0.1250;
                col += tex2D(_UIBlurGrab, uv + float2( ts.x, -ts.y)) * 0.0625;
                col += tex2D(_UIBlurGrab, uv + float2(-ts.x,   0.0)) * 0.1250;
                col += tex2D(_UIBlurGrab, uv                        ) * 0.2500;
                col += tex2D(_UIBlurGrab, uv + float2( ts.x,   0.0)) * 0.1250;
                col += tex2D(_UIBlurGrab, uv + float2(-ts.x,  ts.y)) * 0.0625;
                col += tex2D(_UIBlurGrab, uv + float2(  0.0,  ts.y)) * 0.1250;
                col += tex2D(_UIBlurGrab, uv + float2( ts.x,  ts.y)) * 0.0625;

                // 알파는 Image 컴포넌트 color.a (DOTween DOFade 제어)
                col.a = i.color.a;
                return col;
            }
            ENDCG
        }
    }
}
