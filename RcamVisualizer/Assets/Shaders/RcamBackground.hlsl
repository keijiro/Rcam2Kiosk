#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

#if defined(RCAM_FX0)
#include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise2D.hlsl"
#elif defined(RCAM_FX1)
#include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise3D.hlsl"
#endif

sampler2D _ColorTexture;
sampler2D _DepthTexture;
float4 _ProjectionVector;
float4x4 _InverseViewMatrix;

float2 _Opacity;
float2 _Direction;
float3 _EffectParams;

// Linear distance to Z depth
float DistanceToDepth(float d)
{
    float4 cp = mul(UNITY_MATRIX_P, float4(0, 0, -d, 1));
    return cp.z / cp.w;
}

// Inverse projection into the world space
float3 DistanceToWorldPosition(float2 uv, float d)
{
    float3 p = float3((uv - 0.5) * 2, -1);
    p.xy += _ProjectionVector.xy;
    p.xy /= _ProjectionVector.zw;
    return mul(_InverseViewMatrix, float4(p * d, 1)).xyz;
}

// Per-pixel effects
float3 PixelEffect(float3 wpos, float3 rgb, float luma)
{
#if defined(RCAM_NOFX)

    return rgb;

#endif

#if defined(RCAM_FX0)

    // FX0: Zebra

    // Noise field sample positions
    float2 np1 = float2(wpos.y * 10 - _Time.y *  2, 0.1);
    float2 np2 = float2(wpos.y * 40 - _Time.y * 10, 0.2);

    // Potential value
    float pt = (luma - 0.5) * 0.2 + SimplexNoise(np1) + SimplexNoise(np2) / 4;

    // Threshold value
    float thresh = _EffectParams.x;

    // Grayscale
    float gray = 1 - smoothstep(thresh - 0.4, thresh, abs(pt));

    // Output
    return gray * 4;

#endif

#if defined(RCAM_FX1)

    // FX1: Aura

    // Noise field sample position
    float3 np1 = wpos * 0.9 + float3( 0.12, -0.76, 0.03) * _Time.y;
    float3 np2 = wpos * 1.4 + float3(-0.01, -0.44, 0.04) * _Time.y;
    np2.y += (luma - 0.5) * 0.1;

    // Noise sample
    float n1 = SimplexNoise(np1);
    float4 n2 = SimplexNoiseGrad(np2);

    // Gradient
    const float3 A = 8;
    const float3 B = float3(0.1, 0.2, 0.2) * n2.xyz;
    const float3 C = float3(0.8, 0.5, 0.5);
    float3 srgb = sin(A * n1) * B + C;

    // Secondary gradient
    srgb = lerp(srgb, 1 - srgb, smoothstep(-0.04, 0.04, n2));

    // Highlight
    float hi = smoothstep(0.5, 0, abs(n2));

    return FastSRGBToLinear(srgb) * (1 + hi * 20);

#endif

#if defined(RCAM_FX2)

    // FX2: Slicer

    // Slice frequency (1/height)
    float freq = 60;

    // Per-slice random seed
    uint seed1 = floor(wpos.y * freq + 200 + _Time.y) * 2;

    // Random slice width
    float width = lerp(2, 5, Hash(seed1));

    // Random slice speed
    float speed = lerp(0.7, 4, Hash(seed1 + 1));

    // Effect direction
    float3 dir = float3(_Direction.x, 0, _Direction.y);

    // Potential value (scrolling strips)
    float pt = (dot(wpos, dir) + 100 + _Time.y * speed) * width;

    // Per-strip random seed
    uint seed2 = (uint)floor(pt) * 0x87893u;

    // Per-strip random color
    float hue = frac(_Time.y * 0.033 + Hash(seed2) * 0.3);
    float3 fill = FastSRGBToLinear(HsvToRgb(float3(hue, 1, 1)));

    // Threshold
    bool flag1 = _EffectParams.y > Hash(seed2 + 1);
    bool flag2 = _EffectParams.z > frac(pt);

    // Color blending
    rgb = flag1 && flag2 ? fill * 5 : rgb;

    // Output
    return rgb;

#endif
}

void FullScreenPass(Varyings varyings,
                    out float4 outColor : SV_Target,
                    out float outDepth : SV_Depth)
{
    // Calculate the UV coordinates from varyings
    float2 uv = (varyings.positionCS.xy + float2(0.5, 0.5)) * _ScreenSize.zw;

    // Color/depth samples
    float4 c = tex2D(_ColorTexture, uv);
    float d = tex2D(_DepthTexture, uv).x;

    // Inverse projection
    float3 p = DistanceToWorldPosition(uv, d);

    // Input pixel luma
    float lum = Luminance(FastLinearToSRGB(c.rgb));

    // Per-pixel effects
    float3 eff = PixelEffect(p, c.rgb, lum);

    // Fill mask
    bool mask = c.a < 0.5 ? _Opacity.x > 0 : _Opacity.y > 0;

    // Output
    outColor = float4(lerp(c.rgb, eff, c.a) * mask, c.a);
    outDepth = DistanceToDepth(d) * mask;
}
