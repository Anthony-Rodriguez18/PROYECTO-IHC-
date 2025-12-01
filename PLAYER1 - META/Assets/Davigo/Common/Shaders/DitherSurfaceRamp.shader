Shader "Roystan/Surface/Ramp Dither"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset]
		_Ramp("Ramp", 2D) = "white" {}
		[HDR]
		_EmissionColor("Emission Color", Color) = (0,0,0,0)
		_Occlusion("Occlusion", 2D) = "white" {}
		_OcclusionStrength("Occlusion Strength", Range(0,1)) = 1

		[Header(Dither)]
		_DitherTexture("Dither Texture", 2D) = "white" {}
		_DitherThreshold("Dither Threshold", Range(0, 1)) = 1
		_DitherScale("Dither Scale", Float) = 1
	}

	CGINCLUDE
	#include "Assets/Davigo/Common/Shaders/Dither Functions.cginc"

	sampler2D _DitherTexture;
	float _DitherThreshold;
	float _DitherScale;
	ENDCG

	SubShader
	{
		Tags
		{
			"Queue" = "Geometry"
		}

		CGPROGRAM
		#pragma surface surf ToonRamp fullforwardshadows
		#pragma target 3.0

		sampler2D _Ramp;
		half _OcclusionStrength;

		struct SurfaceOutputToon
		{
			fixed3 Albedo;  // diffuse color
			fixed3 Normal;  // tangent space normal, if written
			fixed3 Emission;
			fixed Alpha;    // alpha for transparencies
			half Occlusion;
		};

		float4 LightingToonRamp(SurfaceOutputToon s, UnityGI gi)
		{
			half3 lightDir = gi.light.dir;

		#ifndef USING_DIRECTIONAL_LIGHT
			lightDir = normalize(lightDir);
		#endif

			float d = 1 - (dot(s.Normal, lightDir) * 0.5 + 0.5);
			float3 ramp = tex2D(_Ramp, float2(d, 0.5)).rgb;

			float occlusion = lerp(s.Occlusion, 1, 1 - _OcclusionStrength);

			float4 c;
			c.rgb = (s.Albedo * gi.light.color.rgb * ramp + gi.indirect.diffuse * s.Albedo) * occlusion + s.Emission;
			c.a = 0;
			return c;
		}

		void LightingToonRamp_GI(SurfaceOutputToon s, UnityGIInput data, inout UnityGI gi)
		{
			gi = UnityGlobalIllumination(data, 1.0, s.Normal);
		}

		sampler2D _MainTex;
		sampler2D _Occlusion;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_Occlusion;
			float4 screenPos;
		};

		float4 _Color;
		float4 _EmissionColor;


		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputToon o)
		{
			ditherClip(IN.screenPos.xy / IN.screenPos.w, _DitherThreshold, _DitherTexture, _DitherScale);

			float4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			half occlusion = tex2D(_Occlusion, IN.uv_Occlusion).r;

			o.Occlusion = occlusion;
			o.Emission = _EmissionColor;
		}
		ENDCG

		Pass
		{
			Name "ShadowCaster"
			Tags
			{
				"LightMode" = "ShadowCaster"
			}

			Fog
			{
				Mode Off
			}

			ZWrite On ZTest LEqual Cull Back
			Offset 1, 1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"
			#include "Assets/Davigo/Common/Shaders/Dither Functions.cginc"

			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 screenPos : TEXCOORD5;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER(o)
				o.screenPos = ComputeScreenPos(o.pos);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				ditherClip(i.screenPos.xy / i.screenPos.w, _DitherThreshold, _DitherTexture, _DitherScale);
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}
