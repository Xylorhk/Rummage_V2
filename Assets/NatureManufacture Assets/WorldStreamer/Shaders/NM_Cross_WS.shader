Shader "NatureManufacture Shaders/Trees/Cross WS"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.65
		_ColorAdjustment("Color Adjustment", Vector) = (1,1,1,0)
		_MainTex("MainTex", 2D) = "white" {}
		_HealthyColor("Healthy Color", Color) = (1,0.9735294,0.9338235,1)
		_Smooothness("Smooothness", Float) = 0.3
		_AO("AO", Float) = 1
		[NoScaleOffset]_BumpMap("BumpMap", 2D) = "bump" {}
		_BumpScale("BumpScale", Range( 0 , 3)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows dithercrossfade 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _BumpScale;
		uniform sampler2D _BumpMap;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float4 _HealthyColor;
		uniform float3 _ColorAdjustment;
		uniform float _Smooothness;
		uniform float _AO;
		uniform float _Cutoff = 0.65;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv0_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			o.Normal = UnpackScaleNormal( tex2D( _BumpMap, uv0_MainTex ), _BumpScale );
			float4 tex2DNode2 = tex2D( _MainTex, uv0_MainTex );
			o.Albedo = ( ( tex2DNode2 * _HealthyColor ) * float4( _ColorAdjustment , 0.0 ) ).rgb;
			float3 temp_cast_2 = (0.0).xxx;
			o.Specular = temp_cast_2;
			o.Smoothness = _Smooothness;
			o.Occlusion = _AO;
			o.Alpha = 1;
			clip( tex2DNode2.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}