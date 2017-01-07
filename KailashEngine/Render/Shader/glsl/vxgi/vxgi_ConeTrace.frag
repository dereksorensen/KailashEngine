
#define MAX_ITERATIONS	500

out vec4 FragColor;

in vec3 ray;


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


uniform sampler2D sampler0;		// Diffuse Texture
uniform sampler2D sampler1;		// Normal Texture
uniform sampler2D sampler2;		// Specular Texture

uniform sampler3D sampler3;		// Voxel Volume


uniform float vx_volume_dimensions;
uniform float vx_volume_scale;
uniform vec3 vx_volume_position;
uniform mat4 vx_inv_view_perspective;


uniform int displayVoxels;
uniform int displayMipLevel;
uniform int maxMipLevels;



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


	vec3 finalColor = vec3(0.0);


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
		while (all(greaterThanEqual(voxelPos, vec3(0.0))) && all(lessThan(voxelPos, volumeDimensions)) && accum.a < 0.8)
		{
			count++;

			vec4 color = textureLod(sampler3, voxelPos/volumeDimensions.x, displayMipLevel);


			float sampleWeight = (1.0 - accum.w);
			accum += vec4(color);

			/*
			if (color.a > 0.0)
			{
				finalColor += color.xyz;
				count /= 10.0;
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



vec4 coneTrace(vec3 origin, vec3 dir, vec3 volumeDimensions, float maxDist, float coneRatio)
{

	float minDiameter = 1.0 / volumeDimensions.x;
	float minVoxelDiameterInv = volumeDimensions.x;
	float dist = minDiameter;

	vec4 accum = vec4(0.0);


	// Perform AABB test with volume.
	vec3 rayOrigin = origin * volumeDimensions.x;
	vec3 result = RayAABBTest(rayOrigin, dir, vec3(0.0), volumeDimensions);
	if (result.x != 0.0)
	{	

		// Traverse through voxels until ray exits volume
		while (dist <= maxDist && accum.w < 1.0)
		{

			float sampleDiameter = max(minDiameter, coneRatio * dist);
			float sampleLOD = log2(sampleDiameter * minVoxelDiameterInv);
			//sampleLOD = clamp(sampleLOD, 0.0, float(maxMipLevels));

			vec3 offset = dir * dist;
			vec3 samplePos = origin + offset;

			vec4 color = textureLod(sampler3, samplePos, sampleLOD);


			//accum.w = accum.w * (sampleLOD/2.0);
			float sampleWeight = (1.0 - accum.w);
			accum += vec4(color * sampleWeight);

			dist += sampleDiameter;

		}

	}

	float occlusion = accum.w;


	return vec4(accum.xyz, occlusion);

}




mat4 rotationMatrix(vec3 axis, float angle)
{
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return mat4(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
                oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
                oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
                0.0,                                0.0,                                0.0,                                1.0);
}


vec4 diffuseCone(float numCones, float angle, vec3 rayOrigin, vec3 normal)
{
	float PI = 3.14159265359;
	float maxDist = 0.15;
	float coneRatio = 0.5;
	float shiftAmount = 200.0;

	float count = 0;
	vec4 sum = vec4(0.0);

	float angle1 = angle;


	vec3 rotAngle = normalize(  cross(vec3(0.0,1.0,0.0), normal)  );
	mat4 rot1 = rotationMatrix(rotAngle, angle1 * PI / 180.0);
	vec3 ref1 = (rot1 * vec4(normal, 0.0)).xyz;
	ref1 = normalize(ref1);

	float num_cones = numCones;
	float inc = floor(360.0 / num_cones);


	for (float ang = 0; ang < 360.0; ang += inc)
	{
		count++;

		vec3 ref2 = (rotationMatrix(normal, ang * PI / 180.0) * (rot1 * vec4(normal, 0.0))).xyz;
		ref2 = normalize(ref2);

		vec3 shift = ((ref2 / dot(ref2,normal)) / shiftAmount);
		sum += coneTrace(rayOrigin + shift, ref2, vec3(vx_volume_dimensions), maxDist, coneRatio);
	}

	count++;
	vec3 shift = ((normal / dot(normal,normal)) / shiftAmount);
	sum += coneTrace(rayOrigin + shift, normal, vec3(vx_volume_dimensions), maxDist, coneRatio) * 1.414;

	return sum * (0.707 / count);
}


vec4 ct_diffuse(vec3 rayOrigin, vec3 normal)
{
	float intensity = 1.0;

	vec4 sum = vec4(0.0);

	sum = diffuseCone(5, 50, rayOrigin, normal);
	sum += diffuseCone(3, 25, rayOrigin, normal);

	sum /= vec4(2.0);

	//float occlusion = sum.a * sum.a;

	return sum;	
}

vec4 ct_specular(vec3 rayOrigin, vec3 reflection, vec3 normal, vec4 specular_settings)
{

	float intensity = 1.0;

	float maxDist = 0.3;
	float coneRatio = (1.0) / specular_settings.a;
	coneRatio = clamp(coneRatio, 0.1, 1.0);

	vec3 shift = ((reflection / dot(reflection,normal)) / 256.0);
	vec4 specular = coneTrace(rayOrigin, reflection, vec3(vx_volume_dimensions), maxDist, coneRatio) * intensity;
	specular *= vec4(specular_settings.xyz,1.0);

	return specular;
}


void main()
{
	vec2 tex_coord = textureSize(sampler0, 0).xy;

	
	vec4 diffuse = texture(sampler0, tex_coord);
	vec4 nd = texture(sampler1, tex_coord);
	vec3 normal = normalize(nd.xyz);
	vec4 specularSettings = texture(sampler2, tex_coord);

	vec4 world_position = (vec4(calcWorldPosition(nd.a, ray, cam_position), 1.0) / vx_volume_scale) * 0.5 + 0.5;



	vec2 rayCoords = ray.xy;
  
    // Compute ray's origin and direction from pixel coordinate.
	vec4 ndc0 = vec4(rayCoords, -1.0, 1.0);
	vec4 ndc1 = vec4(rayCoords, -0.5, 1.0);
	vec4 world0 = vx_inv_view_perspective * ndc0;
	vec4 world1 = vx_inv_view_perspective * ndc1;
	world0 /= world0.w;
	world1 /= world1.w;


	vec3 rayDir = world1.xyz - world0.xyz;
	vec3 rayOrigin = (world_position.xyz).xyz;
	vec3 reflection = normalize(reflect(normalize(rayDir), normalize(nd.xyz)));

	

	vec4 final = vec4(0.0);

		vec4 indirect_diffuse = ct_diffuse(rayOrigin, normal);
		vec4 indirect_specular = ct_specular(rayOrigin, reflection, normal, specularSettings);

		float occlusion_diffuse = 1.0 - indirect_diffuse.a;
		float occlusion_specular = 1.0 - indirect_specular.a;

	if (displayVoxels == 0)
	{

		// Mix with diffuse
		final = (indirect_diffuse * 1.0);
		final += indirect_specular;
		final = vec4((final).xyz,occlusion_diffuse);
	}
	else
	{
		final = rayCast(world0.xyz, rayDir, 500.0, vec3(vx_volume_dimensions), 0);
		final = vec4(occlusion_diffuse);
	} 
		//final = rayCast(world0.xyz, rayDir, 500.0, vec3(vx_volume_dimensions), 0);



	FragColor = final;
	//FragColor = vec4(rayOrigin,1.0);
}






