Shader "Custom/Tri Planar Standard" {

	Properties {

		_Color ("Color", Color) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("Albedo (RGB) Emission strength (A)", 2D) = "white" {}
		[NoScaleOffset] _SpecTex ("Metallic (RRB) Smoothness(A)", 2D) = "black" {}
		_TextureScale ("Main texture scale", Float) = 1
		//_EmissionColor ("Emission color", Color) = (0, 0, 0, 1)
		_EmissionStrength ("Emission strength", Range(0, 8)) = 0
	}

	SubShader {

		Tags { "RenderType"="Opaque"}
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _SpecTex;

		fixed _Metallic;
		fixed _TextureScale;

		struct Input {

			//float2 uv_MainTex;
			fixed3 worldPos;
		};

		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
#define _Color_arr Props
			//UNITY_DEFINE_INSTANCED_PROP(fixed4, _EmissionColor)
			UNITY_DEFINE_INSTANCED_PROP(fixed, _EmissionStrength)
#define _EmissionStrength_arr Props
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {

			// Creating UVs depending on worldPos
			fixed2 xUV = fixed2(IN.worldPos.z, IN.worldPos.y) / _TextureScale;
			fixed2 yUV = fixed2(IN.worldPos.x, IN.worldPos.z) / _TextureScale;
			fixed2 zUV = fixed2(IN.worldPos.x, IN.worldPos.y) / _TextureScale;

			// Sampling texture depending on created UVs
			fixed4 mtx = tex2D (_MainTex, xUV);
			fixed4 mty = tex2D (_MainTex, yUV);
			fixed4 mtz = tex2D (_MainTex, zUV);
			fixed4 stx = tex2D (_SpecTex, xUV);
			fixed4 sty = tex2D (_SpecTex, yUV);
			fixed4 stz = tex2D (_SpecTex, zUV);

			// Getting dot products
			fixed xDot = abs(dot(fixed3(1, 0, 0), o.Normal));
			fixed yDot = abs(dot(fixed3(0, 1, 0), o.Normal));
			fixed zDot = abs(dot(fixed3(0, 0, 1), o.Normal));

			// Getting blend values
			fixed3 blend = (fixed3(xDot, yDot, zDot)) / (xDot + yDot + zDot);
			fixed4 mt = mtx * blend.x + mty * blend.y + mtz * blend.z;
			fixed4 st = stx * blend.x + sty * blend.y + stz * blend.z;

			// Applying values
			o.Albedo = mt.rgb * UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color);
			o.Metallic = st.rgb * UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color);
			o.Smoothness = st.a;
			//o.Emission = UNITY_ACCESS_INSTANCED_PROP(_EmissionColor) * UNITY_ACCESS_INSTANCED_PROP(_EmissionStrength) * mt.a;
			o.Emission = o.Albedo * UNITY_ACCESS_INSTANCED_PROP(_EmissionStrength_arr, _EmissionStrength);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
