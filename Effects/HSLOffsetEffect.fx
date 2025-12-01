sampler uImage0 : register(s0);
float uHueOffset;
float uSaturateOffset;
float uLuminosityOffset;
float3 RGBToHSL(float4 color)
{
	float r = color.x;
	float g = color.y;
	float b = color.z;
	float minC = min(r, min(g, b));
	float maxC = max(r, max(g, b));
	float H = 0.;
	float L = (minC + maxC) * .5;
	if (minC == maxC)
		return float3(H, 0, L);
	else
	{
		float S = 0.;
		float diff = maxC - minC;
		if (L > .5)
			S = diff / (2. - minC - maxC);
		else
			S = diff / (minC + maxC);
            
		if (maxC == r)
			H = (g - b) / diff + (g < b ? 6. : 0.);
		else if (maxC == g)
			H = (b - r) / diff + 2.;
		else if (maxC == b)
			H = (r - g) / diff + 4.;
		H /= 6.;
		return float3(H, S, L);
	}
}
float3 HSLToRGB(float3 hsl)
{
	float H = hsl.x - floor(hsl.x);
	float S = saturate(hsl.y);
	float L = saturate(hsl.z);
	float r = abs(H * 6. - 3.) - 1.;
	float g = 2. - abs(H * 6. - 2.);
	float b = 2. - abs(H * 6. - 4.);
    
	float C = 1. - abs(1. - L * 2.);
	C *= S;
	float m = L - C * .5;
	return float3(r, g, b) * C + float3(m, m, m);
}

float4 PixelShaderFunction(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float4 mainColor = tex2D(uImage0, coord) * color; //¶ÁÈ¡Êý¾Ý
	float a = mainColor.a;
	return float4(HSLToRGB(RGBToHSL(mainColor) + float3(uHueOffset,uSaturateOffset,uLuminosityOffset)), a);

}


technique Technique1
{
	pass HSLOffset
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}