
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
float PCF_NUM_SAMPLES = 16;
float pcss_blur = 100.0;

float PenumbraSize(float zReceiver, float zBlocker) //Parallel plane estimation 
{ 
	return ((zReceiver - zBlocker) * pcss_blur) / zBlocker; 
}
 
void FindBlocker(out float avgBlockerDepth, 
					out float numBlockers, 
					vec2 uv, float zReceiver ) 
{ 
	//This uses similar triangles to compute what 
	//area of the shadow map we should search 

	vec2 searchWidth = (light_size / textureSize(sampler2, 0).xy) * (zReceiver - 0.1f) / zReceiver;
	


	float blockerSum = 0; 
	numBlockers = 0; 

	
	for( int i = 0; i < PCF_NUM_SAMPLES; ++i ) 
	{ 
		vec2 shadowDepth = texture(sampler2, vec3(uv + (poissonDisk[i] * searchWidth), shadow_id)).xy;
		float shadowMapDepth = unpack2(shadowDepth.xy);

		if ( shadowMapDepth < zReceiver-0.0003)
		{
			blockerSum += shadowMapDepth; 
			numBlockers++; 
		} 
	}

	if ( numBlockers != 0 )
	{
		avgBlockerDepth = blockerSum / numBlockers; 
	}

}





// Used to get rid of light bleed
float linstep(float min, float max, float v)
{
	return clamp((v-min)/(max-min), 0.0, 1.0);
}


float vsm(vec3 depth, float blur_amount)
{
	float bias = 0.00001;
	float distance = depth.z;
	
	vec4 c = textureLod(sampler2, vec3(depth.xy, shadow_id), max(0.0, blur_amount / 5.0));	

	vec2 moments;
	moments.x = unpack2(c.xy);


	if(distance <= moments.x-bias)
	{
		return 1.0;
	}

	moments.y = unpack2(c.zw);

	float p = smoothstep(distance - bias, distance, moments.x);
	float variance = max(moments.y - moments.x*moments.x, -0.001);
	float d = distance - moments.x;
	float p_max = linstep(0.5,1.0, variance / (variance + d*d));
	return clamp(max(p,p_max), 0.0, 1.0);
}


float PCSS ( vec3 coords ) 
{ 
	vec2 uv = coords.xy;
	float zReceiver = coords.z; // Assumed to be eye-space z in this code
 
	// STEP 1: blocker search
	float avgBlockerDepth = 0;
	float numBlockers = 0;
	FindBlocker( avgBlockerDepth, numBlockers, uv, zReceiver );
 
	if( numBlockers < 1 )
	{
		//There are no occluders so early out (this saves filtering) 
		return 1.0f;
	}
 
	// STEP 2: penumbra size
	float filterRadiusUV = PenumbraSize(zReceiver, avgBlockerDepth);
	filterRadiusUV = filterRadiusUV * (light_size) * 0.1 / coords.z; 

	//return avgBlockerDepth * 13.0;

	// STEP 3: filtering
	//return PCF_Filter(uv, zReceiver, filterRadiusUV );
	return vsm(coords, filterRadiusUV);

	//return texture(sampler3, uv);
} 








float calcShadow(vec3 world_position, float depth, vec2 tex_coord)
{
	vec4 shadow_viewPosition = shadow_data[shadow_id].view * vec4(world_position, 1.0);
	vec4 shadow_position = shadow_data[shadow_id].perspective * shadow_viewPosition;
	vec3 shadow_depth = shadow_position.xyz / shadow_position.w;
	shadow_depth = shadow_depth * 0.5 + 0.5;

	float reconDepth = length(shadow_viewPosition.xyz);
	
	shadow_depth.z = reconDepth;

	float visibility = 1.0;
	//visibility = vsm(shadow_depth);
	visibility = PCSS(shadow_depth);

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
