
layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec3 tangent;
layout(location = 3) in vec2 texCoord;
layout(location = 4) in vec4 bone_ids;
layout(location = 5) in vec4 bone_weights;

out vec2 v_TexCoord;
out vec4 v_objectPosition;
out vec3 v_worldPosition;
//out vec3 v_viewPosition;
out vec3 v_Normal;
out vec3 v_Tangent;

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

uniform mat4 model;
uniform mat4 model_normal;

uniform int enable_skinning;
uniform mat4[32] bone_matrices;


void main()
{

	vec4 v_position = vec4(position, 1.0);
	vec4 v_normal = vec4(normal, 0.0);
	vec4 v_tangent = vec4(tangent, 0.0);

	// Skinning
	if (enable_skinning == 1)
	{
		mat4 bone_matrix = bone_matrices[int(bone_ids[0])] * bone_weights[0];
		bone_matrix += bone_matrices[int(bone_ids[1])] * bone_weights[1];
		bone_matrix += bone_matrices[int(bone_ids[2])] * bone_weights[2];
		bone_matrix += bone_matrices[int(bone_ids[3])] * bone_weights[3];

		v_position = bone_matrix * v_position;
		v_normal = bone_matrix * v_normal;
		v_tangent = bone_matrix * v_tangent;
	}

	// Position
	v_objectPosition = v_position;
	vec4 world_position = (model * v_objectPosition);
	v_worldPosition = world_position.xyz;


	// Texture Coordinates
	v_TexCoord = texCoord;

	// Put vertex normal in world space
	v_Normal = normalize((model_normal * v_normal).xyz);
	v_Tangent = normalize((model_normal * v_tangent).xyz);

}
