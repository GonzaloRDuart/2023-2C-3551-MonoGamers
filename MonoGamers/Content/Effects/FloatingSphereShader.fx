﻿#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD1;
};

float Time = 0;

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

	// Animate position
    float atenuacion = 0.8;
    float x = input.Position.x;
    float z = input.Position.z;
    input.Position.x = x * cos(Time * atenuacion) - z * sin(Time * atenuacion);
    input.Position.z = z * cos(Time * atenuacion) + x * sin(Time * atenuacion);
    	
    float4 worldPosition = mul(input.Position, World);
    worldPosition.y = worldPosition.y + 5 * cos(Time);
    float4 viewPosition = mul(worldPosition, View);
    
	
	// Project position
    output.Position = mul(viewPosition, Projection);

	// Propagate texture coordinates
    output.TextureCoordinate = input.TextureCoordinate;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{

    float4 textureColor = float4(1, 1, 1, 1) * 0.5;

    return textureColor;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
