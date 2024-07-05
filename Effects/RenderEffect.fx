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
float4 maskGlowColor;
float4 maskBoundColor = float4(1, 1, 1, 1);
float tier2;
float2 position;
float2 ImageSize;
bool lightAsAlpha;
bool inverse;
//超过阈值替换纹理
float4 PSFunction_Mask(float2 coords : TEXCOORD0) : COLOR0
{
	float4 c = tex2D(uImage1, coords);
	if (any(c))
	{
		float v = getValue(c) * c.a;
		if (inverse)
			v = 1 - v;
		if (v < invAlpha) 
		//此处invAlpha只是作为一个参数使用，不是字面意思
		//小于invAlpha的会被替换成颜色插值
		{
			if (lightAsAlpha)
				return tex2D(uImage0, coords) + maskGlowColor * maskGlowColor.a * v / invAlpha * float4(1, 1, 1, v);
			return tex2D(uImage0, coords) + maskGlowColor * maskGlowColor.a * v / invAlpha;
		}
		if (v < tier2)
		{
			return maskBoundColor;
		}
		float2 vec = (coords + position / float2(3840, 2240)) / ImageSize * float2(1920, 1120);
		
		return tex2D(uImage2, float2(vec.x % 1, vec.y % 1));
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
float gauss[7] = { 0.05, 0.1, 0.24, 0.4, 0.24, 0.1, 0.05 };
//float gauss[7] = { 0.142, 0.142, 0.142, 0.142, 0.142, 0.142, 0.142 };

//此处几个全局变量都不是字面意义
//position实际上是两个float，阈值和步长
//offset 屏幕大小
//tier2 亮度

//x方向模糊
float4 PSFunction_BloomX(float2 coords : TEXCOORD0) : COLOR0
{
	float2 vec = coords;
	float4 color = tex2D(uImage0, vec);
	for (int n = -3; n <= 3; n++)
	{
		float2 v = float2(n, 0);
		float4 _color = tex2D(uImage1, coords + v / offset * position.y);
		if (dot(_color, 0.25) > position.x)
		{
			color += _color * gauss[n + 3] * tier2;
		}
	}
	return color;
}
//y方向模糊
float4 PSFunction_BloomY(float2 coords : TEXCOORD0) : COLOR0
{
	float2 vec = coords;
	float4 color = tex2D(uImage0, vec);
	for (int n = -3; n <= 3; n++)
	{
		float2 v = float2(0, n);
		float4 _color = tex2D(uImage1, coords + v / offset * position.y);
		if (dot(_color, 0.25) > position.x)
		{
			color += _color * gauss[n + 3] * tier2;
		}
	}
	return color;
}
float4 PSFunction_BloomX_Weak(float2 coords : TEXCOORD0) : COLOR0
{
	float2 vec = coords;
	float4 color = tex2D(uImage0, vec);
	for (int n = -3; n <= 3; n++)
	{
		float2 v = float2(n, 0);
		float4 _color = tex2D(uImage1, coords + v / offset * position.y);
		if (dot(_color, 0.25) > position.x && dot(_color, 0.25) < 0.9)
		{
			color += _color * gauss[n + 3] * tier2;
		}
	}
	return color;
}
float4 PSFunction_BloomY_Weak(float2 coords : TEXCOORD0) : COLOR0
{
	float2 vec = coords;
	float4 color = tex2D(uImage0, vec);
	for (int n = -3; n <= 3; n++)
	{
		float2 v = float2(0, n);
		float4 _color = tex2D(uImage1, coords + v / offset * position.y);
		if (dot(_color, 0.25) > position.x && dot(_color, 0.25) < 0.9)
		{
			color += _color * gauss[n + 3] * tier2;
		}
	}
	return color;
}


bool uBloomAdditive;
float threshold;
float range;
float intensity;
float2 screenScale;
float maximum;
//c1是底色，c2是上色
float4 ColorBlend(float4 c1, float4 c2)
{
	return c1 + c2;
	float3 vec = uBloomAdditive ? c1.xyz * c1.a + c2.xyz * c2.a : lerp(c1.xyz, c2.xyz, c2.a);
	return float4(vec, c1.a + c2.a - c1.a * c2.a);
}

float4 PSFunction_BloomX_New(float2 coords : TEXCOORD0) : COLOR0
{
	float2 vec = coords;
	float4 color = uBloomAdditive ? tex2D(uImage0, vec) : 0;
	if (dot(color.xyz, 0.333) > maximum)
		return color;
	for (int n = -3; n <= 3; n++)
	{
		float2 v = float2(n, 0);
		float4 _color = tex2D(uImage1, coords + v / screenScale * range);
		if (dot(_color.xyz, 0.333) > threshold)
		{
			_color *= gauss[n + 3] * intensity;
			color = ColorBlend(color, _color);
		}
	}
	return color;
}
float4 PSFunction_BloomY_New(float2 coords : TEXCOORD0) : COLOR0
{
	float2 vec = coords;
	float4 color = uBloomAdditive ? tex2D(uImage0, vec) : 0;
	if (dot(color.xyz, 0.333) > maximum)
		return color;
	for (int n = -3; n <= 3; n++)
	{
		float2 v = float2(0, n);
		float4 _color = tex2D(uImage1, coords + v / screenScale * range);
		if (dot(_color.xyz, 0.333) > threshold)
		{
			_color *= gauss[n + 3] * intensity;
			color = ColorBlend(color, _color);
		}
	}
	return color;
}


float4 PSFunction_DistortX(float2 coords : TEXCOORD0) : COLOR0
{
	float2 vec = coords;
	for (int n = -3; n <= 3; n++)
	{
		float2 v = float2(n, 0);
		float4 _color = tex2D(uImage1, coords + v / offset * position.y);
		float i = dot(_color, 0.25);
		
		if (i > position.x)
		{
			vec += i * gauss[n + 3] * ImageSize;
		}
	}
	return tex2D(uImage0, vec);
}
float4 PSFunction_DistortY(float2 coords : TEXCOORD0) : COLOR0
{
	float2 vec = coords;
	for (int n = -3; n <= 3; n++)
	{
		float2 v = float2(0, n);
		float4 _color = tex2D(uImage1, coords + v / offset * position.y);
		float i = dot(_color, 0.25);
		if (i > position.x)
		{
			vec += i * gauss[n + 3] * ImageSize;
		}
	}
	return tex2D(uImage0, vec);
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
	pass DistortEffectX
	{
		PixelShader = compile ps_3_0 PSFunction_DistortX();
	}
	pass DistortEffectY
	{
		PixelShader = compile ps_3_0 PSFunction_DistortY();
	}
	pass BloomEffectX_Weak
	{
		PixelShader = compile ps_3_0 PSFunction_BloomX_Weak();
	}
	pass BloomEffectY_Weak
	{
		PixelShader = compile ps_3_0 PSFunction_BloomY_Weak();
	}
	pass BloomEffectX_New
	{
		PixelShader = compile ps_3_0 PSFunction_BloomX_New();
	}
	pass BloomEffectY_New
	{
		PixelShader = compile ps_3_0 PSFunction_BloomY_New();
	}
}