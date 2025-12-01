Shader "Davigo/Dither/Standard Tri Planar Normal"
{
	Properties
	{
		_TopColor("Top Color", Color) = (0.5,0.5,0.5,1)
		_SideColor("Side Color", Color) = (0.5,0.5,0.5,1)

		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		_MainTex("Top Texture", 2D) = "white" {}
		_NormalT("Top Normal", 2D) = "bump" {}
		_MainTexSide("Side/Bottom Texture", 2D) = "white" {}

		_Normal("Side/Bottom Normal", 2D) = "bump" {}

		_Scale("Top Scale", Range(-2,2)) = 1
		_SideScale("Side Scale", Range(-2,2)) = 1
		_TopSpread("TopSpread", Range(-2,2)) = 1
		_EdgeWidth("EdgeWidth", Range(0,0.5)) = 1

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
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex, _MainTexSide, _Normal, _NormalT;

		float4 _TopColor, _SideColor;
		float  _TopSpread, _EdgeWidth;
		float _Scale, _SideScale;

		half _Glossiness;
		half _Metallic;

		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
			float3 worldPos; // world position built-in value
			float3 worldNormal; INTERNAL_DATA // world normal built-in value
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			ditherClip(IN.screenPos.xy / IN.screenPos.w, _DitherThreshold, _DitherTexture, _DitherScale);

			// clamp (saturate) and increase(pow) the worldnormal value to use as a blend between the projected textures
			float3 worldNormalE = WorldNormalVector(IN, o.Normal);
			float3 blendNormal = saturate(pow(worldNormalE * 1.4,4));

			// triplanar for top texture for x, y, z sides
			float3 xm = tex2D(_MainTex, IN.worldPos.zy * _Scale);
			float3 zm = tex2D(_MainTex, IN.worldPos.xy * _Scale);
			float3 ym = tex2D(_MainTex, IN.worldPos.zx * _Scale);

			// lerped together all sides for top texture
			float3 toptexture = zm;
			toptexture = lerp(toptexture, xm, blendNormal.x);
			toptexture = lerp(toptexture, ym, blendNormal.y);

			// triplanar for top normal for x, y, z sides
			float3 xnnt = UnpackNormal(tex2D(_NormalT, IN.worldPos.zy * _Scale));
			float3 znnt = UnpackNormal(tex2D(_NormalT, IN.worldPos.xy * _Scale));
			float3 ynnt = UnpackNormal(tex2D(_NormalT, IN.worldPos.zx * _Scale));

			// lerped together all sides for top normal
			float3 toptextureNormal = znnt;
			toptextureNormal = lerp(toptextureNormal, xnnt, blendNormal.x);
			toptextureNormal = lerp(toptextureNormal, ynnt, blendNormal.y);

			// triplanar for side normal for x, y, z sides
			float3 xnn = UnpackNormal(tex2D(_Normal, IN.worldPos.zy * _Scale));
			float3 znn = UnpackNormal(tex2D(_Normal, IN.worldPos.xy * _Scale));
			float3 ynn = UnpackNormal(tex2D(_Normal, IN.worldPos.zx * _Scale));

			// lerped together all sides for side normal
			float3 sidetextureNormal = znn;
			sidetextureNormal = lerp(sidetextureNormal, xnn, blendNormal.x);
			sidetextureNormal = lerp(sidetextureNormal, ynn, blendNormal.y);

			// triplanar for side and bottom texture, x,y,z sides
			float3 x = tex2D(_MainTexSide, IN.worldPos.zy * _SideScale);
			float3 y = tex2D(_MainTexSide, IN.worldPos.zx * _SideScale);
			float3 z = tex2D(_MainTexSide, IN.worldPos.xy * _SideScale);

			// lerped together all sides for side bottom texture
			float3 sidetexture = z;
			sidetexture = lerp(sidetexture, x, blendNormal.x);
			sidetexture = lerp(sidetexture, y, blendNormal.y);

			float worldNormalDot = dot(o.Normal, worldNormalE.y);

			// if dot product is higher than the top spread slider, multiplied by triplanar mapped top texture
			// step is replacing an if statement to avoid branching :
			// if (worldNormalDotNoise > _TopSpread{ o.Albedo = toptexture}
			float3 topTextureResult = step(_TopSpread, worldNormalDot) * toptexture * _TopColor;
			float3 topNormalResult = step(_TopSpread, worldNormalDot) * toptextureNormal;

			// if dot product is lower than the top spread slider, multiplied by triplanar mapped side/bottom texture
			float3 sideTextureResult = step(worldNormalDot, _TopSpread) * sidetexture * _SideColor;
			float3 sideNormalResult = step(worldNormalDot, _TopSpread) * sidetextureNormal;

			// if dot product is in between the two, make the texture darker
			float3 topTextureEdgeResult = step(_TopSpread, worldNormalDot) * step(worldNormalDot, _TopSpread + _EdgeWidth) *  -0.15;

			o.Normal = topNormalResult + sideNormalResult;
			o.Albedo = topTextureResult + sideTextureResult + topTextureEdgeResult;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
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