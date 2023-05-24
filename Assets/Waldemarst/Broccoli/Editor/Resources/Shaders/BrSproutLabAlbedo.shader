Shader "Hidden/Broccoli/SproutLabAlbedo"
{
    Properties {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _TintColor ("Tint Color", Color) = (0.5,1,1,1)
        _BranchShade ("Branch Shade", Float) = 1
        _BranchSat ("Branch Saturation", Float) = 1
        _SproutSat ("SproutSaturation", Float) = 1
        _ApplyExtraSat ("Apply Saturation", Float) = 1
        _IsLinearColorSpace ("Is Linear Color Space", Float) = 0
    }
    SubShader {
        Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
        LOD 100

        Cull Off
        Lighting Off
        //Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            Name "Albedo"
            HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
                #pragma multi_compile_fog
                
                #include "UnityCG.cginc"
                #include "BlendModes.hlsl"

                struct appdata_t {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                    float4 uv3: TEXCOORD3;
                    fixed4 color : COLOR;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
                    float2 texcoord : TEXCOORD0;
                    float4 uv3: TEXCOORD3;
                    fixed4 color : COLOR;
                    UNITY_FOG_COORDS(1)
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed _Cutoff;
                float4 _TintColor;
                float _BranchShade;
                float _BranchSat;
                float _SproutSat;
                float _ApplyExtraSat;
                float _IsLinearColorSpace;

                v2f vert (appdata_t v)
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    //UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.color = v.color;
                    o.uv3 = v.uv3;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }
                
                fixed4 frag (v2f i) : Color
                {
                    fixed4 col = tex2D(_MainTex, i.texcoord);
                    if (_IsLinearColorSpace) {
                        col.rgb = pow (col.rgb, 0.4545);
                        _TintColor.rgb = pow (_TintColor.rgb, 0.4545);
                    }
                    fixed4 vcol = i.color;
                    clip(col.a * vcol.a - _Cutoff);
                    //UNITY_APPLY_FOG(i.fogCoord, col);
                    col.rgb *= 1 - ((1 - i.color.r) / 2);

                    if (i.color.g == 0) { // Green channel is 0 for sprouts.
                        float3 shiftedColor = BlendColor (col, lerp(col, _TintColor.rgb, i.color.b));

                        // preserve vibrance
                        half maxBase = max(col.r, max(col.g, col.b));
                        half newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
                        maxBase /= newMaxBase;
                        maxBase = maxBase * 0.5f + 0.5f;
                        shiftedColor.rgb *= maxBase;

                        if (_ApplyExtraSat == 0) {
                            col.rgb = ContrastSaturationBrightness (saturate (shiftedColor), 1.0, _SproutSat, 1);
                        } else {
                            col.rgb = ContrastSaturationBrightness (saturate (shiftedColor), 1.0, _SproutSat * 1.2, 1.1);
                        }
                    } else { // Green channel is >0 for branches.
                        col.rgb = ContrastSaturationBrightness (col.rgb, 1, _BranchSat, 1);
                    }
                    if (_IsLinearColorSpace)
                        col.rgb = pow (col.rgb, 2.2);
                    return  col;
                }
                
            ENDHLSL
        }
    }
}