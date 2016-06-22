

layout(location = 0) out vec4 diffuse_id;
layout(location = 1) out vec4 normal_depth;
layout(location = 2) out vec4 specular;

in vec2 v_TexCoords;
in vec4 v_objectPosition;
in vec3 v_worldPosition;
in vec3 v_viewPosition;
in vec3 v_Normal;
in vec3 v_Tangent;


//------------------------------------------------------
// Game Config
//------------------------------------------------------
layout(std140, binding = 0) uniform gameConfig
{
	vec4 near_far;
	float target_fps;
};

//------------------------------------------------------
// Camera Spatials
//------------------------------------------------------
layout(std140, binding = 1) uniform cameraSpatials
{
	mat4 view;
	mat4 perspective;
	vec3 cam_position;
	vec3 cam_look;
};


uniform int enable_diffuse_texture;
uniform sampler2D diffuse_texture;
uniform vec3 diffuse_color;
uniform float emission_strength;

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
	mat3 TBN = calcTBN(v_Normal, v_Tangent);
	


	//------------------------------------------------------
	// Parallax Mapping
	//------------------------------------------------------
	vec2 tex_coords = v_TexCoords;
	if (enable_parallax_texture == 1)
	{
		tex_coords = calcParallaxMapping(parallax_texture, tex_coords, TBN, cam_position, v_worldPosition);
	}

	//------------------------------------------------------
	// Diffuse Mapping + Material ID
	//------------------------------------------------------
	vec4 diffuse_color_final = vec4(diffuse_color, 1.0);
	if (enable_diffuse_texture == 1)
	{
		diffuse_color_final = texture(diffuse_texture, tex_coords);
	}
	int material_id = 0;
	if (emission_strength > 0)
	{
		material_id = 1;
		diffuse_color_final.xyz *= emission_strength;
	}
	diffuse_id = vec4(diffuse_color_final.xyz, material_id);


	//------------------------------------------------------
	// Normal Mapping + Linear Depth
	//------------------------------------------------------
	float depth = length(v_viewPosition);
	normal_depth = vec4(v_Normal, depth);
	if (enable_normal_texture == 1)
	{	
		vec3 normal_map = calcNormalMapping(normal_texture, tex_coords, TBN);
		normal_depth = vec4(normal_map, depth);
	}


	//------------------------------------------------------
	// Specular Mapping
	//------------------------------------------------------	
	vec3 specular_color_final = specular_color;
	float specular_shininess_final = specular_shininess / 511.0;
	if (enable_specular_texture == 1)
	{
		specular_color_final = texture(specular_texture, tex_coords).xyz;
	}
	specular = vec4(specular_color_final, specular_shininess_final);

}
