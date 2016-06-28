
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





float calcSpotLightCone(vec3 L, float outerAngle, float blurAmount)
{

	//Blur the spotlight based on distance
	float spotLightBlurCoefficient = blurAmount;
	float spotAngleCutoff_outer = outerAngle * MATH_PI / 180.0;
	float spotAngleCutoff_inner = spotAngleCutoff_outer+(spotLightBlurCoefficient);

	float spotAngle = dot(normalize(vec3(-light_direction.x, light_direction.y, -light_direction.z)),-L);

	float spotAngleDifference = spotAngleCutoff_inner-spotAngleCutoff_outer;
	float spotLightBlur = (spotAngle - spotAngleCutoff_outer)/spotAngleDifference;
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

	float cone = calcSpotLightCone(L, 47.5, 0.02);

	diffuse = temp_diffuse * cone;
	specular = temp_specular * cone;

	//diffuse = vec4(depth);
}
