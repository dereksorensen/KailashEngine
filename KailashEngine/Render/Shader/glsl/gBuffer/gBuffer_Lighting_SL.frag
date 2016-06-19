

out vec4 color;


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



vec3 calcWorldPosition(float depth, vec3 view_ray, vec3 cam_position)
{
	view_ray = normalize(view_ray);
	return view_ray * depth - cam_position;
}

vec3 calcViewPosition(float depth, vec3 view_ray, vec3 cam_position, mat4 view_matrix)
{
	vec3 world_position = calcWorldPosition(depth, view_ray, cam_position);
	return (view_matrix * vec4(world_position, 1.0)).xyz;
}


float calcSpotLightCone(vec3 L, float outerAngle, float blurAmount)
{

	//Blur the spotlight based on distance
	float spotLightBlurCoefficient = blurAmount;
	float spotAngleCutoff_outer = outerAngle;
	float spotAngleCutoff_inner = spotAngleCutoff_outer+(spotLightBlurCoefficient);

	float spotAngle = dot(normalize(-light_direction),-L);
	//spotAngle = max(spotAngle,0);

	float spotAngleDifference = spotAngleCutoff_inner-spotAngleCutoff_outer;
	float spotLightBlur = (spotAngle - spotAngleCutoff_outer)/spotAngleDifference;
	return clamp(spotLightBlur,0.0,1.0) * 2.0;
}


vec4 calcLighting(vec2 tex_coord, vec3 world_position, vec3 world_normal)
{
	
	//------------------------------------------------------
	// Attenuation
	//------------------------------------------------------
	vec3 L = vec3(0.0);
	vec3 N = normalize(world_normal);
	vec3 E = normalize(-cam_position - world_position);



	//------------------------------------------------------
	// Attenuation
	//------------------------------------------------------
	float max_light_brightness = 10.0;
	float light_bright = (light_intensity / max_light_brightness);
	float light_falloff_2 = light_falloff * light_falloff * 4.0;

	vec3 light_distance = (light_position - world_position);
	float light_distance_2 = dot(light_distance, light_distance);
	L = light_distance * inversesqrt(light_distance_2);

	float attenuation = clamp(1.0 - light_distance_2 / light_falloff_2, 0.0, 1.0);
	attenuation *= attenuation;
	//attenuation *= light_bright;


	//------------------------------------------------------
	// Angle of Inclination
	//------------------------------------------------------
	float angle_of_inc = dot(N,L);
	

	//------------------------------------------------------
	// Diffuse
	//------------------------------------------------------
	vec4 diffuse = vec4(light_color * max(angle_of_inc, 0.0), 1.0);


	//------------------------------------------------------
	// Specular
	//------------------------------------------------------
	vec4 specular_properties = texture(sampler1, tex_coord);
	float specular_shininess = specular_properties.a;
	vec3 specular_color = specular_properties.xyz;

	vec3 half_angle = normalize(L + E);

	float angleNormalHalf = acos(dot(half_angle,N));
	float exponent = angleNormalHalf * (specular_shininess * 5.0);
	exponent = -(exponent * exponent);
	float gaussianTerm = exp(exponent);

	gaussianTerm = angle_of_inc != 0.0 ? gaussianTerm : 0.0;

	vec4 specular = vec4(light_color *
					gaussianTerm *
					specular_color, 1.0);


	//------------------------------------------------------
	// Add it all together
	//------------------------------------------------------
	vec4 lighting = 
		attenuation * (
			diffuse +
			specular
		);

	float cone = calcSpotLightCone(L, 0.85, 0.02);

	return lighting * cone;
}


void main()
{

	//Calculate Texture Coordinates
	vec2 Resolution = textureSize(sampler0, 0);
	vec2 tex_coord = gl_FragCoord.xy / Resolution;

	vec4 normal_depth = texture(sampler0, tex_coord);
	float depth = normal_depth.a;

	vec3 world_position = calcWorldPosition(depth, v_viewRay, cam_position);




	vec4 lighting = calcLighting(tex_coord, world_position, normal_depth.xyz);


	color = lighting;

}
