sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float uTime;
float4x4 uTransform;
float4 uItemColor;
float4 uItemGlowColor;
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
float getValue(float4 c1)
{
	float maxValue = max(max(c1.x, c1.y), c1.z);
	float minValue = min(min(c1.x, c1.y), c1.z);
	return (maxValue + minValue) / 2;
}
float4 PSFunction_Bell(PSInput input) : COLOR0
{
	float2 coord = input.Texcoord.xy;
	float4 c = tex2D(uImage0, coord);
	if (!any(c))
		return 0;
	c = tex2D(uImage1, float2(0, uTime) + coord);
	c *= input.Texcoord.z;
	c *= input.Color;
	return c;
}
float4 PSFunction_Item(PSInput input) : COLOR0
{
	float2 coord = input.Texcoord.xy;
	float4 c = tex2D(uImage0, coord);
	if (!any(c))
		return 0;
	float4 c1 = tex2D(uImage1, float2(0, uTime) + coord);
	c1 *= input.Texcoord.z;
	c1 *= input.Color;
	c *= uItemColor;
	return c + c1;
}
float4 PSFunction_ItemAdditive(PSInput input) : COLOR0
{
	float2 coord = input.Texcoord.xy;
	float4 cItem = tex2D(uImage0, coord);
	float4 cItemGlow = tex2D(uImage3, coord);
	if (!any(cItem) && !any(cItemGlow))
		return 0;
	float4 c = tex2D(uImage1, float2(0, uTime) + coord);
	c += tex2D(uImage2, coord);
	c *= input.Texcoord.z;
	c *= input.Color;
	cItem *= uItemColor;
	cItemGlow *= uItemGlowColor;
	return cItem + cItemGlow + c;
}
float4 PSFunction_ItemGlow(PSInput input) : COLOR0
{
	float2 coord = input.Texcoord.xy;
	float4 cItem = tex2D(uImage0, coord);
	float4 cItemGlow = tex2D(uImage3, coord);
	if (!any(cItem) && !any(cItemGlow))
		return 0;
	float4 c = tex2D(uImage2, coord);
	c *= input.Texcoord.z;
	c *= input.Color;
	cItem *= uItemColor;
	cItemGlow *= uItemGlowColor;
	return cItem + cItemGlow + c;
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
	pass Bell
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PSFunction_Bell();
	}
	pass Item
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PSFunction_Item();
	}
	pass ItemAdditive
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PSFunction_ItemAdditive();
	}
	pass ItemGlow
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PSFunction_ItemGlow();
	}
}