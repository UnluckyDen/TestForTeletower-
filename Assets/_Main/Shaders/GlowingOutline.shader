Shader "Custom/GlowingOutline"
{
    Properties
    {
        [HDR] _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.2)) = 0.015
        _OutlineSoftness ("Edge Softness", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+20" }
        
        Pass
        {
            Name "OUTLINE"
            Cull Front
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            float _OutlineWidth;
            float _OutlineSoftness;
            fixed4 _OutlineColor;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float3 smoothNormal : TEXCOORD1; // Для сглаженных нормалей
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // Смещение с учетом сглаженных нормалей
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                worldPos += worldNormal * _OutlineWidth;
                
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                o.worldNormal = worldNormal;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Эффект мягких краев через градиент прозрачности
                float outlineFactor = saturate(_OutlineSoftness * 2);
                
                fixed4 col = _OutlineColor;
                col.a = outlineFactor * _OutlineColor.a;
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}