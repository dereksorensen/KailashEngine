
// Sourced from:
// https://rauwendaal.net/2014/06/

layout(location = 0) in vec3 position;

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

out vec2 v_TexCoord;
out vec3 ray;

const vec2 data[3] = vec2[]
(
  vec2(-1.0, -1.0),
  vec2( 3.0, -1.0),
  vec2(-1.0,  3.0)
);

void main()
{
	
	float x = -1.0 + float((gl_VertexID & 1) << 2);
    float y = -1.0 + float((gl_VertexID & 2) << 1);

    v_TexCoord.x = (x+1.0)*0.5;
    v_TexCoord.y = (y+1.0)*0.5;

	vec4 vPosition = vec4(data[gl_VertexID].x * 1.6, data[gl_VertexID].y, 0, 1.0);
	vec4 finalPosition = inverse(perspective) * vPosition;
	ray = normalize((inverse(view) * vec4(finalPosition.xyz, 1.0)).xyz);

    gl_Position = vec4(data[gl_VertexID], 0, 1);
	
}
