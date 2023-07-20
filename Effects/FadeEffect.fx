sampler uImage0 : register(s0); //主内容
sampler uImage1 : register(s1); //值偏移灰度图
float uOffset;

float4 PixelShaderFunction_Fade(float2 coord : TEXCOORD0,float4 color : COLOR0) : COLOR0
{
	float4 mainColor = tex2D(uImage0, coord)  * color; //读取数据
	mainColor.a *= saturate(tex2D(uImage1, coord) + lerp(-1, 1, uOffset));
	return mainColor;
}


technique Technique1
{
	pass Fade
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Fade();
	}
}