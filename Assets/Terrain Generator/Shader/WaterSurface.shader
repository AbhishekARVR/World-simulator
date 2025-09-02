Shader "Custom/WaterSurface"
{
    Properties
    {
        _WaterColor ("Water Color", Color) = (0,0.5,1,0.4)
        _Glossiness ("Smoothness", Range(0,1)) = 0.7
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Standard alpha:fade

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _WaterColor;
        half _Glossiness;
        half _Metallic;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _WaterColor.rgb;
            o.Alpha = _WaterColor.a;
            o.Smoothness = _Glossiness;
            o.Metallic = _Metallic;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
