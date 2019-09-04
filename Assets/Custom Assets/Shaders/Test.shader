// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "StylizedWater/Desktop Bouy"
{
	Properties
	{
		[HideInInspector] _DummyTex( "", 2D ) = "white" {}
		_Obstacle("Obstacle", vector) = (0,0,0)
		_WaterColor("Water Color", Color) = (0.1176471,0.6348885,1,0)
		_WaterShallowColor("WaterShallowColor", Color) = (0.4191176,0.7596349,1,0)
		_FresnelColor("Fresnel Color", Color) = (1,1,1,0.484)
		_RimColor("Rim Color", Color) = (1,1,1,1)
		_NormalStrength("NormalStrength", Range( 0 , 1)) = 1
		_Transparency("Transparency", Range( 0 , 1)) = 0.75
		_Glossiness("Glossiness", Range( 0 , 1)) = 1
		_Fresnelexponent("Fresnel exponent", Float) = 4
		_ReflectionStrength("Reflection Strength", Range( 0 , 1)) = 0
		_RefractionAmount("Refraction Amount", Range( 0 , 0.1)) = 0
		[Toggle]_Worldspacetiling("Worldspace tiling", Float) = 1
		_Tiling("Tiling", Range( 0 , 1)) = 0.9
		_RimDistance("Rim Distance", Range( 0.01 , 3)) = 0.2448298
		_RimSize("Rim Size", Range( 0 , 20)) = 0
		_Rimfalloff("Rim falloff", Range( 0.1 , 50)) = 0
		_Rimtiling("Rim tiling", Float) = 2
		_SurfaceHighlight("Surface Highlight", Range( -1 , 1)) = 0.05
		[Toggle]_HighlightPanning("HighlightPanning", Float) = 0
		_Surfacehightlightsize("Surface hightlight size", Float) = 0
		_SurfaceHightlighttiling("Surface Hightlight tiling", Float) = 0.25
		_Depth("Depth", Range( 0 , 100)) = 30
		_Wavesspeed("Waves speed", Range( 0 , 10)) = 0.75
		_WaveHeight("Wave Height", Range( 0 , 1)) = 0
		_Tessellation("Tessellation", Range( 0.1 , 100)) = 0.1
		_Wavetint("Wave tint", Range( -1 , 1)) = 0
		_WaveFoam("Wave Foam", Range( 0 , 10)) = 0
		_WaveSize("Wave Size", Range( 0 , 10)) = 0.5
		[NoScaleOffset][Normal]_Normals("Normals", 2D) = "bump" {}
		[NoScaleOffset]_Shadermap("Shadermap", 2D) = "black" {}
		[HideInInspector]_ReflectionTex("ReflectionTex", 2D) = "gray" {}
		_UnlitAlbedo("UnlitAlbedo", Range(0, 1)) = 0
		_UnlitEmission("UnlitEmission", Range(0, 1)) = 0
		_UseIntersectionHighlight("UseIntersectionHighlight", Range( 0 , 1)) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "IsEmissive" = "true"  }
		LOD 200
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		BlendOp Add
		GrabPass{ "_GrabScreen0" }
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Tessellation.cginc"
		#pragma target 4.6
		#pragma surface surf Standard alpha:fade keepalpha noshadow nodirlightmap vertex:vertexDataFunc //tessellate:tessFunction 
		struct Input
		{
			float2 uv_DummyTex;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float4 screenPos;
			fixed4 color : COLOR;
		};

		struct appdata
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			float4 texcoord3 : TEXCOORD3;
			fixed4 color : COLOR;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};
		

		//Water parameters
		float _WaterTime;
		float _WaterScale;
		float _WaterSpeed;
		float _WaterDistance;
		uniform float4 _WaveInterference;
		float _WaveDirectionWeight;
		float _NoiseStrength;
		float _NoiseWalk;

		float3 _Obstacle;
		uniform sampler2D _Normals;
		uniform float _Worldspacetiling;
		uniform sampler2D _DummyTex;
		uniform float _Tiling;
		uniform float _Wavesspeed;
		uniform float4 _WaveDirection;
		uniform float _SurfaceHighlight;
		uniform float _Surfacehightlightsize;
		uniform sampler2D _Shadermap;
		uniform float _WaveSize;
		uniform float _SurfaceHightlighttiling;
		uniform float _UseIntersectionHighlight;
		uniform float _HighlightPanning;
		uniform float4 _RimColor;
		uniform sampler2D _CameraDepthTexture;
		uniform float _Rimfalloff;
		uniform float _Rimtiling;
		uniform float _RimSize;
		uniform float _NormalStrength;
		uniform float _UnlitAlbedo;
		uniform sampler2D _GrabScreen0;
		uniform float _RefractionAmount;
		uniform float4 _WaterShallowColor;
		uniform float4 _WaterColor;
		uniform float _Depth;
		uniform float _Transparency;
		uniform sampler2D _ReflectionTex;
		uniform float _ReflectionStrength;
		uniform float _Wavetint;
		uniform float4 _FresnelColor;
		uniform float _Fresnelexponent;
		uniform float _WaveFoam;
		uniform float _Glossiness;
		uniform float _RimDistance;
		uniform float _WaveHeight;
		uniform float _Tessellation;
		uniform float _UnlitEmission;

	//	float4 tessFunction( appdata v0, appdata v1, appdata v2 )
	//	{
	//		float4 temp_cast_0 = (_Tessellation).xxxx;
	//		return temp_cast_0;
	//	}

		//The wave function
		float3 getWavePos(appdata v)
		{
			float3 pos = v.vertex.xyz;

			float noiseSample = tex2Dlod(_Shadermap, float4(pos.x, pos.z + sin(_WaterTime * _NoiseWalk), 0.0, 0.0) * 0.1).a * _NoiseStrength;

			pos.y += (sin((_WaterTime * _WaterSpeed + ((v.vertex.x + noiseSample) * _WaveDirection.x) + ((v.vertex.z + noiseSample) * _WaveDirection.z)) / _WaterDistance) * _WaterScale) * _WaveDirectionWeight;

			pos.y += (sin((_WaterTime * _WaterSpeed + ((v.vertex.x + noiseSample) * _WaveDirection.x)) / _WaterDistance) * _WaterScale) * _WaveInterference.x;
			pos.y += (sin((_WaterTime * _WaterSpeed + ((v.vertex.z + noiseSample) * _WaveDirection.z)) / _WaterDistance) * _WaterScale) * _WaveInterference.z;

			//pos.y += (sin(_WaterTime * noiseSample) * _WaterScale);

			return pos;
		}

		void vertexDataFunc( inout appdata v)
		{
			//float3 ase_vertexNormal = v.normal.xyz;
			//v.texcoord.xy = v.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
			//float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			//float2 Tiling21 = ( lerp(( -20.0 * v.texcoord.xy ),(ase_worldPos).xz,_Worldspacetiling) * ( 1.0 - _Tiling ) );
			//float2 appendResult500 = (float2(_WaveDirection.x , _WaveDirection.z));
			//float2 WaveSpeed40 = ( ( ( _Wavesspeed * 0.1 ) * _Time.y ) * appendResult500 );
			//float2 temp_output_344_0 = ( ( ( Tiling21 * _WaveSize ) * 0.1 ) + ( WaveSpeed40 * 0.5 ) );
			//float4 tex2DNode94 = tex2Dlod( _Shadermap, float4( temp_output_344_0, 0, 1.0) );
			//float temp_output_95_0 = ( _WaveHeight * tex2DNode94.g );
			//float3 Displacement100 = ( ase_vertexNormal * temp_output_95_0 );

			//Manipulate the position
			float3 withWave = getWavePos(v);
			v.vertex.xyz = withWave;

			//o.height = withWave.y;
			v.color.y = withWave.y;
			
			//v.texcoord3 = mul(unity_ObjectToWorld, v.vertex);
			//v.vertex.xyz += Displacement100;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 texCoordDummy12 = i.uv_DummyTex*float2( 1,1 ) + float2( 0,0 );
			float3 ase_worldPos = i.worldPos;
			float2 Tiling21 = ( lerp(( -20.0 * texCoordDummy12 ),(ase_worldPos).xz,_Worldspacetiling) * ( 1.0 - _Tiling ) );
			float2 appendResult500 = (float2(_WaveDirection.x , _WaveDirection.z));
			float2 WaveSpeed40 = ( ( (_WaterSpeed * 0.1 ) * _WaterTime) * appendResult500 );
			float2 temp_output_339_0 = ( ( Tiling21 * 0.25 ) + WaveSpeed40 );
			float3 temp_output_51_0 = BlendNormals( UnpackNormal( tex2D( _Normals, ( Tiling21 + -WaveSpeed40 ) ) ) , UnpackNormal( tex2D( _Normals, temp_output_339_0 ) ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			//float2 temp_output_344_0 = ( ( ( Tiling21 * _WaterScale) * 0.1 ) + ( WaveSpeed40 * 0.5 ) );
			//float4 tex2DNode94 = tex2D( _Shadermap, temp_output_344_0 );
			
			float Heightmap99 = i.color.y;

			//float Heightmap99 = tex2DNode94.g;

			float temp_output_609_0 = ( Heightmap99 * 0.1 );
			float4 tex2DNode66 = tex2D( _Shadermap, ( ( ( Tiling21 * 0.5 ) + temp_output_609_0 ) * _SurfaceHightlighttiling ) );
			float lerpResult600 = lerp( tex2DNode66.r , tex2DNode66.b , _UseIntersectionHighlight);
			float4 tex2DNode67 = tex2D( _Shadermap, ( _SurfaceHightlighttiling * ( Tiling21 + -temp_output_609_0 ) ) );
			float lerpResult598 = lerp( tex2DNode67.r , tex2DNode67.b , _UseIntersectionHighlight);
			float lerpResult601 = lerp( step( _Surfacehightlightsize , ( lerpResult600 - lerpResult598 ) ) , ( 1.0 - lerp(lerpResult598,( lerpResult600 * lerpResult598 ),_HighlightPanning) ) , _UseIntersectionHighlight);
			float SurfaceHighlights73 = ( _SurfaceHighlight * lerpResult601 );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth493 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(ase_screenPos))));
			float distanceDepth493 = abs( ( screenDepth493 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( 1.0 ) );
			float DepthTexture494 = distanceDepth493;
			float2 temp_output_24_0 = ( Tiling21 * _Rimtiling );
			float3 NormalsBlended362 = temp_output_51_0;
			float temp_output_30_0 = ( tex2D( _Shadermap, ( float3( ( 0.5 * temp_output_24_0 ) ,  0.0 ) + ( NormalsBlended362 * 0.1 ) ).xy ).b * tex2D( _Shadermap, ( temp_output_24_0 + WaveSpeed40 ) ).b );
			float clampResult438 = clamp( ( _RimColor.a * ( 1.0 - ( ( ( DepthTexture494 / _Rimfalloff ) * temp_output_30_0 * 3.0 ) + ( DepthTexture494 / _RimSize ) ) ) ) , 0.0 , 1.0 );
			
			float distLimit = 5;// how far away does obstacle reach
			float distMulti = (distLimit - min(distLimit, distance(float3(i.worldPos.x, i.worldPos.y, i.worldPos.z), float3(_Obstacle.x, _Obstacle.y, _Obstacle.z)))) / distLimit; 
			float Intersection42 = clampResult438 * clamp((1 - distMulti), 0, 1.0);		

			//float Intersection42 = clampResult438;
			
			float3 lerpResult82 = lerp( temp_output_51_0 , ase_vertexNormal , ( SurfaceHighlights73 + Intersection42 ));
			float3 lerpResult621 = lerp( float3(0,1,0) , lerpResult82 , _NormalStrength);
			float3 NormalMap52 = lerpResult621;
			o.Normal = NormalMap52;
			float4 ase_screenPos266 = ase_screenPos;
			#if UNITY_UV_STARTS_AT_TOP
			float scale266 = -1.0;
			#else
			float scale266 = 1.0;
			#endif
			float halfPosW266 = ase_screenPos266.w * 0.5;
			ase_screenPos266.y = ( ase_screenPos266.y - halfPosW266 ) * _ProjectionParams.x* scale266 + halfPosW266;
			ase_screenPos266.xyzw /= ase_screenPos266.w;
			float2 appendResult501 = (float2(ase_screenPos266.r , ase_screenPos266.g));
			float3 temp_output_359_0 = ( ( _RefractionAmount * NormalsBlended362 ) + float3( appendResult501 ,  0.0 ) );
			float4 screenColor372 = tex2D( _GrabScreen0, temp_output_359_0.xy );
			float4 RefractionResult126 = screenColor372;
			float screenDepth105 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(ase_screenPos))));
			float distanceDepth105 = abs( ( screenDepth105 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Depth ) );
			float clampResult144 = clamp( distanceDepth105 , 0.0 , 1.0 );
			float Depth479 = clampResult144;
			float4 lerpResult478 = lerp( _WaterShallowColor , _WaterColor , Depth479);
			float clampResult133 = clamp( ( ( _Transparency + Intersection42 ) - ( ( 1.0 - Depth479 ) * ( 1.0 - _WaterShallowColor.a ) ) ) , 0.0 , 1.0 );
			float Opacity121 = clampResult133;
			float4 lerpResult374 = lerp( RefractionResult126 , lerpResult478 , Opacity121);
			float4 Reflection265 = tex2D( _ReflectionTex, temp_output_359_0.xy );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float fresnelNDotV508 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode508 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNDotV508, 3.0 ) );
			float4 lerpResult297 = lerp( lerpResult374 , Reflection265 , ( ( Opacity121 * _ReflectionStrength ) * fresnelNode508 ));
			float4 WaterColor350 = lerpResult297;
			float4 temp_cast_6 = (( Heightmap99 * _Wavetint )).xxxx;
			float4 RimColor102 = _RimColor;
			float4 lerpResult61 = lerp( ( WaterColor350 - temp_cast_6 ) , ( RimColor102 * 3.0 ) , Intersection42);
			float4 FresnelColor206 = _FresnelColor;
			float fresnelNDotV199 = dot( ase_vertexNormal, ase_worldViewDir );
			float fresnelNode199 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNDotV199, ( _Fresnelexponent * 100.0 ) ) );
			float clampResult505 = clamp( ( _FresnelColor.a * fresnelNode199 ) , 0.0 , 1.0 );
			float Fresnel205 = clampResult505;
			float4 lerpResult207 = lerp( ( lerpResult61 + SurfaceHighlights73 ) , FresnelColor206 , Fresnel205);
			float4 temp_cast_7 = (1.0).xxxx;
			float SurfaceHighlightTex244 = lerpResult601;
			float clampResult401 = clamp( ( pow( (Heightmap99 * _WaveFoam ) , 2.0 ) * SurfaceHighlightTex244 ) , 0.0 , 1.0 );
			//float clampResult401 = clamp((pow((tex2DNode94.g * _WaveFoam), 2.0) * SurfaceHighlightTex244), 0.0, 1.0);
			float WaveFoam221 = clampResult401;
			float4 lerpResult223 = lerp( lerpResult207 , temp_cast_7 , WaveFoam221);
			float4 FinalColor114 = lerpResult223;
			o.Albedo = ( ( 1.0 - _UnlitAlbedo ) * FinalColor114 ).rgb;
			o.Emission = ( _UnlitEmission * FinalColor114 ).rgb;
			o.Smoothness = _Glossiness;
			float clampResult499 = clamp( ( DepthTexture494 / _RimDistance ) , 0.0 , 1.0 );
			
			//distMulti =  distMulti / distLimit; //distance falloff
			//o.Alpha = clampResult499 * clamp((1 - distMulti), 0.7, 1.0);
			
			o.Alpha = clampResult499;
		}

		ENDCG
	}
}
