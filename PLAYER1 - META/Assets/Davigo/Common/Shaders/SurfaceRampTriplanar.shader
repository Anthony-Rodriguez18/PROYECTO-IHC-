Shader "Roystan/Surface/Triplanar Ramp"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		[KeywordEnum(Local, World)]
		_TriplanarSpace("Triplanar Space", Float) = 0
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		[Normal]
		_Normal("Normal", 2D) = "bump" {}
		_Sharpness("Sharpness", Range(1, 64)) = 1
		[NoScaleOffset]
		_Ramp("Ramp", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
		}

		CGPROGRAM
		#pragma shader_feature _TRIPLANARSPACE_LOCAL _TRIPLANARSPACE_WORLD
		#pragma surface surf ToonRamp fullforwardshadows vertex:vert
		#pragma target 3.0

		sampler2D _Ramp;

		half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten)
		{
		#ifndef USING_DIRECTIONAL_LIGHT
			lightDir = normalize(lightDir);
		#endif

			half d = 1 - (dot(s.Normal, lightDir) * 0.5 + 0.5);
			half3 ramp = tex2D(_Ramp, float2(d, 0.5)).rgb;

			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * ramp * atten;
			c.a = 0;
			return c;
		}

		sampler2D _MainTex;
		float4 _MainTex_ST;

		sampler2D _Normal;
		float4 _Normal_ST;

		struct Input
		{
			float3 pos;
			float3 normal;
		};

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);

		#if _TRIPLANARSPACE_LOCAL
			o.pos = v.vertex.xyz;
			o.normal = normalize(v.normal);
		#elif _TRIPLANARSPACE_WORLD
			o.pos = mul(unity_ObjectToWorld, v.vertex);
			o.normal = mul((float3x3)unity_ObjectToWorld, v.normal);
		#endif
		}

		fixed4 _Color;
		float _Sharpness;

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutput o)
		{
			float2 topUV = TRANSFORM_TEX(IN.pos.xz, _MainTex);
			float2 sideUV = TRANSFORM_TEX(IN.pos.zy, _MainTex);
			float2 frontUV = TRANSFORM_TEX(IN.pos.xy, _MainTex);

			float2 topUVNormal = TRANSFORM_TEX(IN.pos.xz, _Normal);
			float2 sideUVNormal = TRANSFORM_TEX(IN.pos.zy, _Normal);
			float2 frontUVNormal = TRANSFORM_TEX(IN.pos.xy, _Normal);

			float3 weights = abs(IN.normal);
			weights = pow(weights, _Sharpness);
			weights = weights / (weights.x + weights.y + weights.z);

			float4 topColor = tex2D(_MainTex, topUV) * weights.y;
			float4 sideColor = tex2D(_MainTex, sideUV) * weights.x;
			float4 frontColor = tex2D(_MainTex, frontUV) * weights.z;

			float4 col = (topColor + sideColor + frontColor) * _Color;

			float3 topNormal = UnpackNormal(tex2D(_Normal, topUVNormal)) * weights.y;
			float3 sideNormal = UnpackNormal(tex2D(_Normal, sideUVNormal)) * weights.x;
			float3 frontNormal = UnpackNormal(tex2D(_Normal, frontUVNormal)) * weights.z;

			float3 normal = normalize(topNormal + sideNormal + frontNormal);

			o.Albedo = col.rgb;
			o.Normal = normal;
		}
		ENDCG
	}

	FallBack "Diffuse"
}
