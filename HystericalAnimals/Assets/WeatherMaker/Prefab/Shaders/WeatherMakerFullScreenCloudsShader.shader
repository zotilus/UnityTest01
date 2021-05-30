//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// *** A NOTE ABOUT PIRACY ***
// 
// If you got this asset from a pirate site, please consider buying it from the Unity asset store at https://assetstore.unity.com/packages/slug/60955?aid=1011lGnL. This asset is only legally available from the Unity Asset Store.
// 
// I'm a single indie dev supporting my family by spending hundreds and thousands of hours on this and other assets. It's very offensive, rude and just plain evil to steal when I (and many others) put so much hard work into the software.
// 
// Thank you.
//
// *** END NOTE ABOUT PIRACY ***
//

// http://bitsquid.blogspot.com/2016/07/volumetric-clouds.html
// https://github.com/greje656/clouds
// http://patapom.com/topics/Revision2013/Revision%202013%20-%20Real-time%20Volumetric%20Rendering%20Course%20Notes.pdf

Shader "WeatherMaker/WeatherMakerFullScreenCloudsShader"
{
	Properties
	{
		_PointSpotLightMultiplier("Point/Spot Light Multiplier", Range(0, 10)) = 1
		_DirectionalLightMultiplier("Directional Light Multiplier", Range(0, 10)) = 1
		_AmbientLightMultiplier("Ambient light multiplier", Range(0, 4)) = 1
	}
	SubShader
	{
		Cull Off Lighting Off ZWrite Off ZTest Always Fog { Mode Off }
		Blend [_SrcBlendMode][_DstBlendMode]

		CGINCLUDE

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9

#if UNITY_VERSION >= 201910

        // UNITY 2019 and newer
        #pragma multi_compile_local __ WEATHER_MAKER_DOWNSAMPLE_2 WEATHER_MAKER_DOWNSAMPLE_4 WEATHER_MAKER_DOWNSAMPLE_8

#else

        // UNITY 2018 and older support
        #pragma multi_compile __ WEATHER_MAKER_DOWNSAMPLE_2 WEATHER_MAKER_DOWNSAMPLE_4 WEATHER_MAKER_DOWNSAMPLE_8

#endif

		#define WEATHER_MAKER_DEPTH_SHADOWS_OFF
		#define WEATHER_MAKER_LIGHT_NO_DIR_LIGHT
		#define WEATHER_MAKER_LIGHT_NO_NORMALS
		#define WEATHER_MAKER_LIGHT_NO_SPECULAR
		#define WEATHER_MAKER_IS_FULL_SCREEN_EFFECT
		#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES

		#include "WeatherMakerCloudVolumetricShaderInclude.cginc"
		#include "WeatherMakerAtmosphereShaderInclude.cginc"

		inline void GetDepthAndRay(float4 uv, inout float3 rayDir, float3 forwardLine, out float depth, out float depth01)
		{
			rayDir = GetFullScreenRayDir(rayDir);

			// uncomment to mirror clouds down
			// rayDir.y = abs(rayDir.y);

            depth01 = (_WeatherMakerVREnabled ? 1.0 : WM_SAMPLE_DEPTH_DOWNSAMPLED_TEMPORAL_REPROJECTION_01_KEYWORD(uv.xy));

#if defined(WEATHER_MAKER_ENABLE_VOLUMETRIC_CLOUDS)

			if (depth01 >= 1.0)
			{
				// if depth is max value, make an "infinite" depth
				depth = lerp(_CloudPlanetRadiusVolumetric, 1000000.0, _CloudPlanetRadiusVolumetric <= 0.0);
			}
			else
			
#endif

			if (WM_CAMERA_RENDER_MODE_CUBEMAP)
			{
				depth = length(rayDir * depth01 * _ProjectionParams.z);
			}
			else
			{
				depth = length(depth01 * forwardLine);
			}
		}

		ENDCG

		// color pass
		Pass
		{
			CGPROGRAM

			#pragma vertex full_screen_vertex_shader
			#pragma fragment temporal_reprojection_fragment_custom
			#pragma multi_compile_instancing
			#pragma multi_compile WEATHER_MAKER_SHADOWS_ONE_CASCADE WEATHER_MAKER_SHADOWS_SPLIT_SPHERES

			#define WEATHER_MAKER_TEMPORAL_REPROJECTION_FRAGMENT_TYPE wm_full_screen_fragment
			#define WEATHER_MAKER_TEMPORAL_REPROJECTION_FRAGMENT_FUNC full_screen_clouds_frag_impl
			#define WEATHER_MAKER_TEMPORAL_REPROJECTION_BLEND_FUNC blendCloudTemporal

			//#define WEATHER_MAKER_TEMPORAL_REPROJECTION_OFF_SCREEN_FUNC offScreenCloudTemporal

			// leave commented out unless testing performance, red areas are full shader runs, try to minimize these
			//#define WEATHER_MAKER_TEMPORAL_REPROJECTION_SHOW_OVERDRAW fixed4(1,0,0,1)

			inline fixed4 blendCloudTemporal(fixed4 prev, fixed4 cur, fixed4 diff, float4 uv, wm_full_screen_fragment i);
			inline fixed4 offScreenCloudTemporal(fixed4 prev, fixed4 cur, float4 uv, wm_full_screen_fragment i);
			fixed4 full_screen_clouds_frag_impl(wm_full_screen_fragment i) : SV_Target;

			#include "WeatherMakerTemporalReprojectionShaderInclude.cginc"

			inline fixed4 blendCloudTemporal(fixed4 prev, fixed4 cur, fixed4 diff, float4 uv, wm_full_screen_fragment i)
			{
				// if blocked by depth, we have to resend the full pixel, otherwise artifacts everywhere
                UNITY_BRANCH
				if (prev.a < 0.003 || cur.a < 0.003 || diff.x >= _WeatherMakerTemporalReprojectionAlphaThreshold)
				{
					prev = CLOUD_COLOR_FORCE_REDRAW;
				}
                // TODO: Causes a lot of artifacts and does not work as expected to force redraw when depth changes, figure out why...
                /*else if (_WeatherMakerTemporalReprojectionDepthThreshold > 0.0)
                {
                    float prevDepth = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevDepth, _point_clamp_sampler, i.uv.xy).r;
                    if (_WeatherMakerDownsampleScale < 1.5)
                    {
                        // full scale, just compare this depth to the prev depth
                        float depth1 = GetDepth01(i.uv.xy);
                        if (abs(prevDepth - depth1) >= _WeatherMakerTemporalReprojectionDepthThreshold)
                        {
                            prev = CLOUD_COLOR_FORCE_REDRAW;
                        }
                    }
                    else
                    {
                        // only half downsample scale is supported, quarter and eighth will have artifacts
                        // check every depth pixel in the full size image, if there is a threshold difference,
                        // we have to redraw this pixel
                        float2 offsets = _CameraDepthTexture_TexelSize.xy;
                        float2 uv = (floor(i.uv.xy * _CameraDepthTexture_TexelSize.zw) + 0.5) * _CameraDepthTexture_TexelSize.xy;
                        float depth1 = GetDepth01(uv.xy);
                        float depth2 = GetDepth01(float2(uv.x + offsets.x, uv.y));
                        float depth3 = GetDepth01(float2(uv.x, uv.y + offsets.y));
                        float depth4 = GetDepth01(float2(uv.x + offsets.x, uv.y + offsets.y));

                        if (abs(prevDepth - depth1) >= _WeatherMakerTemporalReprojectionDepthThreshold ||
                            abs(prevDepth - depth2) >= _WeatherMakerTemporalReprojectionDepthThreshold ||
                            abs(prevDepth - depth3) >= _WeatherMakerTemporalReprojectionDepthThreshold ||
                            abs(prevDepth - depth4) >= _WeatherMakerTemporalReprojectionDepthThreshold)
                        {
                            prev = CLOUD_COLOR_FORCE_REDRAW;
                        }
                    }
                }*/
                else
                {
                    // make sure no naughtly 0 alpha pixels are neighbors, if so we have to stick with just the pixel value
                    // else if not we can use hardware linear sampling

                    float2 uv1 = float2(uv.z + temporalReprojectionPrevFrameBlurOffsets.x, uv.w + temporalReprojectionPrevFrameBlurOffsets.z);
                    float2 uv2 = float2(uv.z, uv.w + temporalReprojectionPrevFrameBlurOffsets.z);
                    float2 uv3 = float2(uv.z + temporalReprojectionPrevFrameBlurOffsets.y, uv.w + temporalReprojectionPrevFrameBlurOffsets.z);
                    float2 uv4 = float2(uv.z + temporalReprojectionPrevFrameBlurOffsets.x, uv.w);
                    float2 uv5 = float2(uv.z + temporalReprojectionPrevFrameBlurOffsets.y, uv.w);
                    float2 uv6 = float2(uv.z + temporalReprojectionPrevFrameBlurOffsets.x, uv.w + temporalReprojectionPrevFrameBlurOffsets.w);
                    float2 uv7 = float2(uv.z, uv.w + temporalReprojectionPrevFrameBlurOffsets.w);
                    float2 uv8 = float2(uv.z + temporalReprojectionPrevFrameBlurOffsets.y, uv.w + temporalReprojectionPrevFrameBlurOffsets.w);

                    fixed a1 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv1).a;
                    fixed a2 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv2).a;
                    fixed a3 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv3).a;
                    fixed a4 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv4).a;
                    fixed a5 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv5).a;
                    fixed a6 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv6).a;
                    fixed a7 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv7).a;
                    fixed a8 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv8).a;

                    UNITY_BRANCH
                    //if (a2 > 0.0 && a4 > 0.0 && a5 > 0.0 && a7 > 0.0)
                    if (a1 > 0.0 && a2 > 0.0 && a3 > 0.0 && a4 > 0.0 && a5 > 0.0 && a6 > 0.0 && a7 > 0.0 && a8 > 0.0)
                    {
                        // can linear blend because no neighbor blocked by depth buffer
                        prev = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _linear_clamp_sampler, uv.zw);
                    }
                }

                return prev;
			}

			inline fixed4 offScreenCloudTemporal(fixed4 prev, fixed4 cur, float4 uv, wm_full_screen_fragment i)
			{
				return cur;
			}

			fixed4 full_screen_clouds_frag_impl(wm_full_screen_fragment i) : SV_Target
			{
				static const float flatTotal = min(1.0, 1.5 * ((_CloudCover[0] * _CloudDensity[0]) + (_CloudCover[1] * _CloudDensity[1]) + (_CloudCover[2] * _CloudDensity[2]) + (_CloudCover[3] * _CloudDensity[3])));
				float depth, depth01;
				float3 cloudRay = i.forwardLine;
				GetDepthAndRay(i.uv, cloudRay, i.forwardLine, depth, depth01);
                //return fixed4(depth01, depth01, depth01, 1.0);

				fixed4 finalColor = fixed4Zero;
				fixed4 cloudColor;
				fixed alphaAccum = 0.0;

				// top layer
				UNITY_BRANCH
				if (_CloudCover[3] > 0.001)
				{
					cloudColor = ComputeCloudColor(cloudRay, depth, _CloudNoise4, i.uv, 3, alphaAccum);
					blendClouds(cloudColor, finalColor);
				}

				// next layer
				UNITY_BRANCH
				if (_CloudCover[2] > 0.001)
				{
					cloudColor = ComputeCloudColor(cloudRay, depth, _CloudNoise3, i.uv, 2, alphaAccum);
					blendClouds(cloudColor, finalColor);
				}

				// next layer
				UNITY_BRANCH
				if (_CloudCover[1] > 0.001)
				{
					cloudColor = ComputeCloudColor(cloudRay, depth, _CloudNoise2, i.uv, 1, alphaAccum);
					blendClouds(cloudColor, finalColor);
				}

				// bottom layer
				UNITY_BRANCH
				if (_CloudCover[0] > 0.001)
				{
					cloudColor = ComputeCloudColor(cloudRay, depth, _CloudNoise1, i.uv, 0, alphaAccum);
					blendClouds(cloudColor, finalColor);
				}

				// pre-multiply and soften flat layer clouds
				fixed flatAlpha = finalColor.a;
				finalColor *= finalColor.a;
				finalColor.a = lerp(finalColor.a, max(finalColor.a, WM_MIN_PIXEL_VALUE), flatAlpha >= WM_MIN_PIXEL_VALUE);

#if defined(WEATHER_MAKER_ENABLE_VOLUMETRIC_CLOUDS)

				// volumetric layer
				UNITY_BRANCH
				if (_CloudCoverVolumetric > 0.0)
				{
					fixed3 backgroundSkyColor;

					UNITY_BRANCH
					if (_CloudBackgroundSkyIntensityVolumetric.x > 0.0)
					{
						float3 skyRay = normalize(float3(cloudRay.x, abs(cloudRay.y) + _WeatherMakerSkyYOffset2D, cloudRay.z));

						UNITY_BRANCH
						if (_CloudBackgroundSkyIntensityVolumetric.y == 0.0)
						{
							backgroundSkyColor = CalculateSkyColorUnityStyleFragment(skyRay);
						}
						else
						{
							backgroundSkyColor = ComputeAtmosphericScatteringSkyColor(skyRay);
						}
						backgroundSkyColor = lerp(backgroundSkyColor * _CloudBackgroundSkyIntensityVolumetric.x, finalColor.rgb, flatTotal * finalColor.a);
					}
					else
					{
						backgroundSkyColor = finalColor.rgb * finalColor.a;
					}

					// volumetric cloud color is already pre-multiplied
					cloudColor = ComputeCloudColorVolumetric(cloudRay, i.uv, depth, backgroundSkyColor);

					// pre-multiply blend
					finalColor = cloudColor + (finalColor * (1.0 - cloudColor.a));
				}

				UNITY_BRANCH
				if (finalColor.a < 1.0 && weatherMakerNightMultiplierSquared > 0.0)
				{
					// blend aurora borealis, the aurora is already pre-multiplied by alpha
					fixed4 auroraColor = ComputeAurora(WEATHER_MAKER_CAMERA_POS, cloudRay, i.uv, depth) * weatherMakerNightMultiplierSquared;
					finalColor.rgb = (auroraColor.rgb * (1.0 - finalColor.a)) + finalColor.rgb;
					finalColor.a = max(auroraColor.a, finalColor.a);
				}

#endif

				// DEBUG:
				// alpha of 0 should only be behind depth buffer, if it is showing up in a cloud area, it is a bug and will show in red
				//finalColor = lerp(fixed4(1.0, 0.0, 0.0, 1.0), finalColor, ceil(finalColor.a));
				return finalColor;
			}

			ENDCG
		}

		// depth write pass (linear 0 - 1)
		Pass
		{
			CGPROGRAM

			#pragma vertex full_screen_vertex_shader
			#pragma fragment frag
			#pragma multi_compile_instancing

			float4 frag(wm_full_screen_fragment i) : SV_Target
			{ 
				WM_INSTANCE_FRAG(i);

#if defined(WEATHER_MAKER_ENABLE_VOLUMETRIC_CLOUDS)

				UNITY_BRANCH
				if (_CloudCoverVolumetric > 0.0)
				{
					UNITY_BRANCH
					if (unity_OrthoParams.w == 0.0)
					{
						// get the 0-1 depth of the cloud layer start
						float depth, depth01;
						float3 rayDir = i.forwardLine;
						// don't use ray offset, we want the exact depth buffer value
						GetDepthAndRay(i.uv, rayDir, i.forwardLine, depth, depth01);
						float3 startPos, startPos2;
						float3 endPos, endPos2;
						float rayLength, rayLength2;
						float distanceToSphere, distanceToSphere2;
						uint iterations = SetupCloudRaymarch(WEATHER_MAKER_CAMERA_POS, rayDir, depth, depth, volumetricSphereInner, volumetricSphereOutter,
							startPos, endPos, rayLength, distanceToSphere, startPos2, endPos2, rayLength2, distanceToSphere2);

						// return 1.0 / ((_ZBufferParams.x * z) + _ZBufferParams.y);
						// TODO: The left of this lerp is incorrect far above the clouds, figure out why...
						//float cloudLayerDepth01 = GetDepth01FromWorldSpaceRay(rayDir, distanceToSphere);
						float depthPos = length(depth01 * i.forwardLine);
						float cloudLayerDepth01 = saturate(lerp(0.0, depth01, distanceToSphere / depthPos));
						return min(depth01, cloudLayerDepth01);
					}
					else
					{
						// orthographic not supported
						return 1.0;
					}
				}
				else

#endif

				{
					return 1.0;
				}
			}

			ENDCG
		}
		
		// cloud ray pass
		Pass
		{
			Blend One Zero

			CGPROGRAM

			#pragma vertex full_screen_vertex_shader
			#pragma fragment frag
			#pragma multi_compile_instancing

			fixed4 frag(wm_full_screen_fragment i) : SV_Target
			{ 
				WM_INSTANCE_FRAG(i);

#if defined(WEATHER_MAKER_ENABLE_VOLUMETRIC_CLOUDS) && VOLUMETRIC_CLOUD_RENDER_MODE == 1
				
				fixed3 shaftColor = fixed3Zero;
				fixed4 pixelColor = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex2, i.uv.xy);

				// take advantage of the fact that dir lights are sorted by perspective/ortho and then by intensity
				UNITY_LOOP
				for (uint lightIndex = 0;
					lightIndex < uint(_WeatherMakerDirLightCount) &&
					_WeatherMakerDirLightVar1[lightIndex].y == 0.0 &&
					_WeatherMakerDirLightColor[lightIndex].a > 0.001 &&
					_WeatherMakerDirLightVar1[lightIndex].z > 0.001; lightIndex++)
				{
					shaftColor += ComputeDirLightShaftColor(i.uv.xy, 0.01, _WeatherMakerDirLightViewportPosition[lightIndex], _WeatherMakerDirLightColor[lightIndex] * _WeatherMakerDirLightVar1[lightIndex].z, pixelColor);
				}

				return fixed4(shaftColor.rgb, 0.0);

#else

				return fixed4Zero;

#endif

			}

			ENDCG
		}

		// cloud ray blit pass
		Pass
		{
			Blend One One

			CGPROGRAM

			#pragma vertex full_screen_vertex_shader
			#pragma fragment frag
			#pragma multi_compile_instancing

			fixed4 frag(wm_full_screen_fragment i) : SV_Target
			{ 
				WM_INSTANCE_FRAG(i);

				// (0.4,-1.2) , (-1.2,-0.4) , (1.2,0.4) and (-0.4,1.2).
				static const float4 offsets = float4
				(
					_MainTex4_TexelSize.x * 0.4,
					_MainTex4_TexelSize.x * 1.2,
					_MainTex4_TexelSize.y * 0.4,
					_MainTex4_TexelSize.y * 1.2
				);

				fixed4 c;
				GaussianBlur17Tap(c, _MainTex4, i.uv.xy, offsets, 1.0);
				return c;
				//return WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex4, i.uv);
			}

			ENDCG
		}

		// atmosphere blit pass
		Pass
		{
			Blend One Zero

			CGPROGRAM

			#pragma vertex full_screen_vertex_shader
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma multi_compile WEATHER_MAKER_SHADOWS_ONE_CASCADE WEATHER_MAKER_SHADOWS_SPLIT_SPHERES

			fixed4 frag(wm_full_screen_fragment i) : SV_Target
			{
				WM_INSTANCE_FRAG(i);

				fixed4 pixelColor = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex2, i.uv.xy);
				pixelColor.rgb = ComputeAtmosphericScatteringFog(i.uv.xy, WEATHER_MAKER_CAMERA_POS, GetFullScreenRayDir(i.forwardLine), i.forwardLine, fixed4(pixelColor.rgb, 1.0), GetDepth01(i.uv.xy)).rgb;
				return pixelColor;
			}

			ENDCG
		}

		// atmosphere light shaft pass
		Pass
		{
			Blend One Zero

			CGPROGRAM

			#pragma vertex full_screen_vertex_shader
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma multi_compile WEATHER_MAKER_SHADOWS_ONE_CASCADE WEATHER_MAKER_SHADOWS_SPLIT_SPHERES
			#pragma multi_compile __ UNITY_URP

			fixed4 frag(wm_full_screen_fragment i) : SV_Target
			{
				WM_INSTANCE_FRAG(i);

				float rayLength = length(i.forwardLine) * WM_SAMPLE_DEPTH_DOWNSAMPLED_01_KEYWORD(i.uv.xy);
				return ComputeAtmosphericLightShafts(i.vertex.xy, WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET, GetFullScreenRayDir(i.forwardLine), rayLength);
			}

			ENDCG
		}
	}
}
