
layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec3 tangent;
layout(location = 3) in vec2 texCoord;

//------------------------------------------------------
// perspective and view matrices
//------------------------------------------------------
layout(std140, binding = 0) uniform cameraMatrices
{
	mat4 view;
	mat4 perspective;
};

uniform mat4 model;

out vec4 v_color;
out vec2 v_TexCoord;

void main()
{

	vec4 objectPosition = vec4(position, 1.0);
	vec4 clipPosition = perspective * (view * (model * objectPosition));

	gl_Position = clipPosition;

	v_color =  vec4(0.9,0.3,0.4,1.0);

	v_TexCoord = texCoord;

}
