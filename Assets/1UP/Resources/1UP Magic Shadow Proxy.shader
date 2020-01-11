Shader "1UP/Magic Shadow/UNLIT"
{
		Properties
		{
				_OutlineColor("Outline Color", Color) = (1, 0, 0, 1)
				_OutlineOffset("Outline Offset", Range(0, 0.3)) = 0.0

				_LightPos("Light Position", Vector) = (100, 100, 100, 0)
				[PowerSlider(10.0)]_Roughness("Roughness", Range(0.0, 1.0)) = 0.0
				//_Metallic ("Metallic", Range(0, 1.0)) = 0.0
				_AoColor("AO Color", Color) = (0, 0, 0, 1)
				_AoSpread("AO Spread", Range(0.0, 300.0)) = 10.0
				_AoRadius("AO Radius", Range(1.0, 30.0)) = 1.0
				_AoXOffset("AO X Offset", Range(-1.0, 1.0)) = 0.0
				_AoYOffset("AO Y Offset", Range(-1.0, 1.0)) = 0.0
				[Toggle(_AOOnly)] _AOOnly("Debug AO", Float) = 0

				_BlendUvTex("Diffuse Texture", 2D) = "white" {}
				_BlendColor("Diffuse Color", Color) = (1,1,1,1)
				_BlendIntensity("Diffuse Instensity", Range(0.0, 1.0)) = 0.0
				[KeywordEnum(UV0, UV1)] _UV_CHANNEL("UV channel", Float) = 0

				_MaterialTex("MatCap Texture", 2D) = "white" {}
				_MaterialColor("MatCap Color", Color) = (1,1,1,1)
				[Toggle(_RevertMatCap)] _RevertMatCap("Revert MatCap", Float) = 0
				[KeywordEnum(None, OpenCV, Linear)] _GRAY("MatCap GrayStyle", Float) = 0

				_ReflectTex("Texture", 2D) = "white" {}
				_ReflectIntensity("Instensity", Range(0.0, 1.0)) = 0.0
				_ReflectRoughness("Roughness", Range(0, 1.0)) = 0.0
				_CubeMap("Reflect CubeMap", Cube) = "" {}
				[Toggle(_UseCubeMap)] _UseCubeMap("Use CubeMap", Float) = 0

				_ShadowColor("Shadow Color", Color) = (0, 0, 0, 1)
				_ShadowFixOffset("Shadow Fix Offset", Range(0, 1.0)) = 0.0
				_ShadowBlur("Shadow Blur", Range(0.0, 10.0)) = 1.0
				_ShadowNoise("Shadow Noise", Range(0.0, 30.0)) = 1.0
				[Toggle(_ShadowOnly)] _ShadowOnly("Debug Indirect Shadow", Float) = 0
		
				[HideInInspector] _ShadowTex0("Shadow Texture0", 2D) = "black" {}
				[HideInInspector] _ShadowOffset0("Shadow Tex0 Enable", float) = 0.0
				[HideInInspector] _ShadowTex1("Shadow Texture1", 2D) = "black" {}
				[HideInInspector] _ShadowOffset1("Shadow Tex1 Enable", float) = 0.0
				[HideInInspector] _ShadowTex2("Shadow Texture2", 2D) = "white" {}
				[HideInInspector] _ShadowOffset2("Shadow Tex2 Enable", float) = 0.0

				//_DepthTex("DepthTex", 2D) = "white" {}	
		}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
