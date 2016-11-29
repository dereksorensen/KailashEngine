
layout(location = 0) out vec4 diffuse;
layout(location = 1) out vec4 specular;

in vec3 v_viewRay;


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
	mat4 inv_view_perspective;
	mat4 previous_view_persepctive;
	mat4 inv_previous_view_persepctive;
	vec3 cam_position;
	vec3 cam_look;
};


uniform sampler2D sampler0;		// Normal & Depth
uniform sampler2D sampler1;		// Specular


uniform vec3 light_position;
uniform vec3 light_direction;
uniform vec3 light_color;
uniform float light_intensity;
uniform float light_falloff;
uniform float light_spot_angle;
uniform float light_spot_blur;



float calcSpotLightCone(vec3 L, float outer_angle, float blur_amount)
{
	// Amount to blur the edge of the cone
	float spot_blur = blur_amount * (outer_angle / MATH_HALF_PI);

	// Add tiny bit to outer angle so it's rounded
	float spotAngle_outer = outer_angle - 0.01;
	float spotAngle_inner = spotAngle_outer + spot_blur;

	float spotAngle = acos(dot(light_direction,-L));

	float spotAngleDifference = spotAngle_inner - spotAngle_outer;
	float spotLightBlur = (-spotAngle + spotAngle_outer) / spotAngleDifference;

	return clamp(spotLightBlur,0.0,1.0);
}



void main()
{

	//Calculate Texture Coordinates
	vec2 Resolution = textureSize(sampler0, 0);
	vec2 tex_coord = gl_FragCoord.xy / Resolution;

	vec4 normal_depth = texture(sampler0, tex_coord);
	float depth = normal_depth.a;

	vec3 world_position = calcWorldPosition(depth, v_viewRay, cam_position);

	vec4 specular_properties = texture(sampler1, tex_coord);

	vec3 L;
	vec4 temp_diffuse;
	vec4 temp_specular;


	calcLighting(
		tex_coord, 
		world_position, normal_depth.xyz, 
		cam_position,
		light_position, light_color, light_intensity, light_falloff,
		specular_properties,
		L, temp_diffuse, temp_specular);

	float cone = calcSpotLightCone(L, light_spot_angle, light_spot_blur);

	diffuse = temp_diffuse * cone;
	specular = temp_specular * cone;

	//diffuse = vec4(depth);
}
