
layout(triangles, invocations = 1) in;
layout(triangle_strip, max_vertices = 3) out;


in vec4 v_worldPosition[];
in vec2 v_TexCoord[];
in vec3 v_Normal[];
in vec3 v_Tangent[];


out vec4 g_worldPosition;
out vec2 g_TexCoord;
out vec3 g_Normal;
out vec3 g_Tangent;

out vec4 g_BBox;
out vec3 g_Color;
out mat3 g_SwizzleMatrix;
out float g_dir;


uniform mat4 vx_projection;
uniform float vx_volume_dimensions;
uniform float vx_volume_scale;


void expandTriangle(float PixelDiagonal, inout vec4 screenPos[3])
{
	vec2 edge[3];
	edge[0] = screenPos[1].xy - screenPos[0].xy;
	edge[1] = screenPos[2].xy - screenPos[1].xy;
	edge[2] = screenPos[0].xy - screenPos[2].xy;

	vec2 edgeNormal[3];
	edgeNormal[0] = normalize(edge[0]);
	edgeNormal[1] = normalize(edge[1]);
	edgeNormal[2] = normalize(edge[2]);
	edgeNormal[0] = vec2(-edgeNormal[0].y, edgeNormal[0].x);
	edgeNormal[1] = vec2(-edgeNormal[1].y, edgeNormal[1].x);
	edgeNormal[2] = vec2(-edgeNormal[2].y, edgeNormal[2].x);

    // If triangle is back facing, flip it's edge normals so triangle does not shrink.
    vec3 a = normalize(screenPos[1].xyz - screenPos[0].xyz);
	vec3 b = normalize(screenPos[2].xyz - screenPos[0].xyz);
	vec3 clipSpaceNormal = cross(a, b);
    if (clipSpaceNormal.z < 0.0)
    {
        edgeNormal[0] *= -1.0;
        edgeNormal[1] *= -1.0;
        edgeNormal[2] *= -1.0;
    }

	vec3 edgeDist;
	edgeDist.x = dot(edgeNormal[0], screenPos[0].xy);
	edgeDist.y = dot(edgeNormal[1], screenPos[1].xy);
	edgeDist.z = dot(edgeNormal[2], screenPos[2].xy);

	screenPos[0].xy = screenPos[0].xy - PixelDiagonal * (edge[2] / dot(edge[2], edgeNormal[0]) + edge[0] / dot(edge[0], edgeNormal[2]));
	screenPos[1].xy = screenPos[1].xy - PixelDiagonal * (edge[0] / dot(edge[0], edgeNormal[1]) + edge[1] / dot(edge[1], edgeNormal[0]));
	screenPos[2].xy = screenPos[2].xy - PixelDiagonal * (edge[1] / dot(edge[1], edgeNormal[2]) + edge[2] / dot(edge[2], edgeNormal[1]));
}


void main() 
{
	float PixelDiagonal = 1.0 / vx_volume_dimensions;
	
    // Calculate swizzle matrix based on eye space normal's dominant direction.
    vec3 eyeSpaceV1 = normalize(gl_in[1].gl_Position.xyz - gl_in[0].gl_Position.xyz);
	vec3 eyeSpaceV2 = normalize(gl_in[2].gl_Position.xyz - gl_in[0].gl_Position.xyz);
	vec3 eyeSpaceNormal = abs(cross(eyeSpaceV1, -eyeSpaceV2));
	float dominantAxis = max(eyeSpaceNormal.x, max(eyeSpaceNormal.y, eyeSpaceNormal.z));
	mat3 swizzleMatrix;
	if (dominantAxis == eyeSpaceNormal.x)
    {
		swizzleMatrix = mat3(vec3(0.0, 0.0, 1.0),
							 vec3(0.0, 1.0, 0.0),
							 vec3(1.0, 0.0, 0.0));
		g_dir = 1;
    }
	else if (dominantAxis == eyeSpaceNormal.y)
    {
		swizzleMatrix = mat3(vec3(1.0, 0.0, 0.0),
						 	 vec3(0.0, 0.0, 1.0),
							 vec3(0.0, 1.0, 0.0));
		g_dir = 2;
    }
	else// if (dominantAxis == eyeSpaceNormal.z)
    {
		swizzleMatrix = mat3(vec3(1.0, 0.0, 0.0),
							 vec3(0.0, 1.0, 0.0),
							 vec3(0.0, 0.0, 1.0));
		g_dir = 3;
    }

	// Unswizzle fragments in frag shader
    g_SwizzleMatrix = swizzleMatrix;

    // Color code each triangle by which plane it projects to: (Red = x-axis, Green = y-axis, Blue = z-axis).
    g_Color = swizzleMatrix * vec3(0.0, 0.0, 1.0);

    // Calculate screen coordinates for triangle.
	vec4 screenPos[3];
	screenPos[0] = vx_projection * vec4(swizzleMatrix * gl_in[0].gl_Position.xyz, 1.0);
	screenPos[1] = vx_projection * vec4(swizzleMatrix * gl_in[1].gl_Position.xyz, 1.0);
	screenPos[2] = vx_projection * vec4(swizzleMatrix * gl_in[2].gl_Position.xyz, 1.0);
 
    screenPos[0] /= screenPos[0].w;
    screenPos[1] /= screenPos[1].w;
    screenPos[2] /= screenPos[2].w;

    
    // Calculate screen space bounding box to be used for clipping in the fragment shader.
    g_BBox.xy = min(screenPos[0].xy, min(screenPos[1].xy, screenPos[2].xy));
    g_BBox.zw = max(screenPos[0].xy, max(screenPos[1].xy, screenPos[2].xy));
    g_BBox.xy -= vec2(PixelDiagonal);
    g_BBox.zw += vec2(PixelDiagonal);


	// Conservtive Rasterization
	//expandTriangle(PixelDiagonal, screenPos);


    // Output triangle.
	for(int i = 0; i < 3; i++)
	{
		g_worldPosition = v_worldPosition[i];
		g_TexCoord = v_TexCoord[i];
		g_Normal = v_Normal[i];
		g_Tangent = v_Tangent[i];

		gl_Position = screenPos[i];
		EmitVertex();
	}


	EndPrimitive();

}