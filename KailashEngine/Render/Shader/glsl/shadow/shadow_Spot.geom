
layout(triangles, invocations = 4) in;
layout(triangle_strip, max_vertices = 3) out;


in vec3 v_worldPosition[];

out vec3 g_viewPosition;


void main()
{
	mat4 view_matrix = shadow_data[gl_InvocationID].view;
	mat4 perspective_matrix = shadow_data[gl_InvocationID].perspective;
	vec3 light_position = shadow_data[gl_InvocationID].light_position.xyz;
	
	vec3 N = normalize(cross(v_worldPosition[1] - v_worldPosition[0], v_worldPosition[2] - v_worldPosition[0]));

	vec4 viewPositions[3];
	vec4 clipPositions[3];
	for (int i = 0; i < 3; i++)
	{
		vec4 worldPosition = vec4(v_worldPosition[i], 1.0);
		viewPositions[i] = view_matrix * worldPosition;
		clipPositions[i] = perspective_matrix * viewPositions[i];
	}

	bool cull = frustumCullTest(clipPositions) && backfaceCullTest(v_worldPosition[0], light_position, N);
	if (cull)
	{
		for (int i = 0; i < 3; i++) 
		{
			g_viewPosition = viewPositions[i].xyz;
			gl_Position = clipPositions[i];
			gl_Layer = gl_InvocationID;

			EmitVertex();
		}
		EndPrimitive();
	}
}
