

out vec4 color;


//------------------------------------------------------
// perspective and view matrices
//------------------------------------------------------
layout(std140, binding = 1) uniform cameraMatrices
{
	mat4 view;
	mat4 perspective;
	vec3 cam_position;
};


uniform vec3 light_position;
uniform vec3 light_direction;
uniform vec3 light_color;
uniform float light_intensity;
uniform float light_falloff;


void main()
{
	
	color = vec4(0.5);

}
