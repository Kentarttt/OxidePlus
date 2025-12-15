Shader "Custom/SoftShadows"
{
    Properties
    {
        _MainTex ("Текстура", 2D) = "white" {}
        _ShadowSoftness ("Мягкость теней", Range(0,1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        CGPROGRAM
        #pragma surface surf SoftShadow fullforwardshadows

        sampler2D _MainTex;
        float _ShadowSoftness;

        // Функция для мягких теней
        fixed4 LightingSoftShadow(SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
            fixed diff = max(0, dot(s.Normal, lightDir));
            float softAtten = lerp(atten, 1.0, _ShadowSoftness);
            fixed4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * (diff * softAtten);
            c.a = s.Alpha;
            return c;
        }

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
        }
        ENDCG
    }
}