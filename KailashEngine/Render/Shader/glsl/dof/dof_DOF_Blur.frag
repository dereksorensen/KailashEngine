
in vec2 v_TexCoord;


out vec3 color;

uniform sampler2D sampler0;		// Scene
uniform sampler2D sampler1;		// COC


uniform vec2 texture_size;
uniform float max_blur;
uniform float sigmaCoeff = 18.7;


vec3 gaussBlur()
{
	vec3 scene = texture(sampler0, v_TexCoord).xyz;
	float coc = texture(sampler1, v_TexCoord).r;

	coc = min(coc * max_blur, max_blur);
	int num_samples = int(ceil(coc));

	if(num_samples <= 0)
	{
		return scene;
	}

	float SIGMA = float(num_samples) / sigmaCoeff;
	float sig2 = SIGMA * SIGMA;

	vec3 guass_increment;
	guass_increment.x = 1.0 / (sqrt(MATH_2_PI) * SIGMA);
	guass_increment.y = exp(-0.5 / sig2);
	guass_increment.z = guass_increment.y * guass_increment.y;


	vec3 final = scene * guass_increment.x;

	float increment_sum = guass_increment.x;
	guass_increment.xy *= guass_increment.yz;

	for(int i = 1; i < num_samples; i++)
	{

		vec2 offset = float(i) * texture_size;
		vec2 texCoord_n = v_TexCoord - offset;
		vec2 texCoord_p = v_TexCoord + offset;

		float coc_n = texture(sampler1, texCoord_n).r;
		float coc_p = texture(sampler1, texCoord_p).r;

		vec3 scene_n = texture(sampler0, texCoord_n).xyz;
		vec3 scene_p = texture(sampler0, texCoord_p).xyz;
		
		float cocWeight_n = clamp(coc + 1.0f - abs(float(i)),0,1);
		float cocWeight_p = clamp(coc + 1.0f - abs(float(i)),0,1);
		float blurWeight_n = coc_n;
		float blurWeight_p = coc_p;
		float weight_n = clamp(blurWeight_n, 0.0, 1.0);
		float weight_p = clamp(blurWeight_p, 0.0, 1.0);

		final += scene_n * guass_increment.x * weight_n / cocWeight_p;
		final += scene_p * guass_increment.x * weight_p / cocWeight_p;

		increment_sum += 2.0 * guass_increment.x * (weight_n + weight_p) / (2.0 * cocWeight_n);
		guass_increment.xy *= guass_increment.yz;

	}

	return final / increment_sum;
}
	




vec3 boxBlur()
{

	vec3 scene = texture(sampler0, v_TexCoord).xyz;
	float depth = texture(sampler1,v_TexCoord).w;
	float coc = texture(sampler1, v_TexCoord).r;

	float max_blur_adjusted = max_blur / 22.0;
	coc = min(coc * max_blur_adjusted, max_blur_adjusted);
	int num_samples = int(ceil(coc));

	if(num_samples <= 0)
	{
		return scene.xyz;
	}

	int count = 0;
	float totalWeight = 0;

	vec3 final = vec3(0.0);

	for(int i = -num_samples; i <= num_samples; ++i)
	{
		vec2 offset = float(i) * texture_size;
		vec2 coord = v_TexCoord + offset;

		float coc_left = texture(sampler1, coord).r;
		float depth_left = texture(sampler1,coord).w;

		float cocWeight = clamp(coc + 1.0f - abs(float(i)),0,1);
		float depthWeight = float(depth_left >= depth);
		float tapWeight = cocWeight * clamp(depthWeight + coc_left,0,1);

		float cocWeight_n = clamp(coc + 1.0f - abs(float(i)),0,1);
		float blurWeight_n = coc_left;
		float weight_n = clamp(blurWeight_n, 0.0, 1.0);
		//tapWeight = weight_n / cocWeight_n;

		vec3 color = texture(sampler0, coord).xyz * tapWeight;
			
		final += color;
		totalWeight += tapWeight;
	}
	final /= totalWeight;

	return final;
}


void main()
{
	//color = gaussBlur();
	color = boxBlur();
}
