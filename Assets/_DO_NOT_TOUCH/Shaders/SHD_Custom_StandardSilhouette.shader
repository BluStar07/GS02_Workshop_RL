Shader "Custom/Standard (Silhouette)" {

	Properties {

		[Header(MAIN PROPERTIES)]
		[Space(10)]
		_Color ("Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		[HDR] _EmissionColor ("Emission color", Color) = (0, 0, 0, 0)

		[Header(SHADING)]
		[Space(10)]
		[HDR] _RimColor ("Rim color", Color) = (0.5, 0.5, 0.5, 0.5)
		_RimPower ("Rim power", Range(0, 10)) = 2

		[Header(MISC)]
		[Space(10)]
		_SilhouetteColor ("Silhouette color", Color) = (0.5, 0.5, 0.5, 1)
	}

	CGINCLUDE
	#include "UnityCG.cginc"
 
	struct appdata {
		float4 vertex : POSITION;
	};
 
	struct v2f {
		float4 pos : POSITION;
		float4 color : COLOR;
	};
 
	uniform float4 _SilhouetteColor;
 
	v2f vert(appdata v) {

		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.color = _SilhouetteColor;
		return o;
	}
	ENDCG

	SubShader {

		Pass {
			Name "SILHOUETTE"
			Cull Off
			ZWrite Off
			ZTest Always
			ColorMask RGB

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
 
			half4 frag(v2f i) : COLOR {
				return i.color;
			}
			ENDCG
		}

		Tags { "RenderType"="Opaque" "Queue"="Geometry+10" }
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		struct Input {

			float3 viewDir;
		};

		float _Glossiness;
		float _Metallic;
		float4 _Color;
		float4 _EmissionColor;

		float4 _RimColor;
		float _RimPower;

		void surf (Input IN, inout SurfaceOutputStandard o) {

			o.Albedo = _Color;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Emission = _EmissionColor;

			float rimDot = pow(clamp(1 - dot(IN.viewDir, o.Normal), 0, 1), _RimPower);
			o.Emission += rimDot * _RimColor;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
