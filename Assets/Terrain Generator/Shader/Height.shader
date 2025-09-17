Shader "Custom/Height"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BandCount ("Band Count", Int) = 4
        _HeightScale ("Height Scale", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting   
        #pragma target 3.0

        sampler2D _MainTex;
        float _HeightScale;
        float _MaxHeights[16];  // Support up to 16 height bands
        float4 _MinHeightColors[16];  // Support up to 16 color bands
        float4 _MaxHeightColors[16];  // Support up to 16 color bands
        int _BandCount;

        struct Input
        {
            float2 uv_MainTex;
        };


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v)
        {
            // Sample height using the mesh UVs
            float height = tex2Dlod(_MainTex, float4(v.texcoord.xy, 0, 0)).r;
            // Displace along normal
            v.vertex.xyz += v.normal * (height * _HeightScale);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Sample the height from the texture (assuming it's stored in the red channel)
            float height = tex2D(_MainTex, IN.uv_MainTex).r;
            
            // Initialize color to black
            float3 finalColor = float3(0, 0, 0);
            
            // Find which height band this pixel belongs to
            for (int i = 0; i < _BandCount && i < 16; i++)
            {
                float maxHeight = _MaxHeights[i];
                
                if (height <= maxHeight)
                {
                    // Interpolate between min and max colors for this band
                    float3 minColor = _MinHeightColors[i].rgb;
                    float3 maxColor = _MaxHeightColors[i].rgb;
                    
                    // Calculate interpolation factor based on height within the band
                    float prevMaxHeight = (i > 0) ? _MaxHeights[i-1] : 0.0;
                    float bandHeight = maxHeight - prevMaxHeight;
                    float t = (bandHeight > 0) ? (height - prevMaxHeight) / bandHeight : 0.0;
                    t = saturate(t);
                    
                    finalColor = lerp(minColor, maxColor, t);
                    break;
                }
            }
            
            o.Albedo = finalColor;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
