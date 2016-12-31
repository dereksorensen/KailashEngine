

out vec4 color;

in vec2 v_TexCoord;

uniform sampler2D sampler0;		// Lighting
uniform sampler2D sampler1;		// Lighting - Specular
uniform sampler2D sampler2;		// Atmospheric Lighting
uniform sampler2D sampler3;		// Diffuse and IDs


void main()
{
	vec3 ambient = vec3(0.0);
	vec3 lighting = texture(sampler0, v_TexCoord).xyz;
	vec3 lighting_specular = texture(sampler1, v_TexCoord).xyz;
	vec4 lighting_atmoshpere = texture(sampler2, v_TexCoord);
	vec4 diffuse_id = texture(sampler3, v_TexCoord);


	int material_id = int(diffuse_id.a);
	vec3 diffuse = diffuse_id.rgb;

	vec3 final = ((lighting + ambient) * diffuse) + lighting_specular;

	// Normal Objects
	// Light Emitters
	// Space
	vec3[] materials = vec3[3](
		final + lighting_atmoshpere.rgb,
		diffuse,
		(diffuse * mix(vec3(0.0), vec3(1.0), max(1.0 - (lighting_atmoshpere.a * 7.0), 0.0))) + lighting_atmoshpere.rgb
	);

	final = materials[material_id];

	color = vec4(final, 1.0);
}
