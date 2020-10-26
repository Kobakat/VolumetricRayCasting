///Header file for raymarching shader.
///Includes signed distance functions
///
///Credit to https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
///for these functions


//p = position
//r = radius
float sdSphere(float3 p, float r)
{
  return length(p) - r;
}

//p = position
//b = box dimensions
float sdBox(float p, float3 b)
{
	float3 q = abs(p) - b;
	return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
}

//p = position
//b = box dimensions
//r = roundness factor
float sdRoundBox(float3 p, float3 b, float r)
{
	float3 q = abs(p) - b;
	return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0) - r;
}

//p = position
//o = outer radius
//i = inner raidus
float sdTorus(float3 p, float o, float i)
{
	float2 q = float2(length(p.xz) - i, p.y);
	return length(q) - o;
}

//p = position
//c = sin/cos of the angle
//h = height
float sdCone(in float3 p, in float2 c, float h)
{
	float2 q = h * float2(c.x / c.y, -1.0);

	float2 w = float2(length(p.xz), p.y);
	float2 a = w - q * clamp(dot(w, q) / dot(q, q), 0.0, 1.0);
	float2 b = w - q * float2(clamp(w.x / q.x, 0.0, 1.0), 1.0);
	float k = sign(q.y);
	float d = min(dot(a, a), dot(b, b));
	float s = max(k * (w.x * q.y - w.y * q.x), k * (w.y - q.y));
	return sqrt(d) * sign(s);
}



