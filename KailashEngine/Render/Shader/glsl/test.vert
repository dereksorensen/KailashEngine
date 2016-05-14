
layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec3 tangent;
layout(location = 3) in vec2 texCoord;

uniform mat4 view;
uniform mat4 perspective;

void main()
{

	vec4 objectPosition = vec4(position, 1.0);
	vec4 clipPosition = perspective * (view * objectPosition);

	gl_Position = clipPosition;

}
