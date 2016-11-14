

out float coc_fixed;



in vec2 v_TexCoord;

uniform sampler2D sampler0;		// COC Blurred
uniform sampler2D sampler1;		// COC



void main()
{
	float coc_blurred = texture(sampler0, v_TexCoord).r;
	float coc = texture(sampler1, v_TexCoord).r;

	coc_fixed = 2 * max( coc_blurred, coc ) - coc;
}

