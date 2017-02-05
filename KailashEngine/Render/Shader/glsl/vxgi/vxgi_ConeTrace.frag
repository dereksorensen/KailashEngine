
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


uniform sampler2D sampler0;		// Normal Texture
uniform sampler2D sampler1;		// Specular Texture
uniform sampler2D sampler2;		// Diffuse Texture

uniform sampler3D sampler3;		// Voxel Volume


uniform float vx_volume_dimensions;
uniform float vx_volume_scale;
uniform vec3 vx_volume_position;

uniform int maxMipLevels;




vec4 coneTrace(vec3 origin, vec3 dir, vec3 volumeDimensions, float maxDist, float coneRatio)
{
	float minDiameter = 1.0 / volumeDimensions.x;
	float minVoxelDiameterInv = volumeDimensions.x;
	float dist = minDiameter;

	vec4 accum = vec4(0.0);

	// Traverse through voxels until ray exits volume
	while (dist <= maxDist && accum.w < 0.999)
	{
		float sampleDiameter = max(minDiameter, coneRatio * dist);
		float sampleLOD = log2(sampleDiameter * minVoxelDiameterInv);
		sampleLOD = clamp(sampleLOD, 0.0, float(maxMipLevels));

		vec3 offset = dir * dist;
		vec3 samplePos = origin + offset;

		vec4 color = textureLod(sampler3, samplePos, sampleLOD);

		//accum.w = accum.w * (sampleLOD/2.0);
		float sampleWeight = (1.0 - accum.w);
		accum += vec4(color * sampleWeight);

		dist += sampleDiameter;
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
	float maxDist = 0.55;
	float coneRatio = 0.7;

	float count = 0;
	vec4 sum = vec4(0.0);

	float angle1 = angle;


	vec3 rotAngle = normalize(  cross(vec3(0.0,1.0,0.0), normal)  );
	mat4 rot1 = rotationMatrix(rotAngle, angle1 * MATH_PI / 180.0);
	vec3 ref1 = (rot1 * vec4(normal, 0.0)).xyz;
	ref1 = normalize(ref1);

	float num_cones = numCones;
	float inc = floor(360.0 / num_cones);


	for (float ang = 0; ang < 360.0; ang += inc)
	{
		count++;

		vec3 ref2 = (rotationMatrix(normal, ang * MATH_PI / 180.0) * (rot1 * vec4(normal, 0.0))).xyz;
		ref2 = normalize(ref2);

		sum += coneTrace(rayOrigin, ref2, vec3(vx_volume_dimensions), maxDist, coneRatio);
	}

	return sum * (1.0 / count);
}


vec4 ct_diffuse(vec3 rayOrigin, vec3 normal)
{
	float intensity = 1.0;

	vec4 sum = vec4(0.0);

	sum = coneTrace(rayOrigin, normal, vec3(vx_volume_dimensions), 0.6, 0.7);
	sum += diffuseCone(7, 70, rayOrigin, normal);
	sum += diffuseCone(3, 30, rayOrigin, normal);

	sum.a /= 3.0;

	return sum;	
}

vec4 ct_specular(vec3 rayOrigin, vec3 reflection, vec3 normal, vec4 specular_settings)
{

	float intensity = 1.0;

	float maxDist = 0.9;
	float coneRatio = specular_settings.a;
	coneRatio = clamp(coneRatio, 0.1, 1.0);

	vec4 specular = coneTrace(rayOrigin, reflection, vec3(vx_volume_dimensions), maxDist, coneRatio) * intensity;
	specular *= vec4(specular_settings.xyz,1.0);

	return specular;
}




void main()
{
	vec4 normal_depth = texture(sampler0, v_TexCoord);
	float depth = normal_depth.a;
	vec3 normal = normal_depth.rgb;
	vec4 specularSettings = texture(sampler1, v_TexCoord);
	vec4 diffuse = vec4(texture(sampler2, v_TexCoord).rgb, 1.0);

	vec3 world_position = calcWorldPosition(depth, ray, cam_position);
	vec3 world_position_biased = ((world_position - vx_volume_position) / (vx_volume_scale)) * 0.5 + 0.5;

	vec3 reflection = normalize(reflect(ray, normal));
	
	vec4 final = vec4(0.0);

	vec3 shift = ((normal) / (vx_volume_dimensions));
	vec3 origin_shifted = world_position_biased + shift;
	float origin_max = max(origin_shifted.x, max(origin_shifted.y, origin_shifted.z));
	float origin_min = min(origin_shifted.x, min(origin_shifted.y, origin_shifted.z));
	if (!(origin_min < 0.0 || origin_max > 1.0)) 
	{
		vec4 indirect_diffuse = ct_diffuse(origin_shifted, normal);
		vec4 indirect_specular = ct_specular(origin_shifted, reflection, normal, specularSettings);

		float occlusion_diffuse = clamp(indirect_diffuse.a, 0.0, 1.0);
		float occlusion_specular = clamp(indirect_specular.a, 0.0, 1.0);

		// Mix with diffuse
		final = (indirect_diffuse * diffuse);
		final += indirect_specular;
		final = vec4((final).xyz,occlusion_diffuse);
	}
	else
	{
		//final = vec4(0.0, 1.0, 0.0, 0.0);
	}

	FragColor = final;
}






