

out vec4 color;

in vec2 v_TexCoord;

uniform sampler2D sampler0;



void main()
{
	
	vec4 tex = texture(sampler0, v_TexCoord);

	float gamma = 1.8;
	vec3 gamma_correction = vec3(1.0 / gamma);
	vec3 final = pow(tex.xyz, gamma_correction);

	color = vec4(final, 1.0);

}
