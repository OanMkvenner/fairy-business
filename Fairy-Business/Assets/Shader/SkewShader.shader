Shader "UI/TrapezoidCanvasAuto"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _TopWidth ("Top Width", Range(-1,1)) = 0
        _BottomWidth ("Bottom Width", Range(-1,1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _MainTex_ST;

            float _TopWidth;
            float _BottomWidth;

            v2f vert(appdata_t v)
            {
                v2f o;
                float4 pos = v.vertex;

                // --- Canvas-relative Y ---
                // pos = lokale UI-Vertex-Position
                // unity_ObjectToWorld wandelt in Weltkoordinaten
                float3 worldPos = mul(unity_ObjectToWorld, pos).xyz;

                // Canvas-Center in Weltkoordinaten
                float canvasHalfHeight = _ScreenParams.y * 0.5; // Annäherung an Canvas-Höhe in Pixels
                float canvasCenterY = 0; // Canvas-Mittelpunkt in Weltkoordinaten Y (0 für Screen Space Overlay)

                float normY = (worldPos.y - canvasCenterY + canvasHalfHeight) / (_ScreenParams.y); // 0..1

                // Trapezbreite interpolieren
                float trapezoid = lerp(_BottomWidth, _TopWidth, normY);

                // X-Verzerrung
                pos.x += pos.x * trapezoid;

                o.vertex = UnityObjectToClipPos(pos);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                return col;
            }
            ENDCG
        }
    }
}
