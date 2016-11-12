
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

void main()
{
	diffuse_id = texture(sampler0, ray);
	normal_depth = vec4(vec3(0.0), near_far.y);
	gl_FragDepth = 1.0;
}
