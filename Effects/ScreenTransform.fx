//sampler uImage0 : register(s0);
//sampler uImage1 : register(s1);
//sampler uImage2 : register(s2);
//sampler uImage3 : register(s3);
//float3 uColor;
//float3 uSecondaryColor;
//float2 uScreenResolution;
//float2 uScreenPosition;
//float2 uTargetPosition;
//float2 uDirection;
//float uOpacity;
//float uTime;
//float uIntensity;
//float uProgress;
//float2 uImageSize1;
//float2 uImageSize2;
//float2 uImageSize3;
//float2 uImageOffset;
//float uSaturation;
//float4 uSourceRect;
//float2 uZoom;
//float4x4 TransformMatrix;
//bool useHeatMap;

//struct VSInput
//{
//	float2 Pos : POSITION0;
//	float4 Color : COLOR0;
//	float3 Texcoord : TEXCOORD0;
//};
//struct PSInput
//{
//	float4 Pos : SV_POSITION;
//	float4 Color : COLOR0;
//	float3 Texcoord : TEXCOORD0;
//};
//PSInput VertexShaderFunction(VSInput input)
//{
//	PSInput output;
//	output.Color = input.Color;
//	output.Texcoord = input.Texcoord;
//	output.Pos = float4(input.Pos, 0, 1);
//	return output;
//}

//float GetGrayValue(float3 color)
//{
//	float max = color.x;
//	float min = color.y;
//	if (min > max)
//	{
//		max = min;
//		min = color.x;
//	}
//	if (color.z > max)
//	{
//		max = color.z;
//	}
//	if (color.z < min)
//	{
//		min = color.z;
//	}
//	return (max + min) * 0.5;
//}

//float4 PixelShaderFunction(PSInput input) : COLOR0
//{
//	float2 coords = input.Texcoord.xy;
//	float4 homoCoord = mul(float4(coords, 1, 0), TransformMatrix);
//	if (homoCoord.z == 0)
//		return float4(0, 0, 0, 0);
//	float2 current = homoCoord.xy / homoCoord.z;
//	float4 result = float4(0, 0, 0, 0);
//	if (current.x == saturate(current.x) && current.y == saturate(current.y))
//	{
//		result = tex2D(uImage0, current);
//	}
//	if (useHeatMap)
//	{
//		return tex2D(uImage1, float2(GetGrayValue(result.xyz), coords.y));
//	}
//	return result;
//}

//technique Technique1
//{
//	pass ScreenTransform
//	{
//		VertexShader = compile vs_2_0 VertexShaderFunction();
//		PixelShader = compile ps_2_0 PixelShaderFunction();
//	}
//}
sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;
float4x4 TransformMatrix;
bool useHeatMap;

float2 width;
float2 offset;

struct PSInput
{
	float4 Pos : SV_POSITION;
	float4 Color : COLOR0;
	float3 Texcoord : TEXCOORD0;
};

float GetGrayValue(float3 color)
{
	float max = color.x;
	float min = color.y;
	if (min > max)
	{
		max = min;
		min = color.x;
	}
	if (color.z > max)
	{
		max = color.z;
	}
	if (color.z < min)
	{
		min = color.z;
	}
	return (max + min) * 0.5;
}

