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
    float4 WorldPosition : TEXCOORD0;
    float2 TextureCoordinate : TEXCOORD1;
};

texture ModelTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float Time = 0;
float4 sphereCenter = float4(0.0, 0.0, 0.0, 0.0);
float sphereRadius = 2.0;

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
	// Get the texture texel textureSampler is the sampler, Texcoord is the interpolated coordinates
    float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
    textureColor.a = 1;
	// Color and texture are combined in this example, 80% the color of the texture and 20% that of the vertex
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