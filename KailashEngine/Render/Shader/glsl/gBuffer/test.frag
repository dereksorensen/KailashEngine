

out vec4 color;


in vec4 v_color;
in vec2 v_TexCoord;

uniform sampler2D diffuse_texture;
uniform vec3 diffuse_color;


void main()
{
	vec4 tDiffuse = texture(diffuse_texture, v_TexCoord);

	vec4 temp_color = vec4(diffuse_color, 1.0) + tDiffuse;








	float gamma = 1.8;
	vec3 gamma_correction = vec3(1.0 / gamma);
	vec3 final = pow(temp_color.xyz, gamma_correction);

	color = vec4(final.xyz, 1.0);

}
