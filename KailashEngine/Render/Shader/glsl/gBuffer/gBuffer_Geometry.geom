
layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

in vec3 te_Normal[];
in vec3 te_Tangent[];
in vec3 te_viewPosition[];
in vec3 te_worldPosition[];
in vec2 te_TexCoord[];

out vec3 g_Normal;
out vec3 g_Tangent;
out vec3 g_viewPosition;
out vec3 g_worldPosition;
out vec2 g_TexCoord;

noperspective out vec3 g_wireframe_distance;
uniform vec2 render_size;

void main() 
{
	// taken from 'Single-Pass Wireframe Rendering'
	vec2 p0 = render_size * gl_in[0].gl_Position.xy/gl_in[0].gl_Position.w;
	vec2 p1 = render_size * gl_in[1].gl_Position.xy/gl_in[1].gl_Position.w;
	vec2 p2 = render_size * gl_in[2].gl_Position.xy/gl_in[2].gl_Position.w;
	vec2 v[3] = vec2[3](p2-p1, p2-p0, p1-p0 );
	float area = abs(v[1].x*v[2].y - v[1].y * v[2].x);

	vec3 chooser[3] = vec3[3]( vec3(1.0,0.0,0.0), vec3(0.0,1.0,0.0), vec3(0.0,0.0,1.0) );

	// Output triangle
	for(int i = 0; i < 3; i++)
	{
		g_wireframe_distance = vec3(area/length(v[i])) * chooser[i];
		g_Normal = te_Normal[i];
		g_Tangent = te_Tangent[i];
		g_viewPosition = te_viewPosition[i];
		g_worldPosition = te_worldPosition[i];
		g_TexCoord = te_TexCoord[i];

		gl_Position = gl_in[i].gl_Position;
		EmitVertex();
	}

	EndPrimitive();
}
