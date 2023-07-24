sampler uImage0 : register(s0); //底层静态图
sampler uImage1 : register(s1); //偏移灰度图
sampler uImage2 : register(s2); //采样/着色图
float4x4 uTransform;
float uTimeX;
float uTimeY;
struct VSInput
{
	float4 Pos : POSITION0; //屑阿汪撅腚使用四维坐标！！
	float4 Color : COLOR0;
	float3 Texcoord : TEXCOORD0;
};
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
float4 GetGrayVector(float2 coord)
{
	return tex2D(uImage0, coord) * tex2D(uImage1, coord + float2(uTimeX, uTimeY));
}
float Gray(float2 coord)
{
	return GetGrayValue(GetGrayVector(coord).xyz);
}
float4 FinalModify(float4 color, float k)
{
	if (!any(color))
		return float4(0, 0, 0, 0);
	if (k < 0.5)
	{
		return float4(lerp(float3(0, 0, 0), color.xyz, 2 * k), color.w);
	}
	return float4(lerp(color.xyz, float3(1, 1, 1), 2 * k - 1), color.w);
}

float4 PixelShaderFunction_VertexColor(PSInput input) : COLOR0
{
	float4 color = input.Color * input.Texcoord.z;
	return FinalModify(color, Gray(input.Texcoord.xy));
}
float4 PixelShaderFunction_HeatMap(PSInput input) : COLOR0
{
	float4 grayVector = GetGrayVector(input.Texcoord.xy);
	float4 color = tex2D(uImage2, grayVector.xy);
	return float4(FinalModify(color, grayVector.x).xyz, color.w * input.Texcoord.z);
}
float4 PixelShaderFunction_ColorMap(PSInput input) : COLOR0
{
	float4 color = tex2D(uImage2, input.Texcoord.xy) * input.Texcoord.z;
	return FinalModify(color, Gray(input.Texcoord.xy));
}
float4 PixelShaderFunction_OriginColor(PSInput input) : COLOR0
{
	return tex2D(uImage0, input.Texcoord.xy) * input.Texcoord.z * input.Color;
}
float4 PixelShaderFunction_OriginColorAddVertexColor(PSInput input) : COLOR0
{
	float4 color = input.Color * input.Texcoord.z;
	return tex2D(uImage0, input.Texcoord.xy) * input.Texcoord.z * input.Color + FinalModify(color, Gray(input.Texcoord.xy));
}
float4 PixelShaderFunction_OriginColorAddHeatMap(PSInput input) : COLOR0
{
	float4 color = tex2D(uImage2, float2(GetGrayVector(input.Texcoord.xy).xy));
	return tex2D(uImage0, input.Texcoord.xy) * input.Texcoord.z * input.Color + FinalModify(color, input.Texcoord.z);
}
float4 PixelShaderFunction_OriginColorAddColorMap(PSInput input) : COLOR0
{
	float4 color = tex2D(uImage2, input.Texcoord.xy) * input.Texcoord.z;
	return tex2D(uImage0, input.Texcoord.xy) * input.Texcoord.z * input.Color + FinalModify(color, Gray(input.Texcoord.xy));
}
PSInput VertexShaderFunction(VSInput input)
{
	PSInput output;
	output.Color = input.Color;
	output.Texcoord = input.Texcoord;
	output.Pos = mul(input.Pos, uTransform);
	return output;
}


technique Technique1
{
	pass VertexColor // 使用采样图，颜色由传入颜色决定，比较单调
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_VertexColor();
	}
	pass HeatMap // 使用采样图，颜色由两份灰度图的叠加进行hsl插值决定
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_HeatMap();
	}
	pass ColorMap //使用采样图，颜色由纹理坐标决定
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_ColorMap();
	}
	pass OriginColor // 最简单的模式，单纯的图元变换，颜色乘上传入顶点的
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_OriginColor();
	}
	pass OriginColorAddVertexColor // 原来的图图附加上一层顶点色，我会有用到这个模式的一天吗
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_OriginColorAddVertexColor();
	}
	pass OriginColorAddHeatMap // 原来的图图附加上一层热度色，我会有用到这个模式的一天吗
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_OriginColorAddHeatMap();
	}
	pass OriginColorAddColorMap // 原来的图图附加上一层坐标色，我会有用到这个模式的一天吗
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_OriginColorAddColorMap();
	}
}