float4x4 uTransform;
struct VSInput
{
	float2 Pos : POSITION0;
	float3 Texcoord : TEXCOORD0;
};
struct PSInput
{
	float4 Pos : SV_POSITION;
	float3 Texcoord : TEXCOORD0;
};

PSInput VertexShaderFunction(VSInput input)
{
	PSInput output;
	output.Texcoord = input.Texcoord;
	output.Pos = mul(float4(input.Pos, 0, 1), uTransform);
	return output;
}
float4 uColor;
float2 uTransition;
float4 GetNoBorderColor(float distance)
{
	return lerp(uColor, 0, smoothstep(uTransition.x, uTransition.y, distance));
}

float4 RightBottom(PSInput input) : COLOR0
{
	float2 coord = input.Texcoord.xy;
	float distance = 1 - dot(coord, coord);
	return GetNoBorderColor(distance);
}


float4 LeftBottom(PSInput input) : COLOR0
{
	float2 coord = input.Texcoord.xy;
	coord.x = 1 - coord.x;
	float distance = 1 - dot(coord, coord);
	return GetNoBorderColor(distance);
}

technique Technique1
{
	pass RightBottom
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 RightBottom();
	}
	pass LeftBottom
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 LeftBottom();
	}
}