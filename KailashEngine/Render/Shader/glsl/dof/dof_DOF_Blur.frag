
in vec2 v_TexCoord;


out vec4 color;

uniform sampler2D sampler0;		// Scene
uniform sampler2D sampler1;		// Depth
uniform sampler2D sampler2;		// COC


uniform vec2 texture_size;
uniform float maxBlur = 160.0;
uniform float sigmaCoeff = 18.7;


vec4 guassBlur()
{
	vec4 scene = texture(sampler0, v_TexCoord);

	const float MAX_BLUR = maxBlur;
	float depth = texture(sampler1,v_TexCoord).w;


	float coc = texture(sampler2, v_TexCoord).r;
	coc = min(coc* maxBlur, maxBlur);
	int numSamples = int(ceil(coc));

	if(numSamples <= 0)
	{
		return scene;
	}

	float SIGMA = coc / sigmaCoeff;
	float sig2 = SIGMA * SIGMA;
	float TWO_PI = 6.2831853071795;

	vec3 gaussInc;
	gaussInc.x = 1.0 / (sqrt(TWO_PI) * SIGMA);
	gaussInc.y = exp(-0.5 / sig2);
	gaussInc.z = gaussInc.y * gaussInc.y;

	vec4 curPixel = scene * gaussInc.x;
	vec4 final = curPixel;

	float cSum = gaussInc.x;
	gaussInc.xy *= gaussInc.yz;


	//numSamples += int(weight);

	for(int i = 1; i < numSamples; i++)
	{

		vec2 offset = float(i) * texture_size;
		vec2 texCoord_n = v_TexCoord - offset;
		vec2 texCoord_p = v_TexCoord + offset;

		float coc_n = texture(sampler2, texCoord_n).r;
		float coc_p = texture(sampler2, texCoord_p).r;

		vec4 scene_n = texture(sampler0, texCoord_n);
		vec4 scene_p = texture(sampler0, texCoord_p);

		float depth_n = texture(sampler1,texCoord_n).w;
		float depth_p = texture(sampler1,texCoord_p).w;

		float cocWeight_n = clamp(coc + 1.0f - abs(float(i)),0,1);
		float cocWeight_p = clamp(coc + 1.0f - abs(float(i)),0,1);
		float blurWeight_n = coc_n;
		float blurWeight_p = coc_p;
		float depthWeight_n = (depth_n >= depth) ? 1.0 : 0.0;
		float depthWeight_p = (depth_p >= depth) ? 1.0 : 0.0;
		float weight_n = clamp(blurWeight_n, 0.0, 1.0);
		float weight_p = clamp(blurWeight_p, 0.0, 1.0);

		final += scene_n * gaussInc.x * weight_n / cocWeight_p;
		final += scene_p * gaussInc.x * weight_p / cocWeight_p;

		cSum += 2.0 * gaussInc.x * (weight_n + weight_p) / (2.0 * cocWeight_n);
		gaussInc.xy *= gaussInc.yz;

	}

	return final / cSum;

}
	



vec3 gatherBlur()
{

	vec4 scene = texture(sampler0, v_TexCoord);

	const float MAX_BLUR = maxBlur;
	float depth = texture(sampler1,v_TexCoord).w;

	float coc = texture(sampler2, v_TexCoord).r * MAX_BLUR;
	int numSamples = int(ceil(coc));

	if(numSamples <= 0)
	{
		return scene.xyz;
	}


	int count = 0;
	float totalWeight = 0;

	vec3 final = vec3(0.0);

	for(int i = -numSamples; i <= numSamples; ++i)
	{
		vec2 coord = v_TexCoord + float(i) * texture_size;

		float sampleCOC = texture(sampler2, coord).r;
		float sampleDEPTH = texture(sampler1,coord).w;

		float cocWeight = clamp(coc + 1.0f - abs(float(i)),0,1);
		float depthWeight = float(sampleDEPTH >= depth);
		float blurWeight= sampleCOC;
		float tapWeight = cocWeight * clamp(depthWeight + blurWeight,0,1);

		vec3 color = texture(sampler0, coord).xyz;
			
		final += color*tapWeight;
		totalWeight += tapWeight;
	}
	final /= totalWeight;

	return final;
}



void main()
{
	color = vec4(guassBlur().xyz,1.0);
	//color = vec4(gatherBlur(),1.0);
}
