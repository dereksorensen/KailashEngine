
layout(triangles, invocations = 12) in;
layout(triangle_strip, max_vertices = 3) out;


in vec3 v_worldPosition[];

out vec3 g_viewPosition;

//------------------------------------------------------
// Shadow Matrices - Point
//------------------------------------------------------
struct ShadowData {
  mat4 view[6];
  mat4 perspective;
  vec3 light_position;
};
layout(std140, binding = 4) uniform shadowMatrices
{
	ShadowData shadow_data[32];
};

void main()
{
	int shadow_data_id = int(floor(gl_InvocationID / 6.0));
	int view_matrix_id = gl_InvocationID % 6;

	mat4 view_matrix = shadow_data[shadow_data_id].view[view_matrix_id];
	mat4 perspective_matrix = shadow_data[shadow_data_id].perspective;
	vec3 light_position = shadow_data[shadow_data_id].light_position.xyz;
	
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
