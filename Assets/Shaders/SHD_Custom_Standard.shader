Shader "Custom/Character" {

	Properties {

		[Header(MAIN PROPERTIES)]
		[Space(10)]
		_Color ("Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		[HDR] _EmissionColor ("Emission color", Color) = (0, 0, 0, 0)
		_MaxEmission ("Max emission strength", Float) = 2

		[Header(SHADING)]
		[Space(10)]
		[HDR] _RimColor ("Rim color", Color) = (0.5, 0.5, 0.5, 0.5)
		_RimPower ("Rim power", Range(0, 10)) = 2
	}

	SubShader {

		Tags { "RenderType"="Opaque" "Queue"="Geometry+10" }
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		struct Input {

			float3 viewDir;
			float4 color : COLOR;
		};

		float _Glossiness;
		float _Metallic;
		float4 _Color;
		float4 _EmissionColor;
		float _MaxEmission;

		float4 _RimColor;
		float _RimPower;

		void surf (Input IN, inout SurfaceOutputStandard o) {

			o.Albedo = _Color * IN.color;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Emission = _EmissionColor;

			float rimDot = pow(clamp(1 - dot(IN.viewDir, o.Normal), 0, 1), _RimPower);
			o.Emission += rimDot * _RimColor;
			o.Emission = clamp(o.Emission, 0, _MaxEmission);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
