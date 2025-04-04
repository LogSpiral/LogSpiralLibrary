sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;
float4x4 TransformMatrix;
bool useHeatMap;

float2 width;
float2 offset;

struct PSInput
{
	float4 Pos : SV_POSITION;
	float4 Color : COLOR0;
	float3 Texcoord : TEXCOORD0;
};



#define Identity float4(1.0,0,0.0,1.0)

// 这一块是奇奇怪怪的复函数库
float2 invereive(float2 v, float r)
{
	return v * r * r / dot(v, v);
}
float2 CplxDiv(float2 a, float2 b)
{
	float d = dot(b, b);
	return float2(dot(a, b), a.y * b.x - a.x * b.y) / d;
}
float2 CplxPow(float2 z, float n)
{
	float r = length(z);
	float a = atan2(z.y, z.x);
	return pow(r, n) * float2(cos(a * n), sin(a * n));
}
float2 CplxSqrt(float2 z)
{
	float m = length(z);
	return sqrt(0.5 * float2(m + z.x, m - z.x)) * float2(1.0, sign(z.y));
}
float2 CplxConj(float2 z)
{
	return float2(z.x, -z.y);
}
float2 CplxEXP(float2 z)
{
	return exp(z.x) * float2(cos(z.y), sin(z.y));
}
float2 CplxMul(float2 z1, float2 z2)
{
	return float2(z1.x * z2.x - z1.y * z2.y, z1.x * z2.y + z1.y * z2.x);
}
float2 CplxSinh(float2 z)
{
	return (CplxEXP(z) - CplxEXP(-z)) * .5;
}
float2 CplxCosh(float2 z)
{
	return (CplxEXP(z) + CplxEXP(-z)) * .5;
}
float2 CplxSin(float2 z)
{
	z = float2(-z.y, z.x);
	z = CplxSinh(z);
	z = float2(z.y, -z.x);
	return z;
}
float2 CplxCos(float2 z)
{
	z = float2(-z.y, z.x);
	z = CplxCosh(z);
	return z;
}

// 坐标转矩阵的函数
// 这里我是用坐标获取颜色，然后转cmyk，塞到矩阵里了
float4 coordToMatrix(float2 coord,float4 c)
{
   // return float4(coord.x,-coord.y,coord.y,coord.x);
   // return texture(iChannel0,coord);
   
	//float4 c = tex2D(uImage0, coord);
   
	float m = max(max(c.x, c.y), c.z);
   
	return float4(1.0, 1.0, 1.0, 1.0) - float4(c.x / m, c.y / m, c.z / m, m);
}

// 矩阵转回颜色，这里是作为cmyk转回rgba
float4 matrixToColor(float4 m)
{

   // return matrix;
   
float ik = 1.0 - m. w;
   float3 c = (float3(1.0,1.0,1.0) - m.xyz) *
ik;
   return float4(c,1.0);
}

// 矩阵代入的函数
// 比如如果你要 e^{M * t}
// 那这里应该写 exp(v * uTime)
// 我这里是写的 M * e^{M * t}
// 这样在uTime = 0.0的时候基本是原图像
float pureFunction(float v)
{

    //return exp(uTime * v);
    
	return v * (cos(uTime * v) * .5 + .5);
    
    // 这里我选用函数v|->v * exp(t * v) 后续两个函数实现要以这个为准
    //return v * exp(uTime * v);
}

// 代入的函数的复数实现，上面有我的复数库(x
float2 complexFunction(float2 v)
{
    //return CplxEXP(v * uTime);
    
	return CplxMul(v, CplxCos(uTime * v) * .5 + float2(.5, 0.0));
    
    //return CplxMul(CplxEXP(v * uTime),v);
}

// 代入的函数的导函数
// 因为特征值重根的情况其实比较少
// 所以放一边不管也是可以的(逃
// 要注意的是，是关于v的导数，而不是关于t的
float derivativeFunction(float v)
{
    //return uTime * exp(uTime * v);
    
    
	return (cos(uTime * v) + 1.0 - v * uTime * sin(uTime * v)) * .5;
    
    //return (uTime * v + 1.0) * exp(uTime * v);
}



// 双实根的处理
float4 realFunction(float4 M, float D, float m, float q)
{
    //return float4(1.0,0.0,0.0,1.0);
    
	q = sqrt(q);
	float l1 = m + q;
	float l2 = m - q;
	float f1 = pureFunction(l1);
	float f2 = pureFunction(l2);
	return ((f1 - f2) * M - D * (f1 / l1 - f2 / l2) * Identity) / q * .5;
}

// 单实根的处理，这个比较难碰见
float4 singleFunction(float4 M, float l)
{
    //return float4(0.0,1.0,0.0,1.0);

	return Identity * (pureFunction(l) - l * derivativeFunction(l)) + M * derivativeFunction(l);
}

// 双复根的处理
float4 imagineFunction(float4 M, float m, float q)
{
    //return float4(0.0,0.0,1.0,1.0);
    
	q = sqrt(-q);
	float2 l1 = float2(m, q);
	float2 l2 = float2(m, -q);
	float2 f1 = complexFunction(l1);
	float2 f2 = complexFunction(l2);
	float2 u = CplxMul(f1, l2) - CplxMul(f2, l1);
	return ((f1.y - f2.y) * M - u.y * Identity) / q * .5;
}


float4 mainImage(PSInput input):COLOR0
{
    // 获取单位化坐标
	float2 uv = input.Texcoord;// / iResolution.xy;
	float4 c = tex2D(uImage0, uv);
	if (!any(c.xyz))
		return 0;
    // 坐标转矩阵
		float4 C = coordToMatrix(uv,c);
    
    // 计算矩阵函数
	float D = C.x * C.w - C.y * C.z;
	float m = (C.x + C.w) * .5;
	float q = m * m - D;
	if (q > 0.0)
		C = realFunction(C, D, m, q);
	else if (q == 0.0)
		C = singleFunction(C, m);
	else
		C = imagineFunction(C, m, q);
    
    // 矩阵转颜色
	return matrixToColor(C);
}

technique Technique1
{
	pass MCosMt
	{
		PixelShader = compile ps_3_0 mainImage();
	}
}