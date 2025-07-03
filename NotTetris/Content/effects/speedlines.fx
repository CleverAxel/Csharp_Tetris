// SpeedLines.fx

// === TEXTURE ===
texture MainTex;

sampler2D MainSampler = sampler_state
{
    Texture = <MainTex>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

// === PARAMETERS ===
float2 Resolution;
float Time;

float SpeedLinesRadialScale;
float SpeedLinesTiling;
float SpeedLinesAnimation;
float SpeedLinesPower;
float SpeedLinesRemap;

float MaskScale;
float MaskHardness;
float MaskPower;

float4 Colour;

// === NOISE ===
float2 mod289(float2 x) { return x - floor(x / 289.0) * 289.0; }
float3 mod289_3(float3 x) { return x - floor(x / 289.0) * 289.0; }
float3 permute(float3 x) { return mod289_3(((x * 34.0) + 1.0) * x); }

float snoise(float2 v)
{
    float4 C = float4(0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439);
    float2 i = floor(v + dot(v, C.yy));
    float2 x0 = v - i + dot(i, C.xx);

    float2 i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
    float4 x12 = x0.xyxy + C.xxzz;
    x12.xy -= i1;

    i = mod289(i);
    float3 p = permute(permute(i.y + float3(0.0, i1.y, 1.0)) + i.x + float3(0.0, i1.x, 1.0));

    float3 m = max(0.5 - float3(dot(x0, x0), dot(x12.xy, x12.xy), dot(x12.zw, x12.zw)), 0.0);
    m *= m * m * m;

    float3 x = 2.0 * frac(p * C.www) - 1.0;
    float3 h = abs(x) - 0.5;
    float3 ox = floor(x + 0.5);
    float3 a0 = x - ox;

    m *= 1.79284291400159 - 0.85373472095314 * (a0 * a0 + h * h);
    float3 g;
    g.x = a0.x * x0.x + h.x * x0.y;
    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
    return 130.0 * dot(m, g);
}

// === SHADERS ===
struct VertexInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

VertexOutput VS(VertexInput input)
{
    VertexOutput output;
    output.Position = input.Position;
    output.TexCoord = input.TexCoord;
    return output;
}

float4 PS(VertexOutput input) : COLOR0
{
    float2 uv = input.TexCoord;
    float4 sceneColor = tex2D(MainSampler, uv);

    // Centered UV for radial
    float2 centered = uv - float2(0.5, 0.5);
    float radius = length(centered);
    float angle = atan2(centered.x, centered.y);

    float2 radialUV;
    radialUV.x = radius * SpeedLinesRadialScale * 2.0;
    radialUV.y = angle / 6.2831853 * SpeedLinesTiling;

    // Animation
    radialUV += float2(-SpeedLinesAnimation * Time, 0.0);

    // Noise
    float noise = snoise(radialUV) * 0.5 + 0.5;

    // Safe pow
    float safeNoise = max(noise, 0.0001);
    float speedLines = saturate((pow(safeNoise, SpeedLinesPower) - SpeedLinesRemap) / (1.0 - SpeedLinesRemap));

    // Circular mask
    float2 maskUV = uv * 2.0 - 1.0;
    float dist = length(maskUV);
    float maskEdge = lerp(0.0, MaskScale, MaskHardness);
    float mask = pow(1.0 - saturate((dist - MaskScale) / (maskEdge - 0.001 - MaskScale)), MaskPower);

    // Final speedline intensity
    float finalMask = speedLines * mask;
    float3 colorRGB = Colour.rgb * finalMask;
    float alpha = Colour.a * finalMask;

    return lerp(sceneColor, float4(colorRGB, 1.0), alpha);
}

// === TECHNIQUE ===
technique SpeedLines
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_3_0 PS();
    }
}
