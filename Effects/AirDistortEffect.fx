sampler uImage0 : register(s0); //背景
sampler uImage2 : register(s1); //高斯模糊贴图
sampler uImage3 : register(s2); //辅助噪音图?
texture2D tex0; //画到rendertarget2D上的内容
sampler2D uImage1 = sampler_state
{
	Texture = <tex0>;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};
//float gauss[7] = { 0.05, 0.1, 0.24, 0.4, 0.24, 0.1, 0.05 };//现在使用Misc[18]
float2 uScreenSize;
float strength;
float rotation;
float4 PSFunction_Distort(float2 coords : TEXCOORD0) : COLOR0
{
	float2 currentCoord = coords;
	float2x2 trans = float2x2(cos(rotation), -sin(rotation), cos(rotation), sin(rotation));
	for (int n = -3; n <= 3; n++)
	{
		for (int m = -3; m <= 3; m++)
		{
			float2 vec = float2(n, m);
			currentCoord += mul(vec, trans) * dot(tex2D(uImage1, coords + vec / uScreenSize).rgb, 0.333) * strength * tex2D(uImage2, (vec + float2(3, 3)) / 6).r;
		}
	}
	return tex2D(uImage0, currentCoord);
}
technique Technique1
{
	pass Distort
	{
		PixelShader = compile ps_3_0 PSFunction_Distort();
	}
}

