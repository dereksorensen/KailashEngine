

in vec2 v_TexCoord;
flat in int instanceID;
 
uniform sampler3D sampler0;				// Albedo Diffuse
layout(rgba8) uniform image3D sampler1;	// Voxel Volume

void main()
{
    ivec3 voxel_TexCoord = ivec3(gl_FragCoord.xy, instanceID);
    
	vec4 albedo = texelFetch(sampler0, voxel_TexCoord, 0);
	vec4 current_value = imageLoad(sampler1, voxel_TexCoord);

	imageStore(sampler1, voxel_TexCoord, vec4(current_value.xyz, albedo.a));

}