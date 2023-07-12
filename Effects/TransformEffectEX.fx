sampler uImage0 : register(s0);
float factor1;
float factor2;
float GetLerpValue(float from, float to, float v)
{
	return (v - from) / (to - from);
}

float4 PixelShaderFunction_Gapped(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	/*
	    def c2 = -0.500000, 0.500000, 6.283185, -3.141593
        def c3 = 0.150000, 0.250000, 1.000000, 0.000000
        def c4 = -0.000002, -0.000022, 0.002604, 0.000260
        def c5 = -0.020833, -0.125000, 1.000000, 0.500000
        dcl t.xy
        dcl v
        dcl_2d s
        add r.w, t.y, c2.y
        frc r.x, r.w
        mad r.x, r.x, c2.z, c2.w
        sincos r1.y, r.x, c4, c5
        add r.x, t.x, c2.x
        mad r.x, r1.y, -c3.x, r.x
        add r.y, t.y, c2.x
        mad r.y, r.y, -r.y, c3.y
        mul r.y, r.y, c1.x
        rcp r.y, r.y
        mad r.x, r.x, r.y, c2.y //获取新x
	
	
        add r.z, -r.x, c3.z //经典判定越界
        cmp r.z, r.z, c3.w, c3.z
        cmp r.w, r.x, c3.w, c3.z
        add r.z, r.z, r.w
        add r.y, t.y, c.x
        texld r1, r, s
        mul r1, r1, v
        cmp r, -r.z, r1, c3.w
        mov oC, r
	*/
	
	float to = factor1 * (0.25 - pow(coord.y - 0.5, 2)) * 3;
	float offsetX = 0.15 * sin(6.283185 * coord.y) + 0.5;
	float x = GetLerpValue(0, to, coord.x - offsetX) + 0.5;
	if (x < 0 || x > 1)
		return 0;
	float y = coord.y + factor2;
	return tex2D(uImage0, float2(x, y)) * color;
	return color;
}
float4 PixelShaderFunction_Eye(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	///让螺线庆祝一下自己总算抄反汇编没抄漏抄多抄错了///
	                  //鼓掌//
	//      ↓↓为了庆祝而特意留下的反编译码↓↓      //
	/*
	float c = factor1;						~\
	float c1 = factor2;						 |
	float4 c2 = float4(0.5, -0.5, 0.25, 1);	 |——>乱七八糟的参数声明
	float4 c3 = float4(0, 1, 0, 0);			 |
	float4 result = float4(0, 0, 0, 0);		_/
	
	result.w = coord.y + c;                 ~\       
	result.x = result.w + c2.y;				 |		 非常复杂，而且尽可能地节省了空间
	result.y = c + c2.x - coord.y;			 |		 但是说白了就是根据y获取x在多少的时候映射到1
	result.x = result.x * result.y + c2.z;	 |——\  to = sqrt(c^2-(y-0.5)^2+1/4)-c   //这东西画一下关于y的图像就知道是一个被x轴截开的半圆，我们去x轴往上的部分
	result.x = pow(result.x, 0.5);			 |	   > from = 0
	result.x -= c;							 |——/  t = GetLerpValue(from,to,x-0.5)
	result.x = 1 / result.x;				 |       x'= t + 0.5
	result.y = c2.x - coord.x;				 |      
	result.x = result.x * result.y + c2.x;  _/
	
	result.z = c2.w - result.x;              ~\
	result.z = result.z >= 0 ? c3.x : c3.y;   |——>这里这一块翻译过来就是你x'∈[0,1]
	result.w = result.x >= 0 ? c3.x : c3.y;   |
	result.z += result.w;                    _/
	
	result.y = c1 + coord.y;    ——>这里是给y加上时间偏移量
	
	if (result.z > 0)                               ~\
		return float4(0, 0, 0, 0);                   |——>猪猪都看得懂这里
	return tex2D(uImage0, result.xy) * color;       _/           
	*/     
	//      ↑↑为了庆祝而特意留下的反编译码↑↑      //
	float r2 = pow(factor1, 2) + 0.25; //半圆的半径的平方，半圆的圆心在(-factor1,0.5),因为过原点就直接代入(0,0)到圆方程里获取半径
	float to = sqrt(r2 - pow(coord.y - 0.5, 2)) - factor1; //反解方程获得x
	float x = GetLerpValue(0, to, coord.x - 0.5) + 0.5; //关于x=0.5进行缩放
	if (x != saturate(x))
		return 0;
	float y = coord.y + factor2;
	return tex2D(uImage0, float2(x, y)) * color;
}
float4 PixelShaderFunction_Moon(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	//第二次人工脑补就容易多了  但是还是有问题，不是正确的
	/*
	    def c2 = -0.500000, 0.500000, 0.250000, 1.000000
        def c3 = 0.000000, 1.000000, 0.000000, 0.000000
        dcl t.xy
        dcl v
        dcl_2d s
        add r.w, t.y, c.x
        add r.x, r.w, c2.x
        add r.y, -t.y, c.x
        add r.y, r.y, c2.y
        mad r.x, r.x, r.y, c2.z
        rsq r.x, r.x
        rcp r.x, r.x
        add r.x, r.x, c2.x
        rcp r.x, r.x
        mad r.y, t.x, -r.x, c2.w
        mul r1.x, r.x, t.x
	
	
        cmp r.x, r.y, c3.x, c3.y
        cmp r.y, r1.x, c3.x, c3.y
        add r.x, r.x, r.y
        add r1.y, t.y, c1.x
        texld r1, r1, s
        mul r1, r1, v
        cmp r, -r.x, r1, c3.x
        mov oC, r
	*/
	//这次是圆心在(0.5,-0.5)，半径平方为factor1^2+0.25的半圆
	float to = sqrt(pow(factor1, 2) + 0.25 - pow(coord.y - 0.5, 2)) - 0.5;
	float x = GetLerpValue(0, to, coord.x); //关于x=0进行缩放
	if (x < 0 || x > 1)
		return 0;
	float y = coord.y + factor2;
	return tex2D(uImage0, float2(x, y)) * color;
}
float4 PixelShaderFunction_Spike(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float to = sqrt((coord.y + factor1 - 0.5) * (factor1 + 1 - coord.y) + 0.25) - factor1;
	float x = GetLerpValue(0, to, coord.x - 0.5) + 0.5;
	if (x == saturate(x))
		return 0;
	float y = coord.y + factor2;
	return tex2D(uImage0, float2(x, y)) * color;
}
float4 PixelShaderFunction_Wave(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float r2 = pow(factor1, 2) + 0.25;
	float to = sqrt(r2 - pow(coord.y - 0.5, 2)) - factor1;
	float x = GetLerpValue(0, to, coord.x - 0.5) + 0.5;
	if (x == saturate(x))
		return 0;
	float y = coord.y + factor2;
	return tex2D(uImage0, float2(x, y)) * color;
}
float GapOffset(float v)
{
	return (1 - cos(6.283185 * sqrt(v))) * .5;
}
float4 PixelShaderFunction_Gapped2(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float k = factor1 * .5;
	float m = lerp(k, 1 - k, (1 + sin(6.283185 * coord.y)) * .5);
	float from = m + GapOffset(coord.y) * k;
	float to = m - GapOffset(1 - coord.y) * k;
	float x = GetLerpValue(from, to, coord.x);
	if (x < 0 || x > 1)
		return 0;
	float y = coord.y + factor2;
	return tex2D(uImage0, float2(x, y)) * color;
}
float4 PixelShaderFunction_Moon2(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float r2 = pow(factor1, 2) + 0.25; //半圆的半径的平方，半圆的圆心在(-factor1,0.5),因为过原点就直接代入(0,0)到圆方程里获取半径
	float to = sqrt(r2 - pow(coord.y - 0.5, 2)) - factor1; //反解方程获得x
	float x = GetLerpValue(0, to, coord.x); //关于x=0进行缩放,eye的变种
	if (x < 0 || x > 1)
		return 0;
	float y = coord.y + factor2;
	return tex2D(uImage0, float2(x, y)) * color;
}
float4 PixelShaderFunction_Tooth(float2 coord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	float to = sqrt((coord.y + factor1 - 0.5) * (factor1 + 1 - coord.y) + 0.25) - factor1;
	float x = GetLerpValue(0, to, coord.x - 0.5) + 0.5;
	if (x - 1 != saturate(x - 1) && x + 1 != saturate(x + 1))
		return 0;
	
	float y = coord.y + factor2;
	return tex2D(uImage0, float2(x, y)) * color;
}
technique Technique1
{
	pass Gapped
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Gapped();
	}
	pass Eye
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Eye();
	}
	pass Moon
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Moon();
	}
	pass Spike
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Spike();
	}
	pass Wave
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Wave();
	}
	pass Gapped2
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Gapped2();
	}
	pass Moon2
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Moon2();
	}
	pass Tooth
	{
		PixelShader = compile ps_3_0 PixelShaderFunction_Tooth();
	}
}


