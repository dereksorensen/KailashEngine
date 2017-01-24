

out float coc_final;


in vec2 v_TexCoord;

uniform sampler2D sampler0;		// COC Background
uniform sampler2D sampler1;		// COC Foreground


void main()
{
	float coc_background = texture(sampler0, v_TexCoord).r;
	float coc_foreground = texture(sampler1, v_TexCoord).r;

	float coc_signed = 0.0;
	if(coc_foreground > coc_background)
	{
		coc_signed = -coc_foreground;
	}
	else
	{
		coc_signed = coc_background;
	}

	coc_final = max(coc_foreground, coc_background);
}

