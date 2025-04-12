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
float2 ComplexMul(float2 z1, float2 z2)
{
	return float2(z1.x * z2.x - z1.y * z2.y, z1.x * z2.y + z1.y * z2.x);
}
float2 ComplexLn(float2 z)
{
	return float2(log(z.x), atan2(z.y, z.x));
}

float2 ComplexPow(float2 z1, float2 z2)
{
	return exp(ComplexMul(ComplexLn(z1),z2));
}
float2 ComplexEXP(float2 z)
{
	return exp(z.x) * float2(cos(z.y), sin(z.y));

}
float2 ComplexSinh(float2 z)
{
	return (ComplexEXP(z) - ComplexEXP(-z)) * .5;
}
float2 ComplexCosh(float2 z)
{
	return (ComplexEXP(z) + ComplexEXP(-z)) * .5;
}
float2 ComplexSin(float2 z)
{
	z = float2(-z.y, z.x);
	z = ComplexSinh(z);
	z = float2(z.y, -z.x);
	return z;
}
float2 ComplexCos(float2 z)
{
	z = float2(-z.y, z.x);
	z = ComplexCosh(z);
	return z;
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
		//z = ComplexMul(z, z) + z0;
		//if (length(z) > 2 || color.z + 0.03 * n >= 1)
		//	break;
		
		//玫瑰分形！z^2 - z + z0
		//z = ComplexMul(z, z) - z + z0;
		//if (length(z) > 2 || color.z + 0.03 * n >= 1)
		//	break;
		
		//翅膀分形！
		//z = ComplexMul(z,z) + z0 - exp(z.x) * float2(cos(z.y), sin(z.y));
		//if (length(z) > 64 || color.z + 0.001 * n >= 1)
		//	break;
		
		//二次分形！ z ^ 2 + uM * z + z0
		//z = ComplexMul(z, z + uM) + z0;
		//if (length(z) > 2 || color.z + 0.03 * n >= 1)
		//	break;
		
		//二次分形2！  z^2 * uM  + z0
		//z = ComplexMul(ComplexMul(z, z), uM) + z0;
		//if (length(z) > 2 || color.z + 0.03 * n >= 1)
		//	break;
		
		z = ComplexMul(z - uM, ComplexMul(z, z)) + z0;
		if (length(z) > 2 || color.z + 0.03 * n >= 1)
			break;
	}
	return float4(GetLerpValue(float2(-2, -2), float2(2, 2), z), color.z + 0.03 * n, 1); //迭代完成塞回数据

}

//float2 uConst;
float4 PixelShaderFunction_JuilaSetCos(float2 coord : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(uImage0, coord); //读取数据
	if (color.z >= 1)//已经迭代得够多了
		return color; //润
	float2 z = lerp(float2(-2, -2), float2(2, 2), color.xy); //读取数据获取z，因为这种尴尬的方式所以我不得不把颜色的初始值设置为(0.5,0.5,0)
	if (!any(z))
		z = lerp(uRange.xy, uRange.zw, coord);
	//float2 z0 = lerp(uRange.xy, uRange.zw, coord); //通过插值获取z0
		int n;
	for (n = 0; n < 30; n++)
	{
		z = ComplexCos(z) + uM;
		
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
	pass JuilaSetCos
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_JuilaSetCos();
		
	}
}