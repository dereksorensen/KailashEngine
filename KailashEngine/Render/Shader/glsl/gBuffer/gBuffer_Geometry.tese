

layout(triangles, fractional_odd_spacing, ccw) in;

in vec4 tc_objectPosition[];
in vec3 tc_worldPosition[];
in vec2 tc_TexCoord[];
in vec3 tc_Normal[];
in vec3 tc_Tangent[];

out vec3 te_Normal;
out vec3 te_Tangent;
//out vec3 te_viewPosition;
out vec3 te_worldPosition;
out vec3 te_previousWorldPosition;
out vec2 te_TexCoord;
//out vec4 te_currentPosition;
//out vec4 te_previousPosition;



uniform sampler2D displacement_texture;
uniform int enable_displacement_texture;
uniform float displacement_strength = 0.1;

uniform mat4 model_previous;


vec2 interpolate2D_triangle(vec2 v0, vec2 v1, vec2 v2)
{
   	return vec2(gl_TessCoord.x) * v0 + vec2(gl_TessCoord.y) * v1 + vec2(gl_TessCoord.z) * v2;
}

vec3 interpolate3D_triangle(vec3 v0, vec3 v1, vec3 v2)
{
   	return vec3(gl_TessCoord.x) * v0 + vec3(gl_TessCoord.y) * v1 + vec3(gl_TessCoord.z) * v2;
}

vec4 interpolate3D_triangle(vec4 v0, vec4 v1, vec4 v2)
{
   	return vec4(gl_TessCoord.x) * v0 + vec4(gl_TessCoord.y) * v1 + vec4(gl_TessCoord.z) * v2;
}

vec2 interpolate2D_quad(vec2 v0, vec2 v1, vec2 v2, vec2 v3)
{
	float u = gl_TessCoord.x;
	float v = gl_TessCoord.y;

	vec2 a = mix(v0, v1, u);
	vec2 b = mix(v2, v3, u);
	return mix(a, b, v);
}

vec3 interpolate3D_quad(vec3 v0, vec3 v1, vec3 v2, vec3 v3)
{
	float u = gl_TessCoord.x;
	float v = gl_TessCoord.y;

	vec3 a = mix(v0, v1, u);
	vec3 b = mix(v2, v3, u);
	return mix(a, b, v);
}




void main()
{
	te_TexCoord = interpolate2D_triangle(tc_TexCoord[0], tc_TexCoord[1], tc_TexCoord[2]);
	te_Normal = normalize(interpolate3D_triangle(tc_Normal[0], tc_Normal[1], tc_Normal[2]));
	te_Tangent = normalize(interpolate3D_triangle(tc_Tangent[0], tc_Tangent[1], tc_Tangent[2]));
	te_worldPosition = interpolate3D_triangle(tc_worldPosition[0], tc_worldPosition[1], tc_worldPosition[2]);
	vec4 objectPosition = interpolate3D_triangle(tc_objectPosition[0], tc_objectPosition[1], tc_objectPosition[2]);
	te_previousWorldPosition = (model_previous * vec4(objectPosition.xyz,1.0)).xyz;

	if(enable_displacement_texture == 1)
	{	
		float displacement = texture(displacement_texture, te_TexCoord).r * displacement_strength;
		vec3 displacement_mod = (te_Normal * displacement);
		te_worldPosition += displacement_mod;
		te_previousWorldPosition = te_previousWorldPosition + (displacement_mod);
	}


	gl_Position = vec4(te_worldPosition, 1.0);
}
