Shader "Custom/BlurShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 10)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        // --- HORIZONTAL BLUR PASS ---
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_horizontal

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
            float _BlurSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Gaussian Kernel (5x5)
            static float weights[5] = { 0.06136, 0.24477, 0.38774, 0.24477, 0.06136 };
            static float offsets[5] = { -2.0, -1.0, 0.0, 1.0, 2.0 };

            fixed4 frag_horizontal (v2f i) : SV_Target
            {
                float4 color = float4(0, 0, 0, 0);

                for (int j = 0; j < 5; j++)
                {
                    float offset = offsets[j] * _BlurSize * 0.001; // Skalierung für UI
                    color += tex2D(_MainTex, i.uv + float2(offset, 0)) * weights[j];
                }

                return color;
            }
            ENDCG
        }

        // --- VERTICAL BLUR PASS ---
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_vertical

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
            float _BlurSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Gleiche Gewichte wie im horizontalen Pass
            static float weights[5] = { 0.06136, 0.24477, 0.38774, 0.24477, 0.06136 };
            static float offsets[5] = { -2.0, -1.0, 0.0, 1.0, 2.0 };

            fixed4 frag_vertical (v2f i) : SV_Target
            {
                float4 color = float4(0, 0, 0, 0);

                for (int j = 0; j < 5; j++)
                {
                    float offset = offsets[j] * _BlurSize * 0.001; // Skalierung für UI
                    color += tex2D(_MainTex, i.uv + float2(0, offset)) * weights[j];
                }

                return color;
            }
            ENDCG
        }
    }
}