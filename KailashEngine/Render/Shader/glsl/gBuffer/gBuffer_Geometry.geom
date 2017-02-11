
layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

in vec3 te_Normal[];
in vec3 te_Tangent[];
//in vec3 te_viewPosition[];
in vec3 te_worldPosition[];
in vec3 te_previousWorldPosition[];
in vec2 te_TexCoord[];
//in vec4 te_currentPosition[];
//in vec4 te_previousPosition[];

out vec3 g_Normal;
out vec3 g_Tangent;
out vec3 g_viewPosition;
out vec3 g_worldPosition;
out vec2 g_TexCoord;
out vec4 g_currentPosition;
out vec4 g_previousPosition;
noperspective out vec3 g_wireframe_distance;


uniform vec2 render_size;


void main() 
{
	
	// Wireframe parts taken from 'Single-Pass Wireframe Rendering'
	vec4[3] viewPositions;
	vec4[3] clipPositions;	
	vec2[3] wireframe_points;

	for(int i = 0; i < 3; i++)
	{
		viewPositions[i] =  view * gl_in[i].gl_Position;
		clipPositions[i] =  perspective * viewPositions[i];
	
		wireframe_points[i] = render_size * clipPositions[i].xy/clipPositions[i].w;
	}

	vec2 v[3] = vec2[3](
		wireframe_points[2] - wireframe_points[1], 
		wireframe_points[2] - wireframe_points[0], 
		wireframe_points[1] - wireframe_points[0]);
	float area = abs(v[1].x*v[2].y - v[1].y * v[2].x);

	vec3 chooser[3] = vec3[3]( vec3(1.0,0.0,0.0), vec3(0.0,1.0,0.0), vec3(0.0,0.0,1.0) );

	// Output triangle
	for(int i = 0; i < 3; i++)
	{
		g_wireframe_distance = vec3(area/length(v[i])) * chooser[i];

		g_worldPosition = te_worldPosition[i];
		g_viewPosition = viewPositions[i].xyz;
		g_Tangent = te_Tangent[i];
		g_currentPosition = clipPositions[i];
		g_previousPosition = previous_view_persepctive * vec4(te_previousWorldPosition[i], 1.0);
		g_TexCoord = te_TexCoord[i];
		g_Normal = te_Normal[i];

		gl_Position = clipPositions[i];
		EmitVertex();
	}
		
	EndPrimitive();


}
