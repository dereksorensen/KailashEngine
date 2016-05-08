
//Reusable vertex shader to show a texture full screen

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texCoord;

out vec2 vTexCoord;

uniform mat4 model;


void main()
{

	vec4 finalPosition = vec4(position, 1.0);
	gl_Position = finalPosition;
	//vTexCoord = (finalPosition.xy + vec2(1.0,1.0))/vec2(2.0,2.0);
	vTexCoord = texCoord;

}
