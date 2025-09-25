Shader "UI/TrapezoidSimple"
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

                // Normiertes Y: -0.5 = unten, +0.5 = oben
                float normY = pos.y + 0.5;

                // Interpolierte Breite zwischen unten und oben
                float trapezoid = lerp(_BottomWidth, _TopWidth, normY);

                // Normiertes X: -0.5 links, +0.5 rechts
                float normX = pos.x;

                // Verschiebung: symmetrisch zur Mitte
                pos.x += normX * trapezoid;

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
