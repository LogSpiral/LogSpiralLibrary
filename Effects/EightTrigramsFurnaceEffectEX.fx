#define DEFINEPASS(NAME) \
pass NAME \
{ \
		VertexShader = compile vs_3_0 VertexShaderFunction(); \
		PixelShader = compile  ps_3_0 PixelShaderFunction_##NAME(); \
}
#define DEFINEALLPASS(NAME)\
DEFINEPASS(NAME)\
DEFINEPASS(NAME##_Round)

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float4x4 uTransform;
float4 uTime;
float3 uAlphaVector;
float uMaxFactor;

struct VSInput
{
	float2 Pos : POSITION0;
	float4 Color : COLOR0;
	float3 Texcoord : TEXCOORD0;
};
struct PSInput
{
	float4 Pos : SV_POSITION;
	float4 Color : COLOR0;
	float3 Texcoord : TEXCOORD0;
};

float GetLerpValue(float from, float to, float v)
{
	return (v - from) / (to - from);
}

float GetHeight_Ellipse(float2 coord)
{
	float d = coord.x / uMaxFactor;
	d = sqrt(1.0 - d * d) * .5;
	return GetLerpValue(.5 - d, .5 + d, coord.y);
}
float GetHeight_Quadratic(float2 coord)
{
	float d = sqrt(coord.x / uMaxFactor) * .5;
	return GetLerpValue(.5 - d, .5 + d, coord.y);
}

bool InvertSaturateCheck(float v)
{
	return v * (1 - v) < 0;
}

float RoundTransform(float y)
{
	return asin(2 * y - 1) / 3.1415926 + .5;
}

float4 Coord2Color(float x, float y)
{
	float value = (tex2D(uImage0, float2(x + uTime.x, y)) + tex2D(uImage1, float2(x + uTime.x, y))).x;
	
	float4 mainColor = tex2D(uImage2, float2(uTime.y, 0.5));
	
	float4 barColor = tex2D(uImage2, float2(x + uTime.z, 0.5));
	
	float4 heatColor = tex2D(uImage3, float2(value + uTime.w, 0.5));
	
	float3x4 colorMatrix = float3x4(mainColor, barColor, heatColor);
	
	float4 result = mul(uAlphaVector, colorMatrix);
	result.a = value;
	
	return result;
}
float4 Coord2Color_Round(float x, float y)
{
	float value = tex2D(uImage0, float2(x + uTime.x, y)).x;

	if (abs(y - .5) < .125)
		value += tex2D(uImage1, float2(x + uTime.x * .0125, RoundTransform(y * 4 - 1.5) + sqrt(x) * 2.5 + uTime.x)).x;
	
	float4 mainColor = tex2D(uImage2, float2(uTime.y, 0.5));
	
	float4 barColor = tex2D(uImage2, float2(x + uTime.z, 0.5));
	
	float4 heatColor = tex2D(uImage3, float2(value + uTime.w, 0.5));
	
	float3x4 colorMatrix = float3x4(mainColor, barColor, heatColor);
	
	float4 result = mul(uAlphaVector, colorMatrix);
	
	if (value < .5)
		result = lerp(float4(0, 0, 0, 0), result, value * 2);
	else
		result = lerp(result, float4(1, 1, 1, 1), value * 2 - 1);
		//result.a = value;
	
	return result;
}
float4 PixelShaderFunction_Line(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	return Coord2Color(coord.x, coord.y);
}


float4 PixelShaderFunction_Quadratic(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float y = coord.x < uMaxFactor ? GetHeight_Quadratic(coord) : coord.y;
	if (InvertSaturateCheck(y))
		return float4(0.0, 0.0, 0.0, 0.0);
	return Coord2Color(coord.x, y);
}

float4 PixelShaderFunction_Ellipse(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float y = coord.x < uMaxFactor ? GetHeight_Quadratic(coord) : coord.y;
	if (InvertSaturateCheck(y))
		return float4(0.0, 0.0, 0.0, 0.0);
	return Coord2Color(coord.x, y);
}

float4 PixelShaderFunction_Line_Round(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	return Coord2Color_Round(coord.x, coord.y);
}


float4 PixelShaderFunction_Quadratic_Round(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float y = coord.x < uMaxFactor ? GetHeight_Quadratic(coord) : coord.y;
	if (InvertSaturateCheck(y))
		return float4(0.0, 0.0, 0.0, 0.0);
	return Coord2Color_Round(coord.x, y);

}

float4 PixelShaderFunction_Ellipse_Round(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float y = coord.x < uMaxFactor ? GetHeight_Quadratic(coord) : coord.y;
	if (InvertSaturateCheck(y))
		return float4(0.0, 0.0, 0.0, 0.0);
	return Coord2Color_Round(coord.x, y);

}


PSInput VertexShaderFunction(VSInput input)
{
	PSInput output;
	output.Color = input.Color;
	output.Texcoord = input.Texcoord;
	output.Pos = mul(float4(input.Pos, 0, 1), uTransform);
	return output;
}


technique Technique1
{
	DEFINEALLPASS(Line)
	DEFINEALLPASS(Quadratic)
	DEFINEALLPASS(Ellipse)
}