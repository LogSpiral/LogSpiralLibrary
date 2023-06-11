sampler uImage0 : register(s0); //迭代数据图 xy记录实部和虚部，[0,1]映射到[-1,1] z记录迭代次数 a暂时没软用
sampler uImage1 : register(s1); //颜色图,由z作为横坐标决定最终颜色
float4 uRange = float4(-2, -2, 2, 2); //区域，前两位是左下角，后两位是右上角
float2 uM;
float GetLerpValue(float from, float to, float value)
{
	return (value - from) / (to - from);
}
float2 GetLerpValue(float2 from, float2 to, float2 value)
{
	return float2(GetLerpValue(from.x, to.x, value.x), GetLerpValue(from.y, to.y, value.y));

}

float4 PixelShaderFunction_Fractal(float2 coord : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(uImage0, coord); //读取数据
	if (color.z >= 1)//已经迭代得够多了
		return color; //润
	float2 z = lerp(float2(-2, -2), float2(2, 2), color.xy); //读取数据获取z，因为这种尴尬的方式所以我不得不把颜色的初始值设置为(0.5,0.5,0)
	float2 z0 = lerp(uRange.xy, uRange.zw, coord); //通过插值获取z0
	int n;
	for (n = 0; n < 30; n++)
	{
		//经典曼德勃特迭代
		//z = float2(z.x * z.x - z.y * z.y, 2 * z.x * z.y) + z0;
		//if (length(z) > 2 || color.z + 0.03 * n >= 1)
		//	break;
		
		//玫瑰分形！z^2 - z + z0
		//z = float2(z.x * z.x - z.y * z.y - z.x, 2 * z.x * z.y - z.y) + z0; 
		//if (length(z) > 2 || color.z + 0.03 * n >= 1)
		//	break;
		
		//翅膀分形！
		//z = float2(z.x * z.x - z.y * z.y, 2 * z.x * z.y) + z0 - exp(z.x) * float2(cos(z.y), sin(z.y));
		//if (length(z) > 64 || color.z + 0.001 * n >= 1)
		//	break;
		
		//二次分形！  z^2 + uM * z + z0
		z = float2(z.x * (z.x + uM.x) - z.y * (z.y + uM.y), z.x * (z.y + uM.y) + z.y * (z.x + uM.x)) + z0;
		if (length(z) > 2 || color.z + 0.03 * n >= 1)
			break;
	}
	return float4(GetLerpValue(float2(-2, -2), float2(2, 2), z), color.z + 0.03 * n, 1); //迭代完成塞回数据

}
float4 PixelShaderFunction_HeatMap(float2 coord : TEXCOORD0) : COLOR0
{
	return tex2D(uImage1, float2(tex2D(uImage0, coord).z, 0.5));

}


technique Technique1
{
	pass Fractal
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Fractal();
	}
	pass HeatMap
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_HeatMap();
	}
}