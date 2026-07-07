Shader "UI/BuffShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlendTex ("Buff Time Texture", 2D) = "white"{}
        
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BlendColor ("Blend Color", Color) = (0,0,0,0)
        
        _VisiblePercentage ("Visible Percentage",  Range(0,1)) = 0.5
        _BlendAlpha ("叠加色占比", Range(0,1)) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue"="Geometry"
            "IgnoreProjector"="True"
            "RenderType"="Geometry"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Pass
        {
            Name "BuffDefault"
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv0 : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float visible : TEXCOORD2;
                
                float4 baseColor : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            sampler2D _BlendTex;
            float4 _BlendTex_ST;
            
            float _BlendAlpha;
            float _VisiblePercentage;
            
            fixed4 _BaseColor;
            fixed4 _BlendColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv0 = TRANSFORM_TEX(v.uv0,_MainTex);
                o.uv1 = TRANSFORM_TEX(v.uv0,_BlendTex);
                o.visible = v.uv0.y;
                
                o.baseColor = v.color * _BaseColor;

                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 colBase = tex2D(_MainTex,i.uv0) * i.baseColor;
                fixed4 colBlend = tex2D(_BlendTex,i.uv1) * _BlendColor;
                
                if (1 - i.visible > _VisiblePercentage)
                {
                    return colBase;
                }
                else
                {
                    return lerp(colBase, colBlend,_BlendAlpha);
                }
            }

            ENDCG
        }
    }
}