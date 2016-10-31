
out vec4 color;

in vec2 v_TexCoord;


uniform sampler2D sampler0;		// Scene



void main()
{
	vec3 scene = texture(sampler0, v_TexCoord).rgb;

	vec3 final = max(vec3(0.0), scene - 1.5) / 3.5;
	final *= vec3(0.4125, 0.4125, 0.621);

	color = vec4(final, 1.0);
}