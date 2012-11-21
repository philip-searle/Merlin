
#define fogEnd 100.0f
#define fogStart 10.0f

cbuffer VSGlobalPerFrameCB
{
	float4x4 gWorld; 
	float4x4 gView; 
	float4x4 gProj; 
	float4 colour;
};

struct VSInput
{
	float3 position : POSITION;
	float3 colour   : COLOR0;
};

struct VSOutput
{
	float4 position : SV_POSITION;
	float4 colour   : COLOR0;
};

VSOutput VShader(VSInput input)
{
	VSOutput output;

	float4x4 worldViewProj = mul(mul(gWorld, gView), gProj);
	output.position = mul(float4(input.position, 1), worldViewProj);

	//output.colour = float4(input.colour, 1.0f);
	output.colour = float4(
		frac(input.position.x / 65.5350f),
		frac(input.position.y / 65.5350f),
		frac(input.position.z / 65.5350f),
		1.0f);

	return output;
}

float4 PShader(VSOutput input) : SV_Target
{
	//return input.colour;
	return float4(
		input.position.z,
		input.position.z,
		input.position.z,
		1.0f);
}
