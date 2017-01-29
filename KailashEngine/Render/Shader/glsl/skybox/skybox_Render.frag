
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


in vec4 current_worldPosition;
in vec4 previous_worldPosition;
in vec3 v_TexCoord;

uniform samplerCube sampler0;		// SkyBox

void main()
{
	vec3 skybox = texture(sampler0, v_TexCoord).xyz;

	diffuse_id = vec4(skybox, 2);
	normal_depth = vec4(vec3(0.0), near_far.y);

	vec2 a = (current_worldPosition.xy / current_worldPosition.w);
	vec2 b = (previous_worldPosition.xy / previous_worldPosition.w);
	vec2 V = (a - b);
	velocity = V;

	gl_FragDepth = 1.0;
}
