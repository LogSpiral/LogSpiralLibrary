sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);

float4x4 uTransform;
float uTime;
float uLighter; //已移除完毕
bool checkAir;
float airFactor;
bool gather;
float lightShift;
float2x2 heatRotation = float2x2(1, 0, 0, 1);
float distortScaler;
bool heatMapAlpha;
float alphaFactor;
float3 AlphaVector; //ultra版本新增变量，自己看下面的颜色矩阵√
bool normalize; //ultra版本新增变量，用于单位化系数向量
bool stab; //ultra版本新增变量，打造突刺的感觉
float4 uItemFrame = float4(0, 0, 1, 1); //ultra版本新增变量，添加对多帧武器的支持√


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
float getLerpValue(float from, float to, float t, bool clamped = true)
{
	float result = (t - from) / (to - from);
	if (clamped)
		result = saturate(result);
	return result;
}
float csaturate(float v)
{
	return clamp(v, 0.005, 0.995);

}

float modifyY(float2 coord)
{
	float2 _coord = coord;
	if (stab)
	{
		float targetWidth = 0;
		float f = (1 - coord.x) * 8;
		if (f > 5.5)
			targetWidth = 2.6 + 52 / (5 * f - 60);
		else if (f > 3.5)
			targetWidth = 1;
		else if (f > 2)
			targetWidth = 2 / 3.0 + 5.0 / 6 / (f - 1);
		else if (f > 1)
			targetWidth = 0.5 + 1 / (3 - f);
		else
			targetWidth = f;
		targetWidth /= 3.0;
		_coord.y = getLerpValue(0.5 - targetWidth, 0.5 + targetWidth, coord.y);
		//for (int n = 0; n < 2; n++)
		//	_coord.y = smoothstep(0, 1, _coord.y);
		//_coord.y = 4 * pow(_coord.y - 0.5, 3.0) + 0.5;

	}
	float start = 0;
	float end = 1;
	if (gather)
	{
		start = _coord.x;
	}
	if (distortScaler > 0)
	{
		start /= distortScaler;
		//end是一个二次函数f
		//f(0)=f(1)=1 / d，f(0.5) = 1
		//这里d是扭曲缩放倍数，空气扭曲部分的绘制会比原来的宽d倍，
		//但是我希望它和正常绘制的两端能连上，于是就有了这个迫真插值
		end = (1 - (1 - 1 / distortScaler) * pow(2 * _coord.x - 1, 2));
	}
	return getLerpValue(start, end, _coord.y);
}

float4 weaponColor(float coordy)
{
	return tex2D(uImage2, lerp(float2(uItemFrame.x, uItemFrame.y + uItemFrame.w), float2(uItemFrame.x + uItemFrame.z, uItemFrame.y), coordy * airFactor));
}

float4 getBaseValue(float3 coord)
{
	float x = uTime + coord.x;
	float y = modifyY(coord.xy);
	//if (y > 1)
	//	return float4(coord.x, coord.y, 0, 1);
	if (y != csaturate(y))
		return float4(0, 0, 0, 0);
	float4 c1 = tex2D(uImage0, float2(coord.x, y));
	float4 c3 = tex2D(uImage1, float2(x, y));
	c1 *= c3;
	//return saturate(c1.a + lightShift);
	return saturate(c1 + lightShift);
}
float3 LightInterpolation(float3 c, float l)
{
	if (l >= 0.5)
	{
		return lerp(c, float3(1, 1, 1), 2 * l - 1);
	}
	else
	{
		return lerp(float3(0, 0, 0), c, 2 * l);
	}
}
float4 PixelShaderFunction_VertexColor(PSInput input) : COLOR0
{
	if (checkAir)
	{
		if (!any(weaponColor(input.Texcoord.y)))
			return float4(0, 0, 0, 0);
	}
	float color = getBaseValue(input.Texcoord).r;
	if (!any(color))
		return float4(0, 0, 0, 0);
	float alpha = input.Color.a;
	if (heatMapAlpha)
		alpha *= color * alphaFactor;
	return float4(input.Color.rgb, alpha);
}
float4 SampleFromHeatMap(float2 coord)
{
	//coord.x = saturate(coord.x);
	//coord.x *= 0.9999;
	coord.x = csaturate(coord.x);
	return tex2D(uImage3, coord);

}
float3 getMapColor(float3 texcoord)
{
	return SampleFromHeatMap(mul(float2(texcoord.x, modifyY(texcoord.xy)) - 0.5, heatRotation) + 0.5).xyz;
}

