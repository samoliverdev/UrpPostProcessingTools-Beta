#ifndef FUNCTIONS_INCLUDED
#define FUNCTIONS_INCLUDED

uniform float4x4 _InverseView;
uniform Texture2D _DepthNormalsTexture;

float DecodeFloatRG(float2 enc) {
    float2 kDecodeDot = float2(1.0, 1 / 255.0);
    return dot(enc, kDecodeDot);
}

//
void GetInverseView_float(out float4x4 Out){
    Out = _InverseView;
}

void GetDepthNormalsTexture(out Texture2D Out){
    Out = _DepthNormalsTexture;
}

float3 DecodeViewNormalStereo(float4 enc4) {
    float kScale = 1.7777;
    float3 nn = enc4.xyz * float3(2 * kScale, 2 * kScale, 0) + float3(-kScale, -kScale, 1);
    float g = 2.0 / dot(nn.xyz, nn.xyz);
    float3 n;
    n.xy = g * nn.xy;
    n.z = g - 1;
    return n;
}

void DecodeDepthNormal_float(float4 enc, out float depth, out float3 normal) {
    depth = DecodeFloatRG(enc.zw);
    normal = DecodeViewNormalStereo(enc);
}

void GetWorldNormal_float(float2 uv, SamplerState ss, out float3 Out){
    float4 enc = SAMPLE_TEXTURE2D(_DepthNormalsTexture, ss, uv);
    float3 normal = DecodeViewNormalStereo(enc);
    
    Out = mul((float3x3)_InverseView, normal);
}
#endif