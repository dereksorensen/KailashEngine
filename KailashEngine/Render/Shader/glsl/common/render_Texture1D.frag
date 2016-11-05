

out vec4 color;

in vec2 v_TexCoord;


uniform sampler1D sampler0;


void main()
{
	
	vec4 tex = texture(sampler0, v_TexCoord.x);

	color = tex;

}
