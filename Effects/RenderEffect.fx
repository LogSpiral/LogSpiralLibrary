sampler uImage0 : register(s0); //背景
sampler uImage2 : register(s1); //辅助贴图

texture2D tex0; //画到rendertarget2D上的内容
sampler2D uImage1 = sampler_state
{
	Texture = <tex0>;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

float2 offset; //偏移量
float invAlpha; //反色量

float getValue(float4 c1)
{
	float maxValue = max(max(c1.x, c1.y), c1.z);
	float minValue = min(min(c1.x, c1.y), c1.z);
	return (maxValue + minValue) / 2;
}

float4 PSFunction(float2 coords : TEXCOORD0) : COLOR0
{
	float2 vec = coords;
	//if (any(tex2D(uImage1, coords)))//如果传入的贴图上有颜色
	//{
	//	vec += offset; //偏移
	//}
	if (any(tex2D(uImage1, coords)))//如果传入的贴图上有颜色
	{
		float4 c = tex2D(uImage1, coords);
		vec += offset * getValue(c) * c.a; //偏移
	}
	float4 color = tex2D(uImage0, vec);
	float4 invColor = color;
	if (any(tex2D(uImage1, coords)))
	{
		invColor.rgb = 1 - invColor.rgb; //反色
		return color * (1 - invAlpha) + invColor * invAlpha;
	}
	return color;
}
//float4 maskBoundColor;
float4 maskGlowColor;
float tier1;
float tier2;
float2 position;
float2 ImageSize;
bool lightAsAlpha;
bool inverse;
float2 screenScale;

float GetLerpValue(float t, float from, float to)
{
	return saturate((t - from) / (to - from));
}
//超过阈值替换纹理
float4 PSFunction_Mask(float2 coords : TEXCOORD0) : COLOR0
{
	/*
	float4 c = tex2D(uImage1, coords);
	if (any(c))
	{
		float v = getValue(c) * c.a;
		if (inverse)
			v = 1 - v;
		if (v < tier1) 
			return tex2D(uImage0, coords) + maskGlowColor * maskGlowColor.a * v / invAlpha * float4(1, 1, 1, lightAsAlpha ? v : 1);
		if (v < tier2)
			return maskBoundColor;
		float2 vec = (coords * screenScale + offset / 2) / ImageSize;
		return tex2D(uImage2, vec);
	}
	return tex2D(uImage0, coords);
	*/
	//↑旧版代码
	
	float4 c = tex2D(uImage1, coords);
	if (any(c))
	{
		float v = getValue(c);
		float t1 = tier1;
		float t2 = tier2;
		float dist = t2 - t1;
		float4 colorMask = tex2D(uImage2, (coords * screenScale + offset / 2) / ImageSize);
		if (inverse)
		{
			float4 cache = c;
			c = colorMask;
			colorMask = cache;
		}
		if (v < t1 + dist)
			return lerp(c, maskGlowColor, smoothstep(0, 1, GetLerpValue(v, saturate(t1 - dist), t1 + dist)));
		
		return lerp(maskGlowColor, colorMask, smoothstep(0, 1, GetLerpValue(v, t1 + dist, saturate(t2 + dist))));
	}
	return tex2D(uImage0, coords);
}
//float4 PSFunction_Bloom(float2 coords : TEXCOORD0) : COLOR0
//{
//	float2 vec = coords;
//	float4 color = tex2D(uImage0, vec);
//	float2 d = float2(1, 1) / offset;
//	for (int n = -1; n < 2; n++)
//	{
//		for (int k = -1; k < 2; k++)
//		{
//			float2 v = float2(n, k);
//			float4 _color = tex2D(uImage1, coords + v / offset);
//			if (getValue(_color) > invAlpha)
//			{
//				color += _color * length(v - float2(0.5, 0.5)) * tier2;
				
//			}
//		}
//	}
//	return color;
//}
//float gauss[7] = { 0.05, 0.1, 0.24, 0.4, 0.24, 0.1, 0.05 };

//float gauss[3] = { 0.12, 0.76, 0.12 };
//float gauss[5] = { 0.02, 0.22, 0.52,0.22,0.02 };
float gauss[7] = { 0.01, 0.06, 0.24, 0.38, 0.24, 0.06, 0.01 };
//float gauss[9] = {0.003, 0.0227, 0.0958, 0.227, 0.3026, 0.227, 0.0958, 0.0227, 0.003};
//float gauss[11] = {0.0019, 0.0109, 0.0429, 0.1141, 0.2053, 0.2497, 0.2053, 0.1141, 0.0429, 0.0109, 0.0019};

//float gauss[7] = { 0.142, 0.142, 0.142, 0.142, 0.142, 0.142, 0.142 };



bool uBloomAdditive;
float threshold;
float range;
float intensity;
//c1是底色，c2是上色
float4 ColorBlend(float4 c1, float4 c2)
{
	return c1 + c2;
	float3 vec = uBloomAdditive ? c1.xyz * c1.a + c2.xyz * c2.a : lerp(c1.xyz, c2.xyz, c2.a);
	return float4(vec, c1.a + c2.a - c1.a * c2.a);
}

float4 PSFunction_BloomX(float2 coords : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(uImage0, coords);
	for (int n = -3; n <= 3; n++)
	{
		float2 v = float2(n, 0);
		float4 _color = tex2D(uImage0, coords + v / screenScale * range);
		if (dot(_color.xyz, 0.333) > threshold)
		{
			_color *= gauss[n + 3] * intensity;
			color = ColorBlend(color, _color);
		}
	}
	return color;
}
float4 PSFunction_BloomY(float2 coords : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(uImage0, coords);
	for (int n = -3; n <= 3; n++)
	{
		float2 v = float2(0, n);
		float4 _color = tex2D(uImage0, coords + v / screenScale * range);
		if (dot(_color.xyz, 0.333) > threshold)
		{
			_color *= gauss[n + 3] * intensity;
			color = ColorBlend(color, _color);
		}
	}
	return color;
}


float4 PSFunction_BloomMK(float2 coords : TEXCOORD0) : COLOR0 //MasakiKawase
{
	float4 color = tex2D(uImage0, coords);
	float k = 0.125 * intensity;
	for (int m = -1; m <= 1; m += 1)
	{
		for (int n = -1; n <= 1; n += 1)
		{
			float2 v = float2(m, n);
			float4 _color = tex2D(uImage0, coords + v / screenScale * range);
			if (dot(_color.xyz, 0.333) > threshold)
			{
				_color *= k;
				color = ColorBlend(color, _color);
			}
		}
	}
	for (int i = -1; i <= 1; i += 1)
	{
		k = 0.125 / (abs(i) + 1) * intensity;
		for (int j = -1; j <= 1; j += 1)
		{
			float2 v = float2(i, j) * 2.0;
			float4 _color = tex2D(uImage0, coords + v / screenScale * range);
			if (dot(_color.xyz, 0.333) > threshold)
			{
				_color *= k / (abs(j) + 1);
				color = ColorBlend(color, _color);
			}
		}
	}
	return color;
}

technique Technique1
{
	pass DistortEffect
	{
		PixelShader = compile ps_3_0 PSFunction();
	}
	pass MaskEffect
	{
		PixelShader = compile ps_3_0 PSFunction_Mask();
	}
	pass BloomEffectX
	{
		PixelShader = compile ps_3_0 PSFunction_BloomX();
	}
	pass BloomEffectY
	{
		PixelShader = compile ps_3_0 PSFunction_BloomY();
	}
	pass BloomEffectMK
	{
		PixelShader = compile ps_3_0 PSFunction_BloomMK();
	}
}