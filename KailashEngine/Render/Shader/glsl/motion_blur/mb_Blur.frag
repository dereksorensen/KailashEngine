
out vec4 color;
in vec2 v_TexCoord;


uniform sampler2D sampler0;		// Source
uniform sampler2D sampler1;		// Velocity Texture
uniform sampler2D sampler2;		// Depth Texture


uniform float fps_scaler;



float cone(vec2 X, vec2 Y, vec2 V)
{
	return clamp(1.0 - length(X-Y)/length(V), 0.0, 1.0);
}
float cylinder(vec2 X, vec2 Y, vec2 V)
{
	return 1.0 - smoothstep(0.95*length(V), 1.05*length(V), length(X-Y));
}
float softDepthCompare(float Za, float Zb)
{
	return clamp(1.0 - (Za - Zb)/0.1, 0.0, 1.0);
	//return clamp( (1.0 + Za * 0.1) - Zb * 0.1, 0.0, 1.0);
}


vec4 motion_blur()
{
	vec4 scene = texture(sampler0, v_TexCoord);
	float depth = texture(sampler2, v_TexCoord).a;


	vec4 final = scene;
	vec2 texelSize = 1.0 / vec2(textureSize(sampler1, 0));
	float velocityScaler = fps_scaler;
	int MAX_SAMPLES = 8;
	

	vec2 velocity = texture(sampler1, v_TexCoord).xy * velocityScaler;
	

	
	float speed = length(velocity / texelSize);
	int numSamples = clamp(int(speed), 1, MAX_SAMPLES);

	float length_velocity = length(velocity);
	vec2 direction = velocity / length_velocity;

	if(length_velocity <= 0.0001)
	{
		return final;
	}

	float weight = 1.0 / length_velocity;
	final *= weight;

	for(int i = 1; i < numSamples; i++)
	{		

		vec2 offset = velocity * (float(i) / float(numSamples - 1.0) - 0.5);
		vec2 sampleTexCoord = v_TexCoord + offset;

		float sampleDepth = texture(sampler2, sampleTexCoord).a;
		vec2 sampleVelocity = texture(sampler1, sampleTexCoord).xy * velocityScaler;

		float f = softDepthCompare(depth, sampleDepth);
		float b = softDepthCompare(sampleDepth, depth);
		float temp = f * cone(sampleTexCoord, v_TexCoord, sampleVelocity) +
					b * cone(v_TexCoord, sampleTexCoord, velocity) +
					cylinder(sampleTexCoord, v_TexCoord, sampleVelocity) * cylinder(v_TexCoord, sampleTexCoord, velocity) * 2.0;
		weight += temp;

		vec4 currentSample =  texture(sampler0, sampleTexCoord);
		final += currentSample * temp;
	}


	final /= weight;
	return final;
}


void main()
{

	color = motion_blur();

}
