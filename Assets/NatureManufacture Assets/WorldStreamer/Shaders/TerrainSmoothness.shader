Shader "NatureManufacture Shaders/Terrain/StandardSmoothness" {

	Properties{
		// used in fallback on old cards & base map
		[HideInInspector] _MainTex("BaseMap (RGB)", 2D) = "white" {}
		[HideInInspector] _Color("Main Color", Color) = (1,1,1,1)
		[HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
	}

		SubShader{
			Tags {
				"Queue" = "Geometry-100"
				"RenderType" = "Opaque"
			}

			CGPROGRAM
			#pragma surface surf NoLighting vertex:SplatmapVert //finalcolor:SplatmapFinalColor finalgbuffer:SplatmapFinalGBuffer addshadow fullforwardshadows
			#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
			#pragma multi_compile_fog // needed because finalcolor oppresses fog code generation.
			#pragma target 3.0
			#include "UnityPBSLighting.cginc"

			#pragma multi_compile_local __ _ALPHATEST_ON
			#pragma multi_compile_local __ _NORMALMAP

			#define TERRAIN_STANDARD_SHADER
			#define TERRAIN_INSTANCED_PERPIXEL_NORMAL
			#define TERRAIN_SURFACE_OUTPUT SurfaceOutput
			#include "TerrainSplatmapCommon.cginc"

			half _Metallic0;
			half _Metallic1;
			half _Metallic2;
			half _Metallic3;

			half _Smoothness0;
			half _Smoothness1;
			half _Smoothness2;
			half _Smoothness3;

			float PositivePow(float base, float power)
			{
				return pow(max(abs(base), float(1.192092896e-07)), power);
			}


			float SRGBToLinear(float c)
			{
				float linearRGBLo = c / 12.92;
				float linearRGBHi = PositivePow((c + 0.055) / 1.055, 2.4);
				float linearRGB = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
				return linearRGB;
			}

			void surf(Input IN, inout SurfaceOutput o) {
				half4 splat_control;
				half weight;
				fixed4 mixedDiffuse;
				half4 defaultSmoothness = half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);

				float3 normal = o.Normal;
				SplatmapMix(IN, defaultSmoothness, splat_control, weight, mixedDiffuse, normal);

				o.Alpha = weight;
				o.Emission = mixedDiffuse.a;

			}
			fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten) {
				return fixed4(0, 0, 0, 0);//half4(s.Albedo, s.Alpha);
			}



			ENDCG

			UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
			UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
	}

		Dependency "AddPassShader" = "NatureManufacture Shaders/Terrain/StandardSmoothnessAddPass"

		//Fallback "Nature/Terrain/Diffuse"
}