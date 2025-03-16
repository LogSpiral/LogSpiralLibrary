sampler uImage0 : register(s0);
float4 PixelShaderFunction_Ring(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	return tex2D(uImage0,coord)  * color;
}
technique Technique1
{
	pass Default
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Ring();
	}
}

