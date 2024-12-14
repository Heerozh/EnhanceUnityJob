using Unity.Mathematics;

namespace EnhanceJobSystem
{
    public static class MathExtensions
    {
        public static float3 Slerp(this float3 a, float3 b, float t)
        {
            float dot = math.dot(a, b);
            dot = math.clamp(dot, -1.0f, 1.0f);

            // Calculate the angle between the vectors
            float theta = math.acos(dot) * t;

            // Calculate the relative vector
            var relativeVec = math.normalize(b - a * dot);

            // Perform the slerp
            return a * math.cos(theta) + relativeVec * math.sin(theta);
        }


        public static float FractalNoise(this float3 pos, float persistence, int octaves,
            float lacunarity,
            float power, bool ridged)
        {
            float value = 0.0f;

            // Initial values
            float amplitude = 0.5f;
            float frequency = 1.0f;

            // Loop of octaves
            float maxValue = 0; // Used for normalizing result to 0.0 - 1.0
            for (int i = 0; i < octaves; i++)
            {
                if (ridged)
                {
                    value += amplitude * (1.0f - (math.abs(noise.snoise(pos * frequency))));
                }
                else
                {
                    value += amplitude * noise.snoise(pos * frequency);
                }

                frequency *= lacunarity;
                maxValue += amplitude;
                amplitude *= persistence;
            }

            return value / maxValue * power;
        }
    }
}
