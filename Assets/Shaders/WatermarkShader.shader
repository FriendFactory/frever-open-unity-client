Shader "Frever/WatermarkShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _OffsetX ("X Offset", Range(0, 1)) = 0.75
        _OffsetY ("Y Offset", Range(0, 1)) = 0.75
        _Scale ("Scale", Range(0, 1)) = 0.25
        _TextureAspect ("Texture Aspect Ratio", Float) = 1.0
        _ScreenAspect ("Screen Aspect Ratio", Float) = 1.0
        _Opacity ("Opacity", Range(0, 1)) = 1.0 
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _OffsetX;
            float _OffsetY;
            float _Scale;
            float _TextureAspect;
            float _ScreenAspect;
            float _Opacity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Start with the basic UV transformation
                float2 scaledUV = (i.uv - float2(1 - (1.0 - _OffsetX), 1 - (1.0 - _OffsetY))) / _Scale;
                
                // Calculate the aspect ratio correction
                float aspectCorrection = _TextureAspect / _ScreenAspect;
                
                // Apply the aspect correction based on which is greater
                if (aspectCorrection > 1.0) {
                    // Texture is wider than screen aspect
                    scaledUV.x /= aspectCorrection;
                } else {
                    // Texture is taller than screen aspect
                    scaledUV.y *= aspectCorrection;
                }

                // Check if the UV coordinates are outside [0,1] bounds
                if (scaledUV.x < 0.0 || scaledUV.x > 1.0 || scaledUV.y < 0.0 || scaledUV.y > 1.0)
                {
                    discard; // Discard any out-of-bounds UVs to avoid artifacts
                }

                // Sample the texture with adjusted UVs
                half4 color = tex2D(_MainTex, scaledUV) * _Color;
                color.a *= _Opacity;
                // Discard near-transparent pixels
                if (color.a < 0.01) discard;

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
