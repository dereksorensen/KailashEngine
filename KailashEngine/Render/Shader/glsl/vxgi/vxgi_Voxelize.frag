



//layout(binding = 7, r32ui) coherent volatile uniform uimage3D volumeTexture;
//layout(binding = 6, r32ui) coherent volatile uniform uimage3D volumeTexture_normal;

writeonly uniform image3D sampler0;



in vec4 g_worldPosition;
in vec2 g_TexCoord;
in vec3 g_Normal;
in vec3 g_Tangent;

in vec4 g_BBox;
in vec3 g_Color;
in mat3 g_SwizzleMatrix;
in float g_dir;


//------------------------------------------------------
// Bindless Material Textures
//------------------------------------------------------
layout(std140, binding = 2) uniform materialTextures
{
	sampler2D tex[256];
};


uniform int enable_diffuse_texture;
uniform int diffuse_texture_unit;
uniform vec3 diffuse_color;
uniform float emission_strength;

uniform float emissionStrength;



vec4 RGBA8_to_VEC4(uint val)
{
	return vec4( float((val&0x000000FF)), float((val&0x0000FF00)>>8U), float((val&0x00FF0000)>>16U), float((val&0xFF000000)>>24U) );
}

uint VEC4_to_RGBA8(vec4 val)
{
	return (uint(val.w)&0x000000FF)<<24U | (uint(val.z)&0x000000FF)<<16U | (uint(val.y)&0x000000FF)<<8U | (uint(val.x)&0x000000FF);
}

void imageAtomicRGBA8Avg(layout(r32ui) coherent volatile uimage3D imgUI, ivec3 coords, vec4 val)
{
	val.rgb *= 255.0f;
	uint newVal = VEC4_to_RGBA8(val);
	uint prevStoredVal = 0; 
	uint curStoredVal;

	while( (curStoredVal = imageAtomicCompSwap(imgUI, coords, prevStoredVal, newVal)) != prevStoredVal )
	{
		prevStoredVal = curStoredVal;
		vec4 rval = RGBA8_to_VEC4(curStoredVal);
		rval.xyz = (rval.xyz*rval.w);
		vec4 curValF = rval+val;
		curValF.xyz /= (curValF.w);
		newVal = VEC4_to_RGBA8(curValF);
	}
}


/*
void imageAtomicRGBA8Avg(layout(r32ui) coherent volatile uimage3D imgUI, ivec3 coords, vec4 val)
{
	uint nextUint = packUnorm4x8(vec4(val.xyz,1.0f/255.0f));
	uint prevUint = 0;
	uint currUint;

	vec4 currVec4;

	vec3 average;
	uint count;

	while((currUint = imageAtomicCompSwap(imgUI, coords, prevUint, nextUint)) != prevUint)
	{
		prevUint = currUint;
		currVec4 = unpackUnorm4x8(currUint);

		average = currVec4.rgb;
		count   = uint(currVec4.a*255.0f);

		//Compute the running average
		average = (average*count + val.xyz) / (count+1);

		//Pack new average and incremented count back into a uint
		nextUint = packUnorm4x8(vec4(average, (count+1)/255.0f));
	}
}
*/



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


		vec3 fragmentColor = diffuse.xyz;// * emission_strength;

		fragmentColor = clamp(fragmentColor, 0.0, 1.0);


		//imageAtomicRGBA8Avg(sampler0, ivec3(coords), vec4(fragmentColor,1.0));
		//imageAtomicRGBA8Avg(sampler0, ivec3(coords), vec4(normal.xyz * 0.5 + 0.5,1.0));

		imageStore(sampler0, ivec3(coords), vec4(fragmentColor,1.0));

    }
    else
    {
        discard;
    }

}

