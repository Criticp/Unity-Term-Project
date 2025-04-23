Shader "Hidden/DepthMaskWithOverlay"
{
    Properties
    {
        _OverlayColor("Overlay Color", Color) = (1,1,1,0.5)
        _ShowOverlay("Show Overlay", Float) = 0
    }
        SubShader
    {
        Tags { "Queue" = "Geometry-1" "RenderType" = "Opaque" }
        Cull Off

        Pass
        {
            Name       "DepthOnly"
            ColorMask  0       
            ZWrite     On      
            ZTest      LEqual
        }

        Pass
        {
            Name       "Overlay"
            ColorMask  RGBA
            ZWrite     Off     
            ZTest      LEqual
            Blend      SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _OverlayColor;
            float  _ShowOverlay;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                clip(_ShowOverlay - 0.5);

            return _OverlayColor;
        }
        ENDCG
    }
    }
}
