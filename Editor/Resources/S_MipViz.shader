Shader "Hidden/S_MipViz"
{
    Properties
    {
        // _MipTex ("MipTexture", 2D) = "grey" {}
        // _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 mipuv : TEXCOORD1;
                // UNITY_FOG_COORDS(2)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _MipTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.mipuv = o.uv * _MainTex_TexelSize.zw / 8.0;
                // UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                half4 col = tex2D(_MainTex, i.uv);
                half4 mip = tex2D(_MipTex, i.mipuv);
                half4 res = 0;
                res.rgb = lerp(col.rgb, mip.rgb, mip.a);
                res.a = col.a;
                // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, res);
                return res;
            }
            ENDCG
        }
    }
}
