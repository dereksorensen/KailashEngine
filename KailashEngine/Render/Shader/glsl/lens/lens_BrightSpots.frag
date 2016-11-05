
out vec4 color;

in vec2 v_TexCoord;


uniform sampler2D sampler0;		// Scene



void main()
{
	vec3 scene = texture(sampler0, v_TexCoord).rgb;

	//float Y = dot(scene, vec3(0.299, 0.587, 0.144));
	//vec3 final = scene * 0.4 * smoothstep(0.8, 6.2, Y);

	vec3 final = max(vec3(0.0), scene - 1.5) / 3.5;
	final *= vec3(0.4125, 0.4125, 0.621);

	color = vec4(final, 1.0);
}