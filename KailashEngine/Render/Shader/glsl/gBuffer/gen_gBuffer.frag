

layout(location = 0) out vec4 diffuse_id;
layout(location = 1) out vec4 normal_depth;
layout(location = 2) out vec4 specular;


in vec2 v_TexCoord;
in vec4 v_objectPosition;
in vec3 v_worldPosition;
in vec3 v_Normal;
in vec3 v_Tangent;


uniform sampler2D diffuse_texture;
uniform vec3 diffuse_color;

uniform int enable_specular_texture;
uniform sampler2D specular_texture;
uniform vec3 specular_color;
uniform float specular_shininess;

uniform int enable_normal_texture;
uniform sampler2D normal_texture;

uniform sampler2D displacement_texture;

uniform int enable_parallax_texture;
uniform sampler2D parallax_texture;



void main()
{
	//------------------------------------------------------
	// Diffuse Mapping + Material ID
	//------------------------------------------------------
	vec4 tDiffuse = texture(diffuse_texture, v_TexCoord);
	vec4 temp_color = vec4(diffuse_color, 1.0) + tDiffuse;

	float gamma = 1.8;
	vec3 gamma_correction = vec3(1.0 / gamma);
	vec3 final = pow(temp_color.xyz, gamma_correction);

	diffuse_id = vec4(final.xyz, 1.0);


	//------------------------------------------------------
	// Normal Mapping + Linear Depth
	//------------------------------------------------------
	normal_depth = vec4(v_Normal, 0.0);
	if (enable_normal_texture == 1)
	{
		vec3 normal_map = calcNormalMapping(normal_texture, v_TexCoord, v_Normal, v_Tangent);
		normal_depth = vec4(normal_map,1.0);
	}


	//------------------------------------------------------
	// Specular Mapping
	//------------------------------------------------------	
	vec3 specular_color_final = specular_color;
	if (enable_specular_texture == 1)
	{
		specular_color_final = texture(specular_texture, v_TexCoord).xyz;
	}
	specular = vec4(specular_color_final, specular_shininess);


}
