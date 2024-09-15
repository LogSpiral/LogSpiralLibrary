sampler uImage0 : register(s0);
//≤‚ ‘”√
float factor;
float uTimer;
float GetTargetR(float a)
{
	float r = (sin(a) + sin(2.0 * a) * .5) / 4.1 + 0.5; // - sin(3.0 * a)
	r = lerp(0.8, 1.0, r);
	return r * factor * .5;

}

float4 PixelShaderFunction_Ring(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float2 vec = coord - float2(.5, .5);
	float r = length(vec);
	float angle = atan2(vec.y, vec.x);
	float targetR = GetTargetR(angle * 4.0 + uTimer);
	float v = 1.2 - pow(r - targetR, 2.0) * 2048;
	v = saturate(v);
	return v * color;

}
technique Technique1
{
	pass Ring
	{
		PixelShader = compile ps_2_0 PixelShaderFunction_Ring();
	}
}


