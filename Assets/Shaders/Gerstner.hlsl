float3 GerstnerWave(float4 wave, float3 gridPoint, float time,
                    inout float3 tangent, inout float3 binormal)
{
    float steepness = wave.z;
    float wavelength = wave.w;
    float k = 2 * PI / wavelength;
    float c = sqrt(9.8 / k);
    float2 d = normalize(wave.xy);
    float f = k * (dot(d, gridPoint.xz) - c * time);
    float a = steepness / k;

    float sinF = sin(f);
    float cosF = cos(f);

    tangent += float3(
        -d.x * d.x * (steepness * sinF),
         d.x * (steepness * cosF),
        -d.x * d.y * (steepness * sinF));
    binormal += float3(
        -d.x * d.y * (steepness * sinF),
         d.y * (steepness * cosF),
        -d.y * d.y * (steepness * sinF));

    return float3(d.x * (a * cosF), a * sinF, d.y * (a * cosF));
}

// Shader Graph entry point. The "_float" suffix is required.
void GerstnerWaves_float(float3 GridPoint, float4 WaveA, float4 WaveB,
                         float4 WaveC, float Time,
                         out float3 Position, out float3 Normal)
{
    float3 tangent = float3(1, 0, 0);
    float3 binormal = float3(0, 0, 1);
    float3 p = GridPoint;

    p += GerstnerWave(WaveA, GridPoint, Time, tangent, binormal);
    p += GerstnerWave(WaveB, GridPoint, Time, tangent, binormal);
    p += GerstnerWave(WaveC, GridPoint, Time, tangent, binormal);

    Position = p;
    Normal = normalize(cross(binormal, tangent));
}