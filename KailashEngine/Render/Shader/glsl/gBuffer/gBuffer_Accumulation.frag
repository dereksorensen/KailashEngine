

out vec4 color;

in vec2 v_TexCoord;

uniform sampler2D sampler0;		// Lighting
uniform sampler2D sampler1;		// Diffuse and IDs


void main()
{
	vec3 ambient = vec3(0.0);
	vec3 lighting = texture(sampler0, v_TexCoord).xyz;
	vec4 diffuse_id = texture(sampler1, v_TexCoord);

	float material_id = diffuse_id.a;
	vec3 diffuse = diffuse_id.rgb;


	vec3 final = ((lighting + ambient) * diffuse);

	color = vec4(final, 1.0);

}
