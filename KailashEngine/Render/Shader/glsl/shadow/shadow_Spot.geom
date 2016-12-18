
layout(triangles, invocations = 2) in;
layout(triangle_strip, max_vertices = 3) out;


in vec3 v_worldPosition[];

out vec3 g_viewPosition;


//------------------------------------------------------
// Shadow Matrices - Spot
//------------------------------------------------------
layout(std140, binding = 3) uniform shadowMatrices
{
	mat4 mat[4];
};



void main()
{
	mat4 perspective_matrix = mat[gl_InvocationID * 2];
	mat4 view_matrix = mat[gl_InvocationID * 2 + 1];

	for (int i = 0; i < 3; i++) 
	{
		vec4 worldPosition = vec4(v_worldPosition[i], 1.0);
		vec4 viewPositioin = view_matrix * worldPosition;
		g_viewPosition = viewPositioin.xyz;

		gl_Position = perspective_matrix * viewPositioin;
		gl_Layer = gl_InvocationID;

		EmitVertex();
	}

	EndPrimitive();
}
