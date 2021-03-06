//覧覧覧覧覧覧覧覧覧覧覧覧覧覧�
//
// WPF ShaderEffect HLSL � DesaturateEffect
//
//覧覧覧覧覧覧覧覧覧覧覧覧覧覧�
 
//覧覧覧覧覧覧覧覧覧覧覧覧覧覧蘭
// Shader constant register mappings (scalars � float, double, Point, Color, Point3D, etc.)
//覧覧覧覧覧覧覧覧覧覧覧覧覧覧蘭
 
/// <summary>The brightness offset.</summary>
/// <type>double</type>
/// <defaultValue>0</defaultValue>
float SaturationValue : register(c0);

//覧覧覧覧覧覧覧覧覧覧覧覧覧覧�
// Sampler Inputs (Brushes, including ImplicitInput)
//覧覧覧覧覧覧覧覧覧覧覧覧覧覧�
 
/// <summary>The implicit input sampler passed into the pixel shader by WPF.</summary>
/// <samplingMode>Auto</samplingMode>
sampler2D Input : register(s0);

//覧覧覧覧覧覧覧覧覧覧覧覧覧覧�
// Pixel Shader
//覧覧覧覧覧覧覧覧覧覧覧覧覧覧�
 
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
