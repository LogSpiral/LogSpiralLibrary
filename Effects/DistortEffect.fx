//sampler uImage0 : register(s0);
//sampler uImage1 : register(s1);
//sampler uImage2 : register(s2);
//float uTime;
//float4x4 uTransform;
//float2 unit;
//struct VSInput 
//{
//	float2 Pos : POSITION0;
//	float4 Color : COLOR0;
//	float3 Texcoord : TEXCOORD0;
//};
//struct PSInput 
//{
//	float4 Pos : SV_POSITION;
//	float4 Color : COLOR0;
//	float3 Texcoord : TEXCOORD0;
//};
//float4 PixelShaderFunction(PSInput input) : COLOR0
//{
//	return tex2D(uImage0, input.Color.rg + unit * input.Texcoord.z * tex2D(uImage1, input.Texcoord.xy).r * tex2D(uImage2, input.Texcoord.xy + float2(uTime, 0)).r);
//}
//PSInput VertexShaderFunction(VSInput input)
//{
//	PSInput output;
//	output.Color = input.Color;
//	output.Texcoord = input.Texcoord;
//	output.Pos = mul(float4(input.Pos, 0, 1), uTransform);
//	return output;
//}


//technique Technique1 
//{
//	pass Distort
//	{
//		VertexShader = compile vs_2_0 VertexShaderFunction();
//		PixelShader = compile ps_2_0 PixelShaderFunction();
//	}
//}
sampler uImage0 : register(s0);
sampler uImage2 : register(s1);

texture2D tex0;
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
float4 PSFunction_Mask(float2 coords : TEXCOORD0) : COLOR0
{
	float4 c = tex2D(uImage1, coords);
	if (any(c))
	{
		float v = getValue(c) * c.a;
		if (inverse)
			v = 1 - v;
		if (v < invAlpha)
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
		PixelShader = compile ps_2_0 PSFunction();
	}
	pass MaskEffect
	{
		PixelShader = compile ps_2_0 PSFunction_Mask();
	}
	pass BloomEffectX
	{
		PixelShader = compile ps_2_0 PSFunction_BloomX();
	}
	pass BloomEffectY
	{
		PixelShader = compile ps_2_0 PSFunction_BloomY();
	}
	pass DistortEffectX
	{
		PixelShader = compile ps_2_0 PSFunction_DistortX();
	}
	pass DistortEffectY
	{
		PixelShader = compile ps_2_0 PSFunction_DistortY();
	}
	pass BloomEffectX_Weak
	{
		PixelShader = compile ps_2_0 PSFunction_BloomX_Weak();
	}
	pass BloomEffectY_Weak
	{
		PixelShader = compile ps_2_0 PSFunction_BloomY_Weak();
	}
}