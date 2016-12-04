

layout(location = 0) in vec3 position;

//------------------------------------------------------
// Camera Spatials
//------------------------------------------------------
layout(std140, binding = 1) uniform cameraSpatials
{
	mat4 view;
	mat4 perspective;
	mat4 inv_view_perspective;
	mat4 previous_view_persepctive;
	mat4 inv_previous_view_persepctive;
	vec3 cam_position;
	vec3 cam_look;
};


out vec4 ray;
out vec4 current_worldPosition;
out vec4 previous_worldPosition;


const vec2 data[6] = vec2[]
(
  vec2(-1.0, -1.0),
  vec2( 1.0, -1.0),
  vec2(-1.0,  1.0),
  vec2(-1.0, 1.0),
  vec2(1.0, -1.0),
  vec2(1.0,  1.0)
);


void main()
{
	vec4 vPosition = vec4(data[gl_VertexID], 1, 1.0);
	ray = inv_view_perspective * vPosition;

	
	current_worldPosition = perspective * (view * ray);
	previous_worldPosition = previous_view_persepctive * (inv_view_perspective * vPosition);

    gl_Position = vPosition;
}
