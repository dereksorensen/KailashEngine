
layout(location = 0) out vec4 diffuse_id;
layout(location = 1) out vec4 normal_depth;

//------------------------------------------------------
// Game Config
//------------------------------------------------------
layout(std140, binding = 0) uniform gameConfig
{
	vec4 near_far;
	float target_fps;
};

in vec3 ray;

uniform samplerCube sampler0;		// SkyBox
uniform vec3 circadian_position;

void main()
{
	vec3 skybox = texture(sampler0, reflect(ray, circadian_position)).xyz;

	diffuse_id = vec4(skybox, 1.0);
	normal_depth = vec4(vec3(0.0), near_far.y);
	gl_FragDepth = 1.0;
}
