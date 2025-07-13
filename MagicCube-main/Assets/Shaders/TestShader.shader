Shader "Unlit/TestShader"
{
     Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tex ("Texture", 2D) = "white" {}
        _Speed("Speed",float) = 0.5
        _Level("Level",Range(0,1)) = 0.5
    }
    SubShader
    {
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
            };
 
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 clipPos : POSITION1;
            };
 
            sampler2D _MainTex;
            sampler2D _Tex;
            float4 _MainTex_ST;
            float _Speed;
            float _Level;
 
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.clipPos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
 
                return o;
            }
 
            float4 frag (v2f i) : SV_Target
            {
                float zDepth = i.clipPos.z / i.clipPos.w;
                //zDepth = (zDepth + 1.f) * .5f;
                return float4(zDepth, zDepth, zDepth, 1.f);
            }
            ENDCG
        }
    }
}
