

out float coc_final;


in vec2 v_TexCoord;

uniform sampler2D sampler0;		// COC Background
uniform sampler2D sampler1;		// COC Foreground


void main()
{
	float coc_background = texture(sampler0, v_TexCoord).r;
	float coc_foreground = texture(sampler1, v_TexCoord).r;

	coc_final = coc_foreground + coc_background;
}

