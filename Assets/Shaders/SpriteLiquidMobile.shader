Shader "SphereMerge/Sprite Liquid Mobile"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _LiquidColor ("Liquid Color", Color) = (0.08,0.55,1,0.82)
        _FillAmount ("Fill Amount", Range(0,1)) = 0.5
        _GlowColor ("Glow Color", Color) = (0.25,0.9,1,1)
        _GlowIntensity ("Glow Intensity", Range(0,4)) = 1
        [HideInInspector] _LiquidUp ("Liquid Up", Vector) = (0,1,0,0)
        [HideInInspector] _LiquidOffset ("Liquid Offset", Vector) = (0,0,0,0)
        [HideInInspector] _RendererColor ("Renderer Color", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

        HLSLINCLUDE
        #pragma target 2.0

        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

        struct Attributes
        {
            COMMON_2D_INPUTS
            half4 color : COLOR;
            UNITY_SKINNED_VERTEX_INPUTS
        };

        struct Varyings
        {
            COMMON_2D_OUTPUTS
            half4 color : COLOR;
        };

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_AlphaTex);
        SAMPLER(sampler_AlphaTex);

        CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half4 _LiquidColor;
            half _FillAmount;
            half4 _GlowColor;
            half _GlowIntensity;
            half4 _LiquidUp;
            half4 _LiquidOffset;
            half _EnableExternalAlpha;
        CBUFFER_END

        Varyings Vert(Attributes input)
        {
            Varyings output = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            UNITY_SKINNED_VERTEX_COMPUTE(input);
            SetUpSpriteInstanceProperties();
            input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

            output.positionCS = TransformObjectToHClip(input.positionOS);
#if defined(DEBUG_DISPLAY)
            output.positionWS = TransformObjectToWorld(input.positionOS);
            output.normalWS = TransformObjectToWorldNormal(input.normal);
#endif
            output.uv = input.uv;
            output.color = input.color * _Color * unity_SpriteColor;
            return output;
        }

        half4 Frag(Varyings input) : SV_Target
        {
            half4 sprite = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
            half externalAlpha = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, input.uv).r;
            sprite.a = lerp(sprite.a, externalAlpha * input.color.a, _EnableExternalAlpha);

            half2 centered = input.uv - half2(0.5h, 0.5h);
            half2 liquidCentered = centered - _LiquidOffset.xy;
            half upLengthSquared = max(dot(_LiquidUp.xy, _LiquidUp.xy), 0.001h);
            half2 up = _LiquidUp.xy * rsqrt(upLengthSquared);

            const half surfaceSoftness = 0.02h;
            const half liquidRadius = 0.47h;
            const half edgeSoftness = 0.018h;

            half height = _FillAmount - 0.5h;
            half surface = height - dot(liquidCentered, up);
            half liquidMask = smoothstep(-surfaceSoftness, surfaceSoftness, surface);
            half innerMask = 1.0h - smoothstep(liquidRadius - edgeSoftness, liquidRadius, length(centered));

            half visibleLiquid = liquidMask * innerMask;
            half surfaceGlow = 1.0h - smoothstep(0.0h, surfaceSoftness * 5.0h, abs(surface));
            half edgeGlow = smoothstep(liquidRadius - 0.12h, liquidRadius, length(centered));
            half glow = (surfaceGlow * 0.8h + edgeGlow * 0.3h + 0.12h) * visibleLiquid;

            half3 liquidRgb = _LiquidColor.rgb + _GlowColor.rgb * _GlowIntensity * glow;
            half3 finalRgb = lerp(liquidRgb, sprite.rgb, sprite.a);
            half finalAlpha = max(sprite.a, _LiquidColor.a * visibleLiquid * input.color.a);

            return half4(finalRgb, finalAlpha);
        }
        ENDHLSL

        Pass
        {
            Name "SpriteLiquid"
            Tags { "LightMode"="Universal2D" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE
            ENDHLSL
        }

        Pass
        {
            Name "SpriteLiquidForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
