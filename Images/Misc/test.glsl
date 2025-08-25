#iChannel0 "file://Misc_1.png"
void main()
{
	vec2 coord = gl_FragCoord.xy / iResolution.xy;
	coord -= .5;
	coord /= iResolution.y / iResolution.x;
	float r = length(coord) * 2.0;
	float t = atan(coord.y, coord.x);
	float k = 0.5 - 0.5 * cos(6.28 * r);
	if (r > 1.0)k = 0.0;
	gl_FragColor = texture(iChannel0, vec2(r * 2.0 - iTime * .25, t / 6.28 + r * .25)) * k;
}