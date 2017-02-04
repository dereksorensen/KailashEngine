
#define MAX_ITERATIONS	500

out vec4 FragColor;

in vec3 ray;
in vec2 v_TexCoord;


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


uniform sampler3D sampler0;		// Voxel Volume


uniform float vx_volume_dimensions;
uniform mat4 vx_inv_view_perspective;



uniform int displayMipLevel;


vec3 RayAABBTest(vec3 rayOrigin, vec3 rayDir, vec3 aabbMin, vec3 aabbMax)
{
	float tMin, tMax;

	// Project ray through aabb
	vec3 invRayDir = 1.0 / rayDir;
	vec3 t1 = (aabbMin - rayOrigin) * invRayDir;
	vec3 t2 = (aabbMax - rayOrigin) * invRayDir;

	vec3 tmin = min(t1, t2);
	vec3 tmax = max(t1, t2);

	tMin = max(tmin.x, max(tmin.y, tmin.z));
	tMax = min(min(99999.0, tmax.x), min(tmax.y, tmax.z));

    if (tMin < 0.0) tMin = 0.0;

	vec3 result;
	result.x = (tMax > tMin) ? 1.0 : 0.0;
	result.y = tMin;
	result.z = tMax;
	return result;
}


vec4 rayCast(vec3 origin, vec3 dir, float attenuation, vec3 volumeDimensions, int level)
{
	vec3 rayOrigin = origin;
	vec3 rayDir = (dir);


    // Check for ray components being parallel to axes (i.e. values of 0).
	const float epsilon = 0.00001;
	if (abs(rayDir.x) <= epsilon) rayDir.x = epsilon * sign(rayDir.x);
	if (abs(rayDir.y) <= epsilon) rayDir.y = epsilon * sign(rayDir.y);
	if (abs(rayDir.z) <= epsilon) rayDir.z = epsilon * sign(rayDir.z);


	// Calculate inverse of ray direction once.
	vec3 invRayDir = 1.0 / rayDir;

	float count = 0;
	vec4 accum = vec4(0.0);
   
	// Perform AABB test with volume.
	vec3 result = RayAABBTest(rayOrigin, rayDir, vec3(0.0), volumeDimensions);
	if (result.x != 0.0)
	{		
		// Extract out ray's start and end position.
		float tMin = result.y;
		float tMax = result.z;

		vec3 startPos = rayOrigin + rayDir * tMin;
		vec3 voxelPos = max(vec3(0.0), min(volumeDimensions - vec3(1.0), floor(startPos)));


		// Traverse through voxels until ray exits volume.
		while (all(greaterThanEqual(voxelPos, vec3(0.0))) && all(lessThan(voxelPos, volumeDimensions)) && accum.a < 0.999)
		{
			count++;

			vec4 color = textureLod(sampler0, voxelPos/volumeDimensions.x, displayMipLevel);

			float sampleWeight = (1.0 - accum.w);
			accum += vec4(color * sampleWeight);

			/*
			if (color.a > 0.0)
			{
				return vec4(color);
			}
			*/
	
			// Move to next closest voxel along ray.
			vec3 t0 = (voxelPos - startPos) * invRayDir;
			vec3 t1 = (voxelPos + vec3(1.0) - startPos) * invRayDir;
			vec3 tmax = max(t0, t1);
			float t = min(tmax.x, min(tmax.y, tmax.z));
			if (tmax.x == t) voxelPos.x += sign(rayDir.x);
			else if (tmax.y == t) voxelPos.y += sign(rayDir.y);
			else if (tmax.z == t) voxelPos.z += sign(rayDir.z);


			if (count > MAX_ITERATIONS)
			{
				return vec4(0.0,1.0,0.0,1.0);
			}

		}    
	}

	return vec4(accum);
}


void main()
{

	vec4 final = vec4(0.0);


	// Compute ray's origin and direction from pixel coordinate.
	vec2 rayCoords = v_TexCoord * 2.0 - 1.0;

	vec4 ndc0 = vec4(rayCoords, -1.0, 1.0);
	vec4 world0 = vx_inv_view_perspective * ndc0;
	world0 /= world0.w;

	final = rayCast(world0.xyz, ray, vx_volume_dimensions, vec3(vx_volume_dimensions), 0);



	FragColor = final;
}






