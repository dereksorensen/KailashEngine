

layout(vertices = 3) out;

in vec3 v_worldPosition[];
in vec3 v_previousWorldPosition[];
in vec2 v_TexCoord[];
in vec3 v_Normal[];
in vec3 v_Tangent[];

out vec3 tc_worldPosition[];
out vec3 tc_previousWorldPosition[];
out vec2 tc_TexCoord[];
out vec3 tc_Normal[];
out vec3 tc_Tangent[];

//------------------------------------------------------
// Camera Spatials
//------------------------------------------------------
layout(std140, binding = 1) uniform cameraSpatials
{
	mat4 view;
	mat4 perspective;
	mat4 inv_view_perspective;
	mat4 previous_view_persepctive;
	mat4 inv_previous_view_persepctive;
	vec3 cam_position;
	vec3 cam_look;
};

uniform int enable_displacement_texture;
uniform vec2 render_size;


float screenSphereSize(vec4 e1, vec4 e2)
{
	vec4 p1 = (e1 + e2) * 0.5;
	vec4 p2 = p1;
	p2.y += distance(e1, e2);

	p1 = p1 / p1.w;
	//p1 = p1 * 0.5 + 0.5;
	p2 = p2 / p2.w;
	//p2 = p2 * 0.5 + 0.5;

	float l = length((p1.xy - p2.xy) * render_size * 0.5);

	return (clamp(l / 15.0, 1.0, 64.0));
}

bool edgeInFrustum(vec4 p, vec4 q)
{
	return !( (p.x < -p.w && q.x < -q.w) || (p.x > p.w && q.x > q.w) 
			|| (p.z < -p.w && q.z < -q.w) || (p.z > p.w && q.z > q.w) );
}

void controlTessellation(mat4 mvp, vec3[gl_MaxPatchVertices] world_position)
{
	float tessLevel = 1.0;

	vec4 vertex_position[3];

	for (int i = 0; i < 3; i++)
	{
		vertex_position[i] = mvp * vec4(world_position[i], 1.0);
	}

	if (
		edgeInFrustum(vertex_position[1], vertex_position[0]) || 
		edgeInFrustum(vertex_position[2], vertex_position[0]) || 
		edgeInFrustum(vertex_position[2], vertex_position[1]))
	{
		if(enable_displacement_texture == 0)
		{
			tessLevel = 1.0;
			gl_TessLevelOuter[2] = tessLevel;
			gl_TessLevelOuter[1] = tessLevel;
			gl_TessLevelOuter[0] = tessLevel;
			gl_TessLevelInner[0] = tessLevel;
		}
		else
		{
			// Calculate the tessellation levels
			gl_TessLevelOuter[2] = screenSphereSize(vertex_position[1], vertex_position[0]);
			gl_TessLevelOuter[1] = screenSphereSize(vertex_position[2], vertex_position[0]);
			gl_TessLevelOuter[0] = screenSphereSize(vertex_position[2], vertex_position[1]);
			gl_TessLevelInner[0] = max(gl_TessLevelOuter[1], max(gl_TessLevelOuter[0],gl_TessLevelOuter[2]));
		}
	}
	else
	{
		gl_TessLevelOuter[0] = gl_TessLevelOuter[1] = gl_TessLevelOuter[2] = 0.0;
		gl_TessLevelInner[0] = 0.0;
	}
}

void main()
{
    tc_worldPosition[gl_InvocationID] = v_worldPosition[gl_InvocationID];
    tc_previousWorldPosition[gl_InvocationID] = v_previousWorldPosition[gl_InvocationID];
	tc_TexCoord[gl_InvocationID] = v_TexCoord[gl_InvocationID];
	tc_Normal[gl_InvocationID] = v_Normal[gl_InvocationID];
	tc_Tangent[gl_InvocationID] = v_Tangent[gl_InvocationID];

	controlTessellation(perspective * view, v_worldPosition);

}

