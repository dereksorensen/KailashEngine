
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
struct ShadowData {
  mat4 view;
  mat4 perspective;
  vec4 light_position;
};
layout(std140, binding = 3) uniform shadowMatrices
{
	ShadowData shadow_data[32];
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





const vec2 poissonDisk[16] = vec2[16](
    vec2( -0.94201624, -0.39906216 ), 
    vec2( 0.94558609, -0.76890725 ), 
    vec2( -0.094184101, -0.92938870 ), 
    vec2( 0.34495938, 0.29387760 ), 
    vec2( -0.91588581, 0.45771432 ), 
    vec2( -0.81544232, -0.87912464 ), 
    vec2( -0.38277543, 0.27676845 ), 
    vec2( 0.97484398, 0.75648379 ), 
    vec2( 0.44323325, -0.97511554 ), 
    vec2( 0.53742981, -0.47373420 ), 
    vec2( -0.26496911, -0.41893023 ), 
    vec2( 0.79197514, 0.19090188 ), 
    vec2( -0.24188840, 0.99706507 ), 
    vec2( -0.81409955, 0.91437590 ), 
    vec2( 0.19984126, 0.78641367 ), 
    vec2( 0.14383161, -0.14100790 ) 
 );
 

float unpack2(vec2 color)
{
	return color.x + (color.y / 255.0);
}



float light_size = 91.0;
float pcss_blur = 100.0;


//------------------------------------------------------
// Shadow Map Evaluators
//------------------------------------------------------

vec2 getMoments(sampler2DArray shadow_depth_sampler, int shadow_depth_id, vec2 tex_coord, float mip_level)
{
	vec4 moments_packed =  textureLod(shadow_depth_sampler, vec3(tex_coord, shadow_depth_id), max(0.5, mip_level));
	vec2 moments;
	moments.x = unpack2(moments_packed.xy);
	moments.y = unpack2(moments_packed.zw);

	return moments;
}

// Used to get rid of light bleed
float linstep(float min, float max, float v)
{
	return clamp((v-min)/(max-min), 0.0, 1.0);
}

float vsm(vec2 moments, float shadow_depth, float min_variance)
{
	float bias = 0.00001;
	float distance = shadow_depth;
	float bleed_factor = 0.7;
	
	if(distance <= moments.x-bias)
	{
		return 1.0;
	}

	float p = smoothstep(distance - bias, distance, moments.x);
	float variance = max(moments.y - moments.x*moments.x, min_variance);
	float d = distance - moments.x;
	float p_max = variance / (variance + d*d);
	p_max = linstep(bleed_factor, 1.0, p_max);
	return clamp(max(p,p_max), 0.0, 1.0);
}


float esm(float exp_d, float compare, float bleed_factor)
{
	float bias = 0.1;
	
	/*
	if(compare <= exp_d-bias)
	{
		return 1.0;
	}
	*/

	float c = bleed_factor;
    float depth = exp(c * (exp_d)) * exp(-c * (compare));
    return clamp(depth, 0.0, 1.0);
}


vec2 g_EVSMExponents = vec2(40.0, 30.0);
float g_EVSM_Derivation = 0.0001f;
vec2 warpDepth(float depth)
{
    depth = 2.0f * depth - 1.0f;
    float pos =  exp( g_EVSMExponents.x * depth);
    float neg = -exp(-g_EVSMExponents.y * depth);
    return vec2(pos, neg);
}


float evsm(vec3 shadow_uv_depth)
{
	float ref_depth = calcLinearDepth(shadow_uv_depth.z, 0.1, 100.0);
	vec2 warped_ref_depth = warpDepth(ref_depth);
	float mip_level = 2.0;

	vec4 moments =  textureLod(sampler2, vec3(shadow_uv_depth.xy, shadow_id), max(0.5, mip_level));

	// Derivative of warping at depth
    vec2 depthScale = g_EVSM_Derivation * g_EVSMExponents * warped_ref_depth;
    vec2 minVariance = depthScale * depthScale;

	// Compute the upper bounds of the visibility function both for x and y
    float posContrib = vsm(moments.xz, warped_ref_depth.x, minVariance.x);
    float negContrib = vsm(moments.yw, warped_ref_depth.y, minVariance.y);

    return min(posContrib, negContrib);
}


//------------------------------------------------------
// PCSS
//------------------------------------------------------


// Based on http://www.derschmale.com/2014/07/24/faster-variance-soft-shadow-mapping-for-varying-penumbra-sizes/
float getAverageOccluderDepth(float light_size, float shadow_map_size, vec3 uv_depth) 
{
    // calculate the mip level corresponding to the search area
    // Really, mipLevel would be a passed in as a constant.
    float mip_level = log2(int(light_size * shadow_map_size));

	vec2 moments = getMoments(sampler2, shadow_id, uv_depth.xy, mip_level);
	float average_depth = moments.x;
    float probability = vsm(moments, uv_depth.z, 0.001);
	//probability = esm(average_depth, uv_depth.z, 0.01);

    // prevent numerical issues
    if (probability > .99) return 0.0;

    // calculate the average occluder depth
    return (average_depth - probability * uv_depth.z) / (1.0 - probability);
}

// Based on http://www.derschmale.com/2014/07/24/faster-variance-soft-shadow-mapping-for-varying-penumbra-sizes/
float estimatePenumbraSize(float light_size, float shadow_map_size, vec3 uv_depth, float penumbra_falloff)
{
    // the search area covers twice the light size
    float averageOccluderDepth = getAverageOccluderDepth(light_size, shadow_map_size, uv_depth);
    float penumbra_size = light_size * (uv_depth.z - averageOccluderDepth) * penumbra_falloff;

    // clamp to the maximum softness, which matches the search area
    return min(penumbra_size, light_size);
}


float PCSS(vec3 uv_depth, float light_size, float penumbra_falloff) 
{ 
	vec2 shadow_map_size = textureSize(sampler2, 0).xy;
	float average_map_size = (shadow_map_size.x + shadow_map_size.y) / 2.0;

	float penumbra_size = estimatePenumbraSize(light_size, average_map_size, uv_depth, penumbra_falloff);

	return vsm(getMoments(sampler2, shadow_id, uv_depth.xy, penumbra_size), uv_depth.z, 0.001);
	//return esm(getMoments(sampler2, shadow_id, uv_depth.xy, penumbra_size).x, uv_depth.z, 0.5);
}


//------------------------------------------------------
// Evaluate Shadow
//------------------------------------------------------

float calcShadow(vec3 world_position, float depth, vec2 tex_coord)
{
	vec4 shadow_viewPosition = shadow_data[shadow_id].view * vec4(world_position, 1.0);
	vec4 shadow_position = shadow_data[shadow_id].perspective * shadow_viewPosition;
	vec3 shadow_uv_depth = shadow_position.xyz / shadow_position.w;
	shadow_uv_depth = shadow_uv_depth * 0.5 + 0.5;
	
	shadow_uv_depth.z = length(shadow_viewPosition.xyz);

	float visibility = 1.0;
	//visibility = vsm(getMoments(sampler2, shadow_id, shadow_uv_depth.xy, 1.0), shadow_uv_depth.z, 0.001);
	//visibility = esm(getMoments(sampler2, shadow_id, shadow_uv_depth.xy, 4.0).x, shadow_uv_depth.z, 0.5);
	//visibility = PCSS(shadow_uv_depth, 10.0, 0.03);
	visibility = evsm(shadow_uv_depth);

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
