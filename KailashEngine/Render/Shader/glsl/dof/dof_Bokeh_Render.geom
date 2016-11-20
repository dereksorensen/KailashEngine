


in vec4 v_bokehColor[1];
in float v_bokehDepth[1];
in float v_bokehSize[1];

out vec4 g_bokehColor;
out float g_bokehDepth;
out vec2 g_TexCoord;

layout(points) in;
layout(triangle_strip, max_vertices = 4) out;


uniform vec2 texture_size;

void main()
{

	gl_Layer = 0;
	g_bokehColor = v_bokehColor[0];
	g_bokehDepth = v_bokehDepth[0];

	float bokeh_size = v_bokehSize[0];

	vec2 offsetx = vec2(texture_size.x * bokeh_size, 0);
	vec2 offsety = vec2(0, texture_size.y * bokeh_size);

	// Expand point into a quad
	gl_Position = vec4((gl_in[0].gl_Position.xy - offsetx - offsety),0,1);
	g_TexCoord	= vec2(0,1);
	EmitVertex();
	gl_Position = vec4((gl_in[0].gl_Position.xy + offsetx - offsety),0,1);
	g_TexCoord	= vec2(1,1);
	EmitVertex();
	gl_Position = vec4((gl_in[0].gl_Position.xy - offsetx + offsety),0,1);
	g_TexCoord	= vec2(0,0);
	EmitVertex();
	gl_Position = vec4((gl_in[0].gl_Position.xy + offsetx + offsety),0,1);
	g_TexCoord	= vec2(1,0);
	EmitVertex();

	EndPrimitive();

}
