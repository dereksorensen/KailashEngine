

out vec4 color;


in vec4 v_color;
in vec2 v_TexCoord;

uniform sampler2D diffuse_texture;
uniform vec3 diffuse_color;


void main()
{
	vec4 tDiffuse = texture(diffuse_texture, v_TexCoord);

	color = vec4(diffuse_color, 1.0) + tDiffuse;

}