float4 PixelShaderFunction_Single(PSInput input) : COLOR0
{
	float2 modifier = float2(uScreenResolution.x / uScreenResolution.y, 1);
	float2 coords = input.Texcoord.xy;
	float4 homoCoord = mul(float4((coords - float2(0.5, 0.5)) * modifier, 0, 1), TransformMatrix);
	if (homoCoord.w == 0)
		return float4(0, 0, 0, 0);
	float2 current = homoCoord.xy / homoCoord.w / modifier + float2(0.5, 0.5);
	float4 result = float4(0, 0, 0, 0);
	if (current.x == saturate(current.x) && current.y == saturate(current.y))//
	{
		result = tex2D(uImage0, current);
	}
	if (useHeatMap)
	{
		return tex2D(uImage1, float2(GetGrayValue(result.xyz), coords.y));
	}
	return result;
}
float4 PixelShaderFunction_Wrap(PSInput input) : COLOR0
{
	float2 modifier = float2(uScreenResolution.x / uScreenResolution.y, 1);
	float2 coords = input.Texcoord.xy;
	float4 homoCoord = mul(float4((coords - float2(0.5, 0.5)) * modifier, 0, 1), TransformMatrix);
	if (homoCoord.w == 0)
		return float4(0, 0, 0, 0);
	float2 current = homoCoord.xy / homoCoord.w / modifier + float2(0.5, 0.5);
	float4 result = float4(0, 0, 0, 0);
	if (true)//current.x == saturate(current.x) && current.y == saturate(current.y)
	{
		result = tex2D(uImage0, current);
	}
	if (useHeatMap)
	{
		return tex2D(uImage1, float2(GetGrayValue(result.xyz), coords.y));
	}
	return result;
}
float4 PixelShaderFunction_ConicSection(PSInput input) : COLOR0
{
	float2 modifier = float2(uScreenResolution.x / uScreenResolution.y, 1);
	float2 coords = input.Texcoord.xy;
	float4 homoCoord = mul(float4((coords - float2(0.5, 0.5)) * modifier, 0, 1), TransformMatrix);
	if (homoCoord.w == 0)
		return float4(0, 0, 0, 0);
	float2 current = homoCoord.xy / homoCoord.w / modifier + float2(0.5, 0.5);
	float4 result = float4(0, 0, 0, 0);
	if (true)//current.x == saturate(current.x) && current.y == saturate(current.y)
	{
		result += tex2D(uImage0, current);
	}
	float _length = length(homoCoord.xy / homoCoord.z + offset);
	if (_length == clamp(_length, width.x, width.y))
	{
		float factor = (_length - width.x) / (width.y - width.x);
		float angle = atan2(current.y, current.x);
		result += tex2D(uImage1, float2(angle / 3.1415 + 0.5 + uTime, factor));
	}
	if (useHeatMap)
	{
		return tex2D(uImage1, float2(GetGrayValue(result.xyz), coords.y));
	}
	return result;
}
float4 PixelShaderFunction_Test(PSInput input) : COLOR0
{
	float2 coords = input.Texcoord.xy;
	float4 result = float4(0, 0, 0, 0);
	if (true)//current.x == saturate(current.x) && current.y == saturate(current.y)
	{
		result += tex2D(uImage0, coords);
	}
	float _length = length(coords);
	
	if (width.x < _length && width.y > _length)
	{
		float factor = (_length - width.x) / (width.y - width.x);
		float angle = atan2(coords.y, coords.x);
		result += tex2D(uImage1, float2(angle / 3.1415 + 0.5, factor));
	    
	}
	//result += tex2D(uImage1, float2(saturate(factor), 0.5));
	return result;
}
float4 PixelShaderFunction_Simple(PSInput input) : COLOR0
{
	float2 coords = input.Texcoord.xy;
	float4 homoCoord = mul(float4(coords, 0, 1), TransformMatrix);
	if (homoCoord.w == 0)
		return float4(0, 0, 0, 0);
	float2 current = homoCoord.xy / homoCoord.w;
	float4 result = float4(0, 0, 0, 0);
	if (current.x == saturate(current.x) && current.y == saturate(current.y))//
	{
		result = tex2D(uImage0, current);
	}
	return result;
}
float4 PixelShaderFunction_Gradient(PSInput input) : COLOR0
{
	float2 coords = input.Texcoord.xy;
	float4 homoCoord = mul(float4(coords, 0, 1), TransformMatrix);
	float4 base = float4(0, 0, 0, 0); //tex2D(uImage1, coords);
	
	if (homoCoord.w == 0)
		return base;
	float2 current = homoCoord.xy / homoCoord.w;
	
	float4 result = tex2D(uImage0, current);
	float dist = length(floor(current));
	result = lerp(base, result, 1 / (dist + 1));
	return result;
}
technique Technique1
{
	pass Single
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Single();
	}
	pass Wrap
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Wrap();
	}
	pass ConicSection
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_ConicSection();
	}
	pass Test
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Test();
	}
	pass Simple
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Simple();
	}
	pass Gradient
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Gradient();

	}

}