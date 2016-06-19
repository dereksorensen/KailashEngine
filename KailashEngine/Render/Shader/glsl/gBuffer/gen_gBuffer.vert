
layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec3 tangent;
layout(location = 3) in vec2 texCoord;

out vec2 v_TexCoords;
out vec4 v_objectPosition;
out vec3 v_worldPosition;
out vec3 v_viewPosition;
out vec3 v_Normal;
out vec3 v_Tangent;

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

uniform mat4 model;
uniform mat4 model_normal;


void main()
{

	// Position
	v_objectPosition = vec4(position, 1.0);
	vec4 world_position = (model * v_objectPosition);
	v_worldPosition = world_position.xyz;
	vec4 viewPosition = view *  world_position;
	v_viewPosition = viewPosition.xyz;
	vec4 clipPosition = perspective * viewPosition;
	gl_Position = clipPosition;

	// Texture Coordinates
	v_TexCoords = texCoord;

	// Put vertex normal in world space
	v_Normal = normalize((model_normal * vec4(normal, 0.0)).xyz);
	v_Tangent = (model_normal * vec4(tangent, 0.0)).xyz;

}
