

layout(location = 0) in vec3 position;


out vec4 current_worldPosition;
out vec4 previous_worldPosition;
out vec3 v_TexCoord;


uniform vec3 circadian_position;

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
	vec4 ray = inv_view_perspective * vPosition;
	v_TexCoord = reflect(ray.xyz, circadian_position);
	
	current_worldPosition = perspective * (view * ray);
	previous_worldPosition = previous_view_persepctive * (inv_view_perspective * vPosition);

    gl_Position = vPosition;
}
