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
        _OutlineEnabled ("Outline Enabled", Float) = 1
        _OutlineWidth ("Outline Width", Range(0,16)) = 4
        _OutlineColor ("Outline Color", Color) = (0.03,0.22,0.55,1)
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
            half _OutlineEnabled;
            half _OutlineWidth;
            half4 _OutlineColor;
            half4 _LiquidUp;
            half4 _LiquidOffset;
            half _EnableExternalAlpha;
        CBUFFER_END

        half SampleSpriteAlpha(half2 uv, half vertexAlpha)
        {
            half insideUv =
                step(0.0h, uv.x) * step(uv.x, 1.0h) *
                step(0.0h, uv.y) * step(uv.y, 1.0h);
            half mainAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
            half externalAlpha = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, uv).r;
            return lerp(mainAlpha, externalAlpha, _EnableExternalAlpha) * vertexAlpha * insideUv;
        }

        Varyings Vert(Attributes input)
        {
            Varyings output = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            UNITY_SKINNED_VERTEX_COMPUTE(input);
            SetUpSpriteInstanceProperties();
            input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

            half outlineUv = max(_OutlineWidth, 0.0h) * 0.01h * step(0.5h, _OutlineEnabled);
            half expandFactor = 1.0h + outlineUv * 2.0h;
            input.positionOS.xy *= expandFactor;

            output.positionCS = TransformObjectToHClip(input.positionOS);
#if defined(DEBUG_DISPLAY)
            output.positionWS = TransformObjectToWorld(input.positionOS);
            output.normalWS = TransformObjectToWorldNormal(input.normal);
#endif
            output.uv = (input.uv - half2(0.5h, 0.5h)) * expandFactor + half2(0.5h, 0.5h);
            output.color = input.color * _Color * unity_SpriteColor;
            return output;
        }

        half4 Frag(Varyings input) : SV_Target
        {
            half4 sprite = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
            sprite.a = SampleSpriteAlpha(input.uv, input.color.a);

            half2 centered = input.uv - half2(0.5h, 0.5h);
            half2 liquidCentered = centered - _LiquidOffset.xy;
            half upLengthSquared = max(dot(_LiquidUp.xy, _LiquidUp.xy), 0.001h);
            half2 up = _LiquidUp.xy * rsqrt(upLengthSquared);

            const half surfaceSoftness = 0.02h;
            const half edgeSoftness = 0.018h;

            // ✅ FIX: expandFactor fragment'ta da hesaplanıyor
            half outlineUv = max(_OutlineWidth, 0.0h) * 0.01h;
            half expandFactor = 1.0h + outlineUv * 2.0h;

            // ✅ FIX: Radius'lar expandFactor'a göre normalize ediliyor
            half liquidRadius = 0.47h / expandFactor;
            half spriteOuterRadius = 0.47h / expandFactor;

            half height = _FillAmount - 0.5h;
            half surface = height - dot(liquidCentered, up);
            half liquidMask = smoothstep(-surfaceSoftness, surfaceSoftness, surface);
            half radiusDistance = length(centered);
            half innerMask = 1.0h - smoothstep(liquidRadius - edgeSoftness, liquidRadius, radiusDistance);

            half outlineSoftness = max(outlineUv * 0.2h, 0.003h);
            half outlineMask =
                smoothstep(spriteOuterRadius, spriteOuterRadius + outlineSoftness, radiusDistance) *
                (1.0h - smoothstep(spriteOuterRadius + outlineUv - outlineSoftness, spriteOuterRadius + outlineUv, radiusDistance)) *
                step(0.001h, outlineUv) *
                step(0.5h, _OutlineEnabled);
            half visibleOutline = outlineMask * (1.0h - sprite.a);

            half visibleLiquid = liquidMask * innerMask;
            half surfaceGlow = 1.0h - smoothstep(0.0h, surfaceSoftness * 5.0h, abs(surface));
            half edgeGlow = smoothstep(liquidRadius - 0.12h, liquidRadius, radiusDistance);
            half glow = (surfaceGlow * 0.8h + edgeGlow * 0.3h + 0.12h) * visibleLiquid;

            half3 liquidRgb = _LiquidColor.rgb + _GlowColor.rgb * _GlowIntensity * glow;
            half3 finalRgb = lerp(liquidRgb, sprite.rgb, sprite.a);
            finalRgb = lerp(finalRgb, _OutlineColor.rgb, visibleOutline);
            half finalAlpha = max(sprite.a, _LiquidColor.a * visibleLiquid * input.color.a);
            finalAlpha = max(finalAlpha, visibleOutline * _OutlineColor.a * input.color.a);

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