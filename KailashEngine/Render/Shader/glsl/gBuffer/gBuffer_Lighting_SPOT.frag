
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

//------------------------------------------------------
// Shadow Matrices - Spot
//------------------------------------------------------
layout(std140, binding = 3) uniform shadowMatrices
{
	mat4 mat[64];
};

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









float unpack2(vec2 color)
{
	return color.x + (color.y / 255.0);
}


// Used to get rid of light bleed
float linstep(float min, float max, float v)
{
	return clamp((v-min)/(max-min), 0.0, 1.0);
}


float vsm(vec3 depth)
{
	float bias = 0.0002;
	float distance = depth.z;
	
	vec4 c = texture(sampler2, vec3(depth.xy, shadow_id));	

	vec2 moments;
	moments.x = unpack2(c.xy);


	if(distance <= moments.x-bias)
	{
		return 1.0;
	}

	moments.y = unpack2(c.zw);

	float p = smoothstep(distance-0.0002, distance, moments.x);
	float variance = max(moments.y - moments.x*moments.x, -0.001);
	float d = distance - moments.x;
	float p_max = linstep(0.2,1.0, variance / (variance + d*d));
	return clamp(max(p,p_max), 0.0, 1.0);
}


float calcShadow(vec3 world_position, float depth, vec2 tex_coord)
{
	vec4 shadow_viewPosition = mat[int(shadow_id) * 2 + 1] * vec4(world_position, 1.0);
	vec4 shadow_position = mat[int(shadow_id) * 2] * shadow_viewPosition;
	vec3 shadow_depth = shadow_position.xyz / shadow_position.w;
	shadow_depth = shadow_depth * 0.5 + 0.5;

	float reconDepth = length(shadow_viewPosition.xyz);
	
	shadow_depth.z = reconDepth;

	float visibility = 1.0;
	visibility = vsm(shadow_depth);

	return visibility;
}


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
		visibility = calcShadow(world_position, depth, tex_coord);
	}

	calcLighting(
		tex_coord, 
		world_position, normal_depth.xyz, 
		cam_position,
		light_position, light_color, light_intensity, light_falloff,
		specular_properties,
		L, temp_diffuse, temp_specular);

	float cone = calcSpotLightCone(L, light_spot_angle, light_spot_blur) * visibility;

	diffuse = temp_diffuse * cone;
	specular = temp_specular * cone;

	//diffuse = vec4(world_position, 1.0);
}
