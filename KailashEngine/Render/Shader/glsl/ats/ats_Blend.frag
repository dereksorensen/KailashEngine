

out vec4 color;

in vec2 v_TexCoord;

uniform sampler2D sampler0;		// Atmoshpere

void main()
{
	color = texture(sampler0, v_TexCoord);

}
