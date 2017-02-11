
//layout(triangles, invocations = 4) in;
layout(triangle_strip, max_vertices = 3) out;


in vec3 v_worldPosition[];

out vec3 g_viewPosition;



void main()
{
	int shadow_data_id = int(floor(gl_InvocationID / 4.0));
	int shadow_matrix_id = gl_InvocationID % 4;

	mat4 view_matrix = shadow_data[shadow_data_id].view[shadow_matrix_id];
	mat4 perspective_matrix = shadow_data[shadow_data_id].perspective[shadow_matrix_id];
	vec3 light_position = shadow_data[shadow_data_id].light_position.xyz;	

	vec4 viewPositions[3];
	vec4 clipPositions[3];
	for (int i = 0; i < 3; i++)
	{
		vec4 worldPosition = vec4(v_worldPosition[i], 1.0);
		viewPositions[i] = view_matrix * worldPosition;
		clipPositions[i] = perspective_matrix * viewPositions[i];
	}


	for (int i = 0; i < 3; i++) 
	{
		g_viewPosition = viewPositions[i].xyz;
		gl_Position = clipPositions[i];
		gl_Layer = gl_InvocationID;

		EmitVertex();
	}
	EndPrimitive();

}