float4 PixelShaderFunction_MapColor(PSInput input) : COLOR0
{
	if (checkAir)
	{
		if (!any(weaponColor(input.Texcoord.y)))
			return float4(0, 0, 0, 0);
	}
	float color = getBaseValue(input.Texcoord).r;
	if (!any(color))
		return float4(0, 0, 0, 0);
	float alpha = input.Color.a;
	if (heatMapAlpha)
		alpha *= color * alphaFactor;
	return float4(getMapColor(input.Texcoord), alpha);
}
float4 PixelShaderFunction_WeaponColor(PSInput input) : COLOR0
{
	float3 coord = input.Texcoord;
	float4 c = weaponColor(coord.y);
	if (!any(c))
		return float4(0, 0, 0, 0);
	float color = getBaseValue(input.Texcoord).r;
	if (!any(color))
		return float4(0, 0, 0, 0);
	float alpha = input.Color.a;
	if (heatMapAlpha)
		alpha *= color * alphaFactor;
	return float4(c.rgb, alpha);
}
float4 PixelShaderFunction_HeatMap(PSInput input) : COLOR0
{
	if (checkAir)
	{
		if (!any(weaponColor(input.Texcoord.y)))
			return float4(0, 0, 0, 0);
	}
	float3 coord = input.Texcoord;
	float light = getBaseValue(coord).r;
	if (!any(light))
		return float4(0, 0, 0, 0);
	float4 c = SampleFromHeatMap(light);
	float alpha = input.Color.a;
	if (heatMapAlpha)
		alpha *= light * alphaFactor;
	return float4(c.rgb, alpha);
}
float4 PixelShaderFunction_BlendMW(PSInput input) : COLOR0
{
	float3 coord = input.Texcoord;
	float4 c = weaponColor(coord.y);
	if (!any(c))
		return float4(0, 0, 0, 0);
	float color = getBaseValue(coord).r;
	if (!any(color))
		return float4(0, 0, 0, 0);
	float alpha = input.Color.a;
	if (heatMapAlpha)
		alpha *= color * alphaFactor;
	return float4((c.rgb + getMapColor(coord)) * .5f, alpha);
}
float4 PixelShaderFunction_OriginColor(PSInput input) : COLOR0
{
	float3 coord = input.Texcoord;
	float4 c = weaponColor(coord.y);
	if (!any(c))
		return float4(0, 0, 0, 0);
	return getBaseValue(input.Texcoord) * input.Texcoord.z;
}
float4 PixelShaderFunction_VertexColor2(PSInput input) : COLOR0
{
	float3 coord = input.Texcoord;
	float4 c = weaponColor(coord.y);
	if (!any(c))
		return float4(0, 0, 0, 0);
	float greyValue = getBaseValue(input.Texcoord).r;
	float4 result = float4(LightInterpolation(input.Color.rgb, greyValue + lightShift) * input.Texcoord.z, input.Color.a);
	if (heatMapAlpha)
	{
		result.a *= greyValue * airFactor;
		result.a = saturate(result.a);
	}
	return result;
}
float alphaOffset;
float4 PixelShaderFunction_MapColor2(PSInput input) : COLOR0
{
	float a = 1 - saturate(1 - AlphaVector.x) * saturate(1 - AlphaVector.y) * saturate(1 - AlphaVector.z);
	if (a == 0)
		return float4(0, 0, 0, 0);
	
	float3 coord = input.Texcoord;
	float4 _weaponColor = weaponColor(coord.y);
	if (!any(_weaponColor))
		return float4(0, 0, 0, 0);
	float4 _mapColor = float4(getMapColor(coord), 1);
	//float4 mapColor = tex2D(uImage3, mul(float4(coord, 1) - float4(0.5, 0.5, 0, 0), heatRotation).xy + float2(0.5, 0.5));
	float greyValue = getBaseValue(input.Texcoord).r;
	if (!any(greyValue))
		return float4(0, 0, 0, 0);
	float4 _heatColor = SampleFromHeatMap(greyValue);
	float3x4 colorMatrix = float3x4(_mapColor, _weaponColor, _heatColor);
	float3 cVector = AlphaVector;
	if (normalize)
	{
		float d = dot(float3(1.0, 1.0, 1.0), AlphaVector);
		cVector = d == 0 ? float3(0.3333, 0.3333, 0.3333) : cVector / d;

	}
	float4 result = mul(AlphaVector, colorMatrix) * coord.z / a;

	result.a = input.Color.a * a;
	if (heatMapAlpha)
	{
		result.a *= greyValue * alphaFactor;
		result.a += alphaOffset;
		//result.a = saturate(result.a);
	}
	return result;
}
//生成法线图
//float4 PixelShaderFunction_Normal(PSInput input) : COLOR0
//{
//	float3 coord = input.Texcoord;
//	float4 _weaponColor = weaponColor(coord.y);
//	if (!any(_weaponColor))
//		return float4(0, 0, 0, 0);
//	float4 _mapColor = float4(tex2D(uImage3, mul(float2(input.Texcoord.x, modifyY(input.Texcoord.xy)), heatRotation)).xyz, 1);
//	//float4 mapColor = tex2D(uImage3, mul(float4(coord, 1) - float4(0.5, 0.5, 0, 0), heatRotation).xy + float2(0.5, 0.5));
//	float greyValue = getBaseValue(input.Texcoord).r;
//	float4 pos = input.texcoor
//	return DirectionToColor();
//}
//float4 DirectionToColor(float2 direction)
//{
//	direction += float2(1, 1);
//	direction *= .5f;
//	return float4(0, direction, 1);
//}

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
	pass VertexColor
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_VertexColor();
	}
	pass WeaponColor
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_WeaponColor();
	}
	pass HeatMap
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_HeatMap();
	}
	pass BlendMW
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_BlendMW();
	}
	pass MapColor
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_MapColor();
	}
	pass OriginColor // 单纯对两张图变换然后叠加，不使用采样图或者武器贴图
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_OriginColor();
	}
	pass VertexColor2 // 使用灰度图，颜色由传入颜色决定，渐变之类的还是用采样图代替吧
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_VertexColor2();
	}
	pass MapColor2 // 使用采样图和武器贴图，颜色由线性插值决定(a,b,c)*(v1,v2,v3),a+b+c=1那种
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_MapColor2();
	}
}


