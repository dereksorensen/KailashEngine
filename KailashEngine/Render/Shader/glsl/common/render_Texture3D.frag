

out vec4 color;
in vec2 v_TexCoord;

uniform samplerCube sampler0;
uniform int layer;

void main()
{	
	vec4 tex = texture(sampler0, vec3(v_TexCoord, layer));
	color = tex;
}
