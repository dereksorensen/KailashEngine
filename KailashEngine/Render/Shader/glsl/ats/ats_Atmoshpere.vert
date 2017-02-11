

layout(location = 0) in vec3 position;



out vec3 ray;
out vec2 v_TexCoord;


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

	ray = (inv_view_perspective * vPosition).xyz;
	v_TexCoord = (vPosition.xy + vec2(1.0)) / vec2(2.0);

    gl_Position = vPosition;
}
