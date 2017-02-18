


layout(location = 0) out vec4 diffuse_id;
layout(location = 1) out vec4 normal_depth;
layout(location = 2) out vec4 specular;
layout(location = 3) out vec2 velocity;

in vec2 g_TexCoord;
in vec3 g_worldPosition;
in vec3 g_viewPosition;
in vec3 g_Normal;
in vec3 g_Tangent;
in vec4 g_currentPosition;
in vec4 g_previousPosition;
noperspective in vec3 g_wireframe_distance;


uniform int enable_Wireframe;

uniform int enable_diffuse_texture;
uniform int diffuse_texture_unit;
uniform vec3 diffuse_color;
uniform float emission_strength;

uniform int enable_specular_texture;
uniform int specular_texture_unit;
uniform vec3 specular_color;
uniform float specular_shininess;

uniform int enable_normal_texture;
uniform int normal_texture_unit;

uniform int enable_parallax_texture;
uniform int parallax_texture_unit;



void main()
{
	mat3 TBN = calcTBN(g_Normal, g_Tangent);
	

	//------------------------------------------------------
	// Parallax Mapping
	//------------------------------------------------------
	vec2 tex_coords = g_TexCoord;
	if (enable_parallax_texture == 1)
	{
		tex_coords = calcParallaxMapping(tex[parallax_texture_unit], tex_coords, TBN, cam_position, g_worldPosition);
	}


	//------------------------------------------------------
	// Diffuse Mapping + Material ID
	//------------------------------------------------------
	vec4 diffuse_color_final = vec4(diffuse_color, 1.0);
	if (enable_diffuse_texture == 1)
	{
		diffuse_color_final = texture(tex[diffuse_texture_unit], tex_coords);
	}
	int material_id = 0;
	if (emission_strength > 0)
	{
		material_id = 1;
		diffuse_color_final.xyz *= (emission_strength);
	}
	diffuse_id = vec4(diffuse_color_final.xyz, material_id);

	if(enable_Wireframe == 1)
	{
		// Wireframe
		float near_distance = min(min(g_wireframe_distance[0], g_wireframe_distance[1]), g_wireframe_distance[2]);
		float line_size = 1.0;
		float edgeIntensity1 = exp2(-(1.0/line_size) * near_distance * near_distance);
		line_size = 20.0;
		float edgeIntensity2 = exp2(-(1.0/line_size) * near_distance * near_distance);

		vec3 lineColor_inner = (edgeIntensity1 * vec3(1.0)) + ((1.0 - edgeIntensity1) * vec3(0.0));
		vec3 lineColor_outer = (edgeIntensity2 * vec3(0.0)) + ((1.0 - edgeIntensity2) * diffuse_id.xyz);

		diffuse_id.xyz = lineColor_inner + lineColor_outer;
	}


	//------------------------------------------------------
	// Normal Mapping + Linear Depth
	//------------------------------------------------------
	float depth = length(g_viewPosition);
	normal_depth = vec4(g_Normal, depth);
	if (enable_normal_texture == 1)
	{	
		vec3 normal_map = calcNormalMapping(tex[normal_texture_unit], tex_coords, TBN);
		normal_depth = vec4(normal_map, depth);
	}


	//------------------------------------------------------
	// Specular Mapping
	//------------------------------------------------------
	vec3 specular_color_final = specular_color;
	float specular_shininess_final = max(0.05, 0.9 - (log2(specular_shininess) / 9.0));
	if (enable_specular_texture == 1)
	{
		specular_color_final = texture(tex[specular_texture_unit], tex_coords).xyz;
	}
	specular = vec4(specular_color_final, specular_shininess_final);


	//------------------------------------------------------
	// Velocity Mapping
	//------------------------------------------------------

	vec2 a = (g_currentPosition.xy / g_currentPosition.w);
	vec2 b = (g_previousPosition.xy / g_previousPosition.w);
	vec2 V = (a - b);
	velocity = V;

}
