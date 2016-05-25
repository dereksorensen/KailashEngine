

out vec4 color;


in vec4 v_color;
in vec2 v_TexCoord;

uniform sampler2D diffuse_texture;
uniform vec3 diffuse_color;

uniform int enable_specular_texture;
uniform sampler2D specular_texture;
uniform vec3 specular_color;
uniform float specular_shininess;

uniform int enable_normal_texture;
uniform sampler2D normal_texture;

uniform sampler2D displacement_texture;

uniform int enable_parallax_texture;
uniform sampler2D parallax_texture;



void main()
{
	vec4 tDiffuse = texture(diffuse_texture, v_TexCoord);

	vec4 temp_color = vec4(diffuse_color, 1.0) + tDiffuse;








	float gamma = 1.8;
	vec3 gamma_correction = vec3(1.0 / gamma);
	vec3 final = pow(temp_color.xyz, gamma_correction);

	color = vec4(final.xyz, 1.0);

}
