
out vec4 color;

in vec2 v_TexCoord;

uniform sampler2D sampler0;		// Source Texture


uniform int blur_amount;
uniform int iteration;
uniform vec2 size_and_direction;

vec3 SiWrecklessStreakFilterRGB(
	sampler2D tSource, vec2 texCoord,
	vec2 streak_direction,
	int streakSamples, float attenuation,
	int iter)
{
	vec2 texCoordSample = vec2(0.0);
	vec3 cOut = vec3(0.0);
	float b = pow(streakSamples, iter);

	for (int s = 0; s < streakSamples; s++)
	{
		// Weight = a^(b*s)
		float weight = pow(attenuation, b * s);

		// Streak direction is a 2D vector in image space
		texCoordSample = texCoord + (streak_direction * b * vec2(s, s));

		// Scale and accumulate
		cOut += clamp(weight, 0.0, 1.0) * texture(tSource, texCoordSample).xyz;
	}

	return max(cOut, vec3(0.0));
}

void main()
{
	
	vec2 renderSize = vec2(1.0) / textureSize(sampler0, 0).xy;


	float atten = 0.89;

	vec3 streak = SiWrecklessStreakFilterRGB(
		sampler0, 
		v_TexCoord,
		size_and_direction, 
		blur_amount, atten, iteration);
	
	streak += SiWrecklessStreakFilterRGB(
		sampler0, 
		v_TexCoord,
		-size_and_direction, 
		blur_amount, atten, iteration);


	color = vec4(streak, 1.0);
}
