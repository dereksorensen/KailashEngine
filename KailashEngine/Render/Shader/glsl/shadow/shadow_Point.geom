
layout(triangles, invocations = 6) in;
layout(triangle_strip, max_vertices = 3) out;


in vec3 v_worldPosition[];

out vec3 g_viewPosition;
out vec4 g_clipPosition;

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
	mat4 view_matrix = shadow_data[0].view[gl_InvocationID];
	mat4 perspective_matrix = shadow_data[0].perspective;
	vec3 light_position = shadow_data[0].light_position.xyz;
	
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
			g_clipPosition = clipPositions[i];
			gl_Position = g_clipPosition;
			gl_Layer = gl_InvocationID;

			EmitVertex();
		}
		EndPrimitive();
	}
}
