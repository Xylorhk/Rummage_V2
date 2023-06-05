// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "NatureManufacture Shaders/Terrain/StandardNormalAddPass" {
	Properties{
		[HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
	}
		SubShader{
			Tags {
				"Queue" = "Geometry-99"
				"IgnoreProjector" = "True"
				"RenderType" = "Opaque"
			}

			CGPROGRAM
			#pragma surface surf NoLighting decal:add vertex:SplatmapVert finalcolor:SplatmapFinalColor finalgbuffer:SplatmapFinalGBuffer fullforwardshadows nometa
			#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
			#pragma multi_compile_fog
			#pragma target 3.0
			#include "UnityPBSLighting.cginc"

			#pragma multi_compile_local_fragment __ _ALPHATEST_ON
			#pragma multi_compile_local __ _NORMALMAP

			#define TERRAIN_SPLAT_ADDPASS
			#define TERRAIN_STANDARD_SHADER
			#define TERRAIN_INSTANCED_PERPIXEL_NORMAL
			#define TERRAIN_SURFACE_OUTPUT SurfaceOutput
			#include "TerrainSplatmapCommon.cginc"

			float _gamma;

			half _Metallic0;
			half _Metallic1;
			half _Metallic2;
			half _Metallic3;

			half _Smoothness0;
			half _Smoothness1;
			half _Smoothness2;
			half _Smoothness3;

			void surf(Input IN, inout SurfaceOutput o) {
				half4 splat_control;
				half weight;
				fixed4 mixedDiffuse;
				half4 defaultSmoothness = half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);
				float3 normal = o.Normal;
				SplatmapMix(IN, defaultSmoothness, splat_control, weight, mixedDiffuse, normal);

				o.Alpha = weight;
				
				if (_gamma == 1)
				{
					normal = normal * 0.5 + 0.5;
				}
				else
				{
					normal = pow(normal * 0.5 + 0.5, 2.2);
				}

				o.Emission = normal;
			}


			fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten) {
				return fixed4(0, 0, 0, 0);//half4(s.Albedo, s.Alpha);
			}
			ENDCG
	}

}
