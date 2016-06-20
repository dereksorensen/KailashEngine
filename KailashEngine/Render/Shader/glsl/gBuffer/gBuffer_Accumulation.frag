

out vec4 color;

in vec2 v_TexCoord;

uniform sampler2D sampler0;		// Lighting
uniform sampler2D sampler1;		// Lighting - Specular
uniform sampler2D sampler2;		// Diffuse and IDs


void main()
{
	vec3 ambient = vec3(0.0);
	vec3 lighting = texture(sampler0, v_TexCoord).xyz;
	vec3 lighting_specular = texture(sampler1, v_TexCoord).xyz;
	vec4 diffuse_id = texture(sampler2, v_TexCoord);

	int material_id = int(diffuse_id.a);
	vec3 diffuse = diffuse_id.rgb;


	vec3 final = ((lighting + ambient) * diffuse) + lighting_specular;


	vec3[] materials = vec3[2](
		final, 
		diffuse
	);

	final = materials[material_id];


	color = vec4(final, 1.0);

}
