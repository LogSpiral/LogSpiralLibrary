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
float2x2 rotation;
float colorOffset; //是否启用色差效果，0为不启用，直接以扭曲后坐标作为结果
//遍历周围像素然后求和得到最终方向
//寄蒜亮巨大，不推荐使用
float4 ColorOffset(float2 origin, float2 offset)
{
	//return 
	//tex2D(uImage0, currentCoord) * lerp(float4(1, 1, 1, 1), float4(1, 0, 0, 1), colorOffset) + 
	//tex2D(uImage0, coords) * lerp(float4(0, 0, 0, 0), float4(0, 1, 0, 1), colorOffset) +
	//tex2D(uImage0, 2 * coords - currentCoord) * lerp(float4(0, 0, 0, 0), float4(0, 0, 1, 1), colorOffset);
	
	/*float fac1 = colorOffset;
	float fac2 = 1 - fac1;
	return
		tex2D(uImage0, origin + offset) * float4(1, fac2, fac2, 1) +
		tex2D(uImage0, origin) * float4(0, fac1, 0, fac1) +
		tex2D(uImage0, origin - offset) * float4(0, 0, fac1, fac1);*/
	return
		tex2D(uImage0, origin + offset * (1 + colorOffset)) * float4(1, 0, 0, 1) +
		tex2D(uImage0, origin + offset) * float4(0, 1, 0, 1) +
		tex2D(uImage0, origin + offset * (1 - colorOffset)) * float4(0, 0, 1, 1);
}

float4 PSFunction_Distort_AVG(float2 coords : TEXCOORD0) : COLOR0
{
	float2 offset = 0;
	float2x2 trans = rotation;
	for (int n = -3; n <= 3; n++)
	{
		for (int m = -3; m <= 3; m++)
		{
			float2 vec = float2(n, m);
			if (any(vec))
			{
				offset += mul(vec / length(vec), trans) * dot(tex2D(uImage1, coords + vec / uScreenSize).rgb, 0.333) * tex2D(uImage2, (vec + float2(3, 3)) / 6).r;
				
			}
		}
	}
	offset *= strength;
	offset /= uScreenSize;
	
	//if (colorOffset != 0)
	//{
	//	return ColorOffset(coords, offset);
	//}
	return ColorOffset(coords, offset);
}
float4 PSFunction_Distort_AVG_Weak(float2 coords : TEXCOORD0) : COLOR0
{
	float2 offset = 0;
	float2x2 trans = rotation;
	for (int n = -2; n <= 2; n++)
	{
		for (int m = -2; m <= 2; m++)
		{
			float2 vec = float2(n, m);
			if (any(vec))
			{
				offset += mul(vec / length(vec), trans) * dot(tex2D(uImage1, coords + vec / uScreenSize).rgb, 0.333) * tex2D(uImage2, float2(n + 2, m + 2) / 4).r;
				
			}
		}
	}
	offset *= strength;
	offset /= uScreenSize;
	
	//if (colorOffset != 0)
	//{
	//	return ColorOffset(coords, offset);
	//}
	return ColorOffset(coords, offset);
}
float4 PSFunction_Distort_AVG_Weakest(float2 coords : TEXCOORD0) : COLOR0
{
	float2 offset = 0;
	float2x2 trans = rotation;
	for (int n = -1; n <= 1; n++)
	{
		for (int m = -1; m <= 1; m++)
		{
			float2 vec = float2(n, m);
			if (any(vec))
			{
				offset += mul(vec / length(vec), trans) * dot(tex2D(uImage1, coords + vec / uScreenSize).rgb, 0.333) * tex2D(uImage2, (vec + float2(1, 1)) / 2).r;
			}
		}
	}
	offset *= strength;
	offset /= uScreenSize;
	//if (colorOffset != 0)
	//{
	//	return ColorOffset(coords, offset);
	//}
	return ColorOffset(coords, offset);
}
//法线贴图法
float4 PSFunction_Distort_Normal(float2 coords : TEXCOORD0) : COLOR0
{
	float2 offset = tex2D(uImage1, coords).yz;
	offset *= 2;
	offset -= float2(1, 1);
	float2x2 trans = rotation;
	offset = mul(offset, trans) / uScreenSize;
	return ColorOffset(coords, offset);
}
technique Technique1
{
	pass Distort_AVG
	{
		PixelShader = compile ps_3_0 PSFunction_Distort_AVG();
	}
	pass Distort_AVG_Weak
	{
		PixelShader = compile ps_3_0 PSFunction_Distort_AVG_Weak();
	}
	pass Distort_AVG_Weakest
	{
		PixelShader = compile ps_3_0 PSFunction_Distort_AVG_Weakest();
	}
	pass Distort_Normal
	{
		PixelShader = compile ps_3_0 PSFunction_Distort_Normal();
	}
}

