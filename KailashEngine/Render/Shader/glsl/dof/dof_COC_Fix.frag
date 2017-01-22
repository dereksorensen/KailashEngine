

out float coc_foreground_fixed;


in vec2 v_TexCoord;


uniform sampler2D sampler0;		// COC
uniform sampler2D sampler1;		// COC Blurred



void main()
{
	float coc_foreground = texture(sampler0, v_TexCoord).r;
	float coc_foreground_blurred = texture(sampler1, v_TexCoord).r;

	coc_foreground_fixed = 2 * max(coc_foreground_blurred, coc_foreground) - coc_foreground;
}

