


//layout(r32ui) uniform uimage3D sampler0;		// Voxel Volume
//layout(r32ui) coherent volatile uniform uimage3D sampler1;		// Voxel Volume - Diffuse

writeonly uniform image3D sampler0;		// Voxel Volume
writeonly uniform image3D sampler1;		// Voxel Volume - Diffuse


in vec4 g_worldPosition;
in vec2 g_TexCoord;
in vec3 g_Normal;
in vec3 g_Tangent;

in vec4 g_BBox;
in mat3 g_SwizzleMatrix;
in float g_dir;


uniform int enable_diffuse_texture;
uniform int diffuse_texture_unit;
uniform vec3 diffuse_color;
uniform float emission_strength;

uniform float emissionStrength;


void main()
{

	//------------------------------------------------------
	// Diffuse
	//------------------------------------------------------
	vec4 diffuse = vec4(diffuse_color,1.0);
	if(enable_diffuse_texture == 1)
	{
		vec4 diffuseTexture = texture(tex[diffuse_texture_unit], g_TexCoord);
		diffuse = diffuseTexture;
	}



    ivec2 viewportSize = imageSize(sampler0).xy;
    vec2 bboxMin = floor((g_BBox.xy * 0.5 + 0.5) * viewportSize);
	vec2 bboxMax = ceil((g_BBox.zw * 0.5 + 0.5) * viewportSize);

	// If fragments are outside the bounding box for this triange, discard
	if (all(greaterThanEqual(gl_FragCoord.xy, bboxMin)) && all(lessThanEqual(gl_FragCoord.xy, bboxMax)))
	{


		vec3 coords = g_SwizzleMatrix * (vec3(gl_FragCoord.xy, gl_FragCoord.z * viewportSize.x));

		//Shift becuase of right-hand coord system
		if (g_dir == 1)
		{
			coords.x = viewportSize.x - coords.x;
		}
		else if (g_dir == 2)
		{
			coords.y = viewportSize.x - coords.y;
		}
		else if (g_dir == 3)
		{
			coords.z = viewportSize.x - coords.z;
		}


		vec3 fragmentColor = diffuse.xyz * (emission_strength * 10.0);
		//fragmentColor = clamp(fragmentColor, 0.0, 1.0);


		imageStore(sampler0, ivec3(coords), vec4(fragmentColor,1.0));
		imageStore(sampler1, ivec3(coords), vec4(diffuse.xyz,1.0));
		
		//imageAtomicAverageRGBA8(sampler0, ivec3(coords), vec4(fragmentColor,1.0));
		//imageAtomicAverageRGBA8(sampler1, ivec3(coords), vec4(diffuse.xyz,1.0));

    }
    else
    {
        discard;
    }

}

