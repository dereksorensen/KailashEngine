
layout(location = 0) out vec4 diffuse;
layout(location = 1) out vec4 specular;

in vec3 v_viewRay;



uniform sampler2D sampler0;		// Normal & Depth
uniform sampler2D sampler1;		// Specular
uniform sampler2DArray sampler2;		// Shadow Depth

uniform vec3 light_position;
uniform vec3 light_direction;
uniform vec3 light_color;
uniform float light_intensity;
uniform float light_falloff;
uniform float light_spot_angle;
uniform float light_spot_blur;

uniform int shadow_id;


void main()
{
	// Calculate Texture Coordinates
	vec2 resolution = textureSize(sampler0, 0);
	vec2 tex_coord = gl_FragCoord.xy / resolution;

	vec4 normal_depth = texture(sampler0, tex_coord);
	float depth = normal_depth.a;

	vec3 world_position = calcWorldPosition(depth, v_viewRay, cam_position);

	vec4 specular_properties = texture(sampler1, tex_coord);

	vec3 L;
	vec4 temp_diffuse;
	vec4 temp_specular;

	float visibility = 1.0;
	if (shadow_id != -1)
	{
		visibility = calcShadow(
			sampler2, shadow_id, 
			shadow_data_spot[shadow_id].view, shadow_data_spot[shadow_id].perspective, world_position, 
			10.0, 0.03);
	}

	calcLighting(
		tex_coord, 
		world_position, normal_depth.xyz, 
		cam_position,
		light_position, light_color, light_intensity, light_falloff,
		specular_properties,
		L, temp_diffuse, temp_specular);

	visibility *= calcSpotLightCone(L, light_direction, light_spot_angle, light_spot_blur);

	diffuse = temp_diffuse * visibility;
	specular = temp_specular * visibility;
}
