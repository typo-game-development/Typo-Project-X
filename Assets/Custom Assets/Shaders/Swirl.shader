// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/Swirl" {
	Properties {
		_TestTex ("Texture Renderer", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Color2 ("Color", Color) = (1,1,1,1)
		_OuterTex ("Outer (RGB)", 2D) = "white" {}
		_RotationSpeed ("Rotation Speed", Float) = 2.0

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard fullforwardshadows

		#pragma surface surf Lambert vertex:vert


		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _OuterTex;
				sampler2D _TestTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv_OuterTex;
			float2 uv_TestTex;

		};

		fixed4 _Color;
				fixed4 _Color2;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)
		
		// possibly add interactivity
		//uniform float radius
		//uniform float angle
		//uniform float2 center
		    float _RotationSpeed;

            void vert (inout appdata_full v) {
                v.texcoord.xy -=0.5;
                float s = sin ( _RotationSpeed * _Time );
                float c = cos ( _RotationSpeed * _Time );
                float2x2 rotationMatrix = float2x2( c, -s, s, c);
                rotationMatrix *=0.5;
                rotationMatrix +=0.5;
                rotationMatrix = rotationMatrix * 2-1;
                v.texcoord.xy = mul ( v.texcoord.xy, rotationMatrix );
                v.texcoord.xy += 0.5;
            }

		fixed4 Swirl(sampler2D tex, inout float2 uv, float time) {
			float radius = 1000;
			float2 center = float2(_ScreenParams.x, _ScreenParams.y);
			float2 texSize = float2(_ScreenParams.x / 0.5, _ScreenParams.y / 0.5);
			float2 tc = uv * texSize;
			tc -= center;

			float dist = length(tc);
			float angle =1;
			float fixedTime =  _Time.y;

			if (dist < radius)
			{
				float percent = (radius - dist) / radius;
				float theta = percent * percent * angle * 28.0;
				float s = sin(theta);
				float c = cos(theta);

				tc = float2(dot(tc, float2(c, -s)), dot(tc, float2(s, c)));
			}

			tc += center;
			float3 color = tex2D(tex, tc / texSize).rgb;
			
			//color.r = 1.0;
			return fixed4(color, 1.0);
		}

		void surf (Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = Swirl(_MainTex, IN.uv_MainTex.xy, 1) * _Color;

			fixed4 c1 = Swirl(_OuterTex, IN.uv_OuterTex.xy, 1) * _Color2;
						
			//fixed4 c2 = Swirl(_TestTex, IN.uv_TestTex.xy, 1);
			

			o.Albedo = c.rgb + c1.rbg;
			// Metallic and smoothness come from slider variables
			o.Alpha = c.a + c1.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