/*   ↓↓↓↓↓↓↓放弃的UL↓↓↓↓↓↓↓↓
sampler uImage0 : register(s0); //底层静态图
sampler uImage1 : register(s1); //偏移灰度图
sampler uImage2 : register(s2); //武器本体贴图
sampler uImage3 : register(s3); //采样/着色图
float4x4 uTransform; //世界变换矩阵√
float uTime; //时间偏移量√
float uLighter; //亮度偏移系数，废弃待删
bool checkAir; //检测空心√
float airFactor; //末端空心系数√
bool gather; //末端收束√
float lightShift; //亮度偏离量√
float4x4 heatRotation; //采样图变换矩阵√
float distortScaler; //扭曲缩放系数(?  这货还要回忆一下什么作用
bool heatMapAlpha; //是否用灰度值影响透明度√
float alphaFactor; //是否用灰度值影响透明度的系数√
float3 AlphaVector;//ultra版本新增变量，自己看下面的颜色矩阵√
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
float GetLerpValue(float from, float to, float t)
{
	if (from == to)
		return 0;
		return (t - from) / (to - from);
}
float4 GetBaseValue(float2 coord)
{
	float from = 0;
	if (gather)
		from = 1 - coord.x;
	float to = 1;
	if (distortScaler != 0)
		to = lerp(1, 1 / distortScaler, coord.x);
	coord.y = GetLerpValue(from, to, coord.y / airFactor);
	return tex2D(uImage0, coord) + tex2D(uImage1, coord + float2(uTime, 0));
}
float GetGreyValue(float4 color)
{
	float max = color.x;
	float min = color.y;
	if (min > max)
	{
		max = min;
		min = color.x;
	}
	if (color.z > max)
	{
		max = color.z;
	}
	if (color.z < min)
	{
		min = color.z;
	}
	return (max + min) * 0.5;
}
float GetGreyValue(float2 coord)
{
	return GetGreyValue(GetBaseValue(coord));
}
float4 LightInterpolation(float4 origin, float value)
{
	value = saturate(value);
	if (value < 0.5)
		return lerp(float4(0, 0, 0, origin.a), origin, value * 2);
	return lerp(origin, float4(1, 1, 1, origin.a), value * 2 - 1);
}
bool AirCheck(float y)
{
	return y / airFactor > 1 || !any(tex2D(uImage2, float2(1 - y, y)));
}

float4 PixelShaderFunction_OriginColor(PSInput input) : COLOR0
{
	if (checkAir && AirCheck(input.Texcoord.y))
		return float4(0, 0, 0, 0);
	return GetBaseValue(input.Texcoord.xy / float2(1, airFactor)) * input.Texcoord.z;
}
float4 PixelShaderFunction_VertexColor(PSInput input) : COLOR0
{
	if (checkAir && AirCheck(input.Texcoord.y))
		return float4(0, 0, 0, 0);
	
	float greyValue = GetGreyValue(input.Texcoord.xy / float2(1, airFactor));
	float4 result = LightInterpolation(input.Color, greyValue + lightShift) * input.Texcoord.z;
	if (heatMapAlpha)
	{
		result.a *= greyValue * airFactor;
		result.a = saturate(result.a);
	}
	return result;
}
float4 PixelShaderFunction_MapColor(PSInput input) : COLOR0
{
	if (checkAir && AirCheck(input.Texcoord.y))
		return float4(0, 0, 0, 0);
	float3 coord = input.Texcoord;
	float4 weaponColor = tex2D(uImage2, float2(1 - coord.y, coord.y));
	float4 mapColor = tex2D(uImage3, mul(float4(coord, 1) - float4(0.5, 0.5, 0, 0), heatRotation).xy + float2(0.5, 0.5));
	float greyValue = GetGreyValue(input.Texcoord.xy / float2(1, airFactor));
	float4 heatColor = tex2D(uImage3, float2(saturate(greyValue + lightShift), coord.y / airFactor));
	float3x4 colorMatrix = float3x4(weaponColor, mapColor, heatColor);
	float4 result = mul(AlphaVector, colorMatrix) * coord.z;
	if (heatMapAlpha)
	{
		result.a *= greyValue * airFactor;
		result.a = saturate(result.a);
	}
	return result;
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
	pass OriginColor // 单纯对两张图变换然后叠加，不使用采样图或者武器贴图
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_OriginColor();
	}
	pass VertexColor // 使用灰度图，颜色由传入颜色决定，渐变之类的还是用采样图代替吧
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_VertexColor();
	}
	pass MapColor // 使用采样图和武器贴图，颜色由线性插值决定(a,b,c)*(v1,v2,v3),a+b+c=1那种
	{
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction_MapColor();
	}
}
*/