
layout(location = 0) out vec4 diffuse_id;
layout(location = 1) out vec4 normal_depth;
layout(location = 2) out vec2 velocity;

//------------------------------------------------------
// Game Config
//------------------------------------------------------
layout(std140, binding = 0) uniform gameConfig
{
	vec4 near_far;
	float target_fps;
};

in vec4 ray;
in vec4 current_worldPosition;
in vec4 previous_worldPosition;


uniform samplerCube sampler0;		// SkyBox
uniform vec3 circadian_position;

void main()
{
	vec3 skybox = texture(sampler0, reflect(ray.xyz, circadian_position)).xyz;

	diffuse_id = vec4(skybox, 1.0);
	normal_depth = vec4(vec3(0.0), near_far.y);
	velocity = (current_worldPosition.xy / current_worldPosition.w - previous_worldPosition.xy / previous_worldPosition.w) / 90.0;

	//diffuse_id = vec4(ray_previous.xy / ray_previous.w, 0.0, 1.0);

	gl_FragDepth = 1.0;
}
