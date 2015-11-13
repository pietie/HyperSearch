//————————————————————————————–
//
// WPF ShaderEffect HLSL — DesaturateEffect
//
//————————————————————————————–
 
//—————————————————————————————–
// Shader constant register mappings (scalars – float, double, Point, Color, Point3D, etc.)
//—————————————————————————————–
 
/// <summary>The brightness offset.</summary>
/// <type>double</type>
/// <defaultValue>0</defaultValue>
float SaturationValue : register(c0);

//————————————————————————————–
// Sampler Inputs (Brushes, including ImplicitInput)
//————————————————————————————–
 
/// <summary>The implicit input sampler passed into the pixel shader by WPF.</summary>
/// <samplingMode>Auto</samplingMode>
sampler2D Input : register(s0);

//————————————————————————————–
// Pixel Shader
//————————————————————————————–
 
float4 main(float2 uv : TEXCOORD) : COLOR
{
    float3  LuminanceWeights = float3(0.299,0.587,0.114);
    float4 srcPixel = tex2D(Input, uv);
    float    luminance = dot(srcPixel,LuminanceWeights);
    float4    dstPixel = lerp(luminance,srcPixel,SaturationValue);
    //retain the incoming alpha
	dstPixel.a = srcPixel.a;
    return dstPixel;
}
