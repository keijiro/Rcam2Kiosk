#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
#include "Packages/jp.keijiro.klak.lineargradient/Shader/LinearGradient.hlsl"

LinearGradient _BackGradient;
LinearGradient _FrontGradient;

float4 _EdgeColor;
float _EdgeThreshold;
float _EdgeContrast;
float _Dithering;
float2 _Opacity;

float4 SampleColorSRGB(uint2 pcs)
{
    float4 cs = LOAD_TEXTURE2D_X_LOD(_ColorPyramidTexture, pcs, 0);
    return FastLinearToSRGB(cs);
}

float4 FullScreenPass(Varyings varyings) : SV_Target
{
    // Clip space position
    uint2 pcs = varyings.positionCS.xy;
    uint2 pcsm4 = pcs % 4;

    // Color samples in sRGB
    float4 c0 = SampleColorSRGB(pcs + uint2(0, 0));
    float4 c1 = SampleColorSRGB(pcs + uint2(1, 1));
    float4 c2 = SampleColorSRGB(pcs + uint2(1, 0));
    float4 c3 = SampleColorSRGB(pcs + uint2(0, 1));

    // Edge detection (Roberts cross operator)
    float3 g1 = (c1.rgb) - (c0.rgb);
    float3 g2 = (c3.rgb) - (c2.rgb);
    float g = sqrt(dot(g1, g1) + dot(g2, g2));
    g = saturate((g - _EdgeThreshold) * _EdgeContrast);

    // Bayer dither matrix
    const float4x4 bayer =
      float4x4(0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5) / 16;

    // Luminance + dithering
    float dither = (bayer[pcsm4.x][pcsm4.y] - 0.5) * _Dithering;
    float lm = Luminance(c0.rgb) + dither;

    // Back/front gradients
    float4 bg = float4(SampleLinearGradientColor(_BackGradient, lm), _Opacity.x);
    float4 fg = float4(SampleLinearGradientColor(_FrontGradient, lm), _Opacity.y);

    // Edge blending (only affects the front)
    fg.rgb = lerp(fg.rgb, _EdgeColor.rgb, g);

    return FastSRGBToLinear(lerp(bg, fg, c0.a));
}
