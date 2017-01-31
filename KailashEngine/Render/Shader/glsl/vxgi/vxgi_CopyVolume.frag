

in vec2 v_TexCoord;
flat in int instanceID;
 
uniform sampler3D sampler0;				// Source
writeonly uniform image3D sampler1;		// Destination 1
writeonly uniform image3D sampler2;		// Destination 2
writeonly uniform image3D sampler3;		// Destination 3
writeonly uniform image3D sampler4;		// Destination 4
writeonly uniform image3D sampler5;		// Destination 5

void main()
{
    ivec3 voxel_TexCoord = ivec3(gl_FragCoord.xy, instanceID);
    
	vec4 source = texelFetch(sampler0, voxel_TexCoord, 0);

	imageStore(sampler1, voxel_TexCoord, source);
	imageStore(sampler2, voxel_TexCoord, source);
	imageStore(sampler3, voxel_TexCoord, source);
	imageStore(sampler4, voxel_TexCoord, source);
	imageStore(sampler5, voxel_TexCoord, source);
}