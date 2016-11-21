out vec4 color;

in vec2 v_TexCoord;


uniform sampler2D sampler0;		// Source Texture


uniform int blur_amount;
uniform vec2 texture_size;



vec4 guassian_blur()
{	
	vec4 source_texture = texture(sampler0, v_TexCoord);
	float num_samples = blur_amount;

	float SIGMA = float(num_samples) / 18.7;
	float SIGMA_2 = SIGMA * SIGMA;

	vec3 guass_increment;
	guass_increment.x = 1.0 / (sqrt(MATH_2_PI) * SIGMA);
	guass_increment.y = exp(-0.5 / SIGMA_2);
	guass_increment.z = guass_increment.y * guass_increment.y;

	vec4 final = source_texture * guass_increment.x;

	float increment_sum = guass_increment.x;
	guass_increment.xy *= guass_increment.yz;

	for(float i = 1; i < num_samples; i++)
	{		
		vec2 offset = float(i) * texture_size;
		final += texture(sampler0, v_TexCoord - offset) * guass_increment.x;
		final += texture(sampler0, v_TexCoord + offset) * guass_increment.x;

		increment_sum += 2.0 * guass_increment.x;
		guass_increment.xy *= guass_increment.yz;
	}

	return final / increment_sum;
}



void main()
{
	color = guassian_blur();	
}
