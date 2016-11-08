#ifndef GLSL2CG
#define GLSL2CG


#define M_PI 3.14159265358979323846
#define vec2 float2
#define vec3 float3
#define vec4 float4
#define fract frac
#define mix lerp
#define mod fmod

// Modulo 289, optimizes to code without divisions
vec3 mod289(vec3 x) {
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}
vec4 mod289(vec4 x) {
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}

// Permutation polynomial (ring size 289 = 17*17)
vec3 permute(vec3 x) {
	return mod289(((x*34.0) + 1.0)*x);
}
vec4 permute(vec4 x) {
	return mod289(((x*34.0) + 1.0)*x);
}


#endif
