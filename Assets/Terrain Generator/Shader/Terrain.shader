Shader "Custom/HeightBandsGradientWater"
{
    Properties {
        _WaterColor ("Water Color (Lowest)", Color) = (0,0.5,1,0.5)
        _Color1 ("Color 1", Color) = (0,0.8,0,1)
        _Color2 ("Color 2", Color) = (0.7,0.6,0.2,1)
        _Color3 ("Color 3 (Highest)", Color) = (1,1,1,1)
        
        _Height0 ("Water Height", Float) = 2
        _Height1 ("Band 1 Height", Float) = 7
        _Height2 ("Band 2 Height", Float) = 14
        _Height3 ("Band 3 Height", Float) = 20
        
        _Gradient0 ("Water Gradient Height", Float) = 1
        _Gradient1 ("Band 1 Gradient Height", Float) = 2
        _Gradient2 ("Band 2 Gradient Height", Float) = 2
        _Gradient3 ("Band 3 Gradient Height", Float) = 1

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Standard alpha:fade

        struct Input {
            float3 worldPos;
        };

        fixed4 _WaterColor, _Color1, _Color2, _Color3;
        float _Height0, _Height1, _Height2, _Height3;
        float _Gradient0, _Gradient1, _Gradient2, _Gradient3;
        half _Glossiness, _Metallic;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            float h = IN.worldPos.y;
            float alpha = 1;
            fixed3 result;

            // Water band (lowest)
            if(h < _Height0 - _Gradient0) {
                result = _WaterColor.rgb;
                alpha = _WaterColor.a;
            }
            // Water->Band 1 gradient
            else if(h < _Height0 + _Gradient0) {
                float t = saturate((h - (_Height0 - _Gradient0)) / (_Gradient0*2));
                result = lerp(_WaterColor.rgb, _Color1.rgb, t);
                alpha = lerp(_WaterColor.a, 1, t);
            }
            // Band 1
            else if(h < _Height1 - _Gradient1) {
                result = _Color1.rgb;
                alpha = 1;
            }
            // Band 1->2 gradient
            else if(h < _Height1 + _Gradient1) {
                float t = saturate((h - (_Height1 - _Gradient1)) / (_Gradient1*2));
                result = lerp(_Color1.rgb, _Color2.rgb, t);
                alpha = 1;
            }
            // Band 2
            else if(h < _Height2 - _Gradient2) {
                result = _Color2.rgb;
                alpha = 1;
            }
            // Band 2->3 gradient
            else if(h < _Height2 + _Gradient2) {
                float t = saturate((h - (_Height2 - _Gradient2)) / (_Gradient2*2));
                result = lerp(_Color2.rgb, _Color3.rgb, t);
                alpha = 1;
            }
            // Band 3 and above
            else {
                result = _Color3.rgb;
                alpha = 1;
            }

            o.Albedo = result;
            o.Smoothness = _Glossiness;
            o.Metallic = _Metallic;
            o.Alpha = alpha;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
