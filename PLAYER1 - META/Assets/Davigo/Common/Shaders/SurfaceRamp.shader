Shader "Roystan/Surface/Ramp"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset]
		_Ramp("Ramp", 2D) = "white" {}
		[HDR]
		_EmissionColor("Emission Color", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags 
		{ 
			"RenderType"="Opaque" 
		}

        CGPROGRAM
        #pragma surface surf ToonRamp fullforwardshadows
        #pragma target 3.0

		sampler2D _Ramp;

		float4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten)
		{
		#ifndef USING_DIRECTIONAL_LIGHT
			lightDir = normalize(lightDir);
		#endif

			float d = 1 - (dot(s.Normal, lightDir) * 0.5 + 0.5);
			float3 ramp = tex2D(_Ramp, float2(d, 0.5)).rgb;

			float4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * ramp * atten + s.Emission;
			c.a = 0;
			return c;
		}

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        float4 _Color;
		float4 _EmissionColor;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
			float4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
			o.Emission = _EmissionColor;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
