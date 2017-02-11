
layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec3 tangent;
layout(location = 3) in vec2 texCoord;

out vec3 v_viewRay;


uniform mat4 model;

void main()
{
	// Position
	vec4 worldPosition = model * vec4(position, 1.0);
	vec4 viewPosition = view * worldPosition;
	vec4 clipPosition = perspective * viewPosition;
	gl_Position = clipPosition;

	v_viewRay = worldPosition.xyz + cam_position;
}
