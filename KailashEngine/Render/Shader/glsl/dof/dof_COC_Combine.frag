

out float coc_final;


in vec2 v_TexCoord;

uniform sampler2D sampler0;		// COC Background
uniform sampler2D sampler1;		// COC Foreground
uniform sampler2D sampler2;		// COC Foreground Blurred


void main()
{
	float coc_background = texture(sampler0, v_TexCoord).r;
	float coc_foreground = texture(sampler1, v_TexCoord).r;
	float coc_foreground_blurred = texture(sampler2, v_TexCoord).r;

	float coc_foreground_fixed = 2 * max(coc_foreground_blurred, coc_foreground) - coc_foreground;

	coc_final = max(coc_foreground_fixed, coc_background);

}

