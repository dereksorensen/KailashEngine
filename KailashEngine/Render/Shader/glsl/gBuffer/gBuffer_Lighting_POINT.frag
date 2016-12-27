

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
// Shadow Matrices - Point
//------------------------------------------------------
struct ShadowData {
  mat4 view[6];
  mat4 perspective;
  vec3 light_position;
};
layout(std140, binding = 4) uniform shadowMatrices
{
	ShadowData shadow_data[32];
};


uniform sampler2D sampler0;		// Normal & Depth
uniform sampler2D sampler1;		// Specular
uniform samplerCubeArray sampler2;		// Shadow Depth

uniform vec3 light_position;
uniform vec3 light_color;
uniform float light_intensity;
uniform float light_falloff;

uniform int shadow_id;


void main()
{
	
	//Calculate Texture Coordinates
	vec2 Resolution = textureSize(sampler0, 0);
	vec2 tex_coord = gl_FragCoord.xy / Resolution;

	vec4 normal_depth = texture(sampler0, tex_coord);
	float depth = normal_depth.a;

	vec3 world_position = calcWorldPosition(depth, v_viewRay, cam_position);

	vec4 specular_properties = texture(sampler1, tex_coord);
	//specular_properties.a = 1.0;

	vec3 L;
	vec4 temp_diffuse;
	vec4 temp_specular;

	float visibility = 1.0;
	if (shadow_id != -1)
	{
		visibility = calcShadow(
			sampler2, shadow_id, 
			world_position, light_position,
			10.0, 0.03);
	}

	calcLighting(
		tex_coord, 
		world_position, normal_depth.xyz, 
		cam_position,
		light_position, light_color, light_intensity, light_falloff,
		specular_properties,
		L, temp_diffuse, temp_specular);


	diffuse = temp_diffuse * visibility;
	specular = temp_specular * visibility;

	//diffuse = vec4(world_position, 1.0);
}
