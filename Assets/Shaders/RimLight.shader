Shader "Custom/RimLight"
{
    Properties
    {
        _MainTex ("Основная текстура", 2D) = "white" {}
        _RimColor ("Цвет свечения", Color) = (0.5,0.8,1,1)
        _RimPower ("Сила свечения", Range(0.1,10)) = 3
        _RimIntensity ("Яркость свечения", Range(0,5)) = 1
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        CGPROGRAM
        #pragma surface surf Standard

        sampler2D _MainTex;
        fixed4 _RimColor;
        float _RimPower;
        float _RimIntensity;

        struct Input
        {
            float2 uv_MainTex;  // Для текстуры
            float3 viewDir;     // Для Rim эффекта
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Загружаем основную текстуру
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            
            // Основной цвет берем из текстуры
            o.Albedo = tex.rgb;
            
            // Создаем свечение по краям
            float rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            rim = pow(rim, _RimPower);
            
            // Добавляем свечение поверх текстуры
            o.Emission = _RimColor.rgb * rim * _RimIntensity;
            
            o.Metallic = 0.1;
            o.Smoothness = 0.5;
        }
        ENDCG
    }
    FallBack "Diffuse"
}