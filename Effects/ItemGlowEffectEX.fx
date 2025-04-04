sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float uTime;
float4x4 uTransform;
float4 uItemColor;
float4 uItemGlowColor;
float4 uItemFrame = float4(0, 0, 1, 1);
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
float4 PSFunction(PSInput input) : COLOR0
{
	float2 coord = input.Texcoord.xy;
	float4 cItem = tex2D(uImage0, coord);
	float4 cItemGlow = tex2D(uImage3, coord);
	//if (dot(cItem + cItemGlow, float4(1,1,1,1)) < 0.01)
	//	return float4(0,0,0,0);
	float2 lightCoord = (coord - uItemFrame.xy) / (uItemFrame.zw - uItemFrame.xy);
	float4 c = tex2D(uImage1, lightCoord + float2(0,uTime));
	c += tex2D(uImage2, lightCoord);
	c *= input.Color;
	cItem *= uItemColor;
	cItemGlow *= uItemGlowColor;
	float4 result = cItem + cItemGlow + c;
	result.a = input.Texcoord.z;
	return result * sign((cItem + cItemGlow).x);//OpenGL下用if提前返回空白像素有时候会炸，不知道为什么-2025.3.28
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
	pass ItemGlow
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PSFunction();
	}
}