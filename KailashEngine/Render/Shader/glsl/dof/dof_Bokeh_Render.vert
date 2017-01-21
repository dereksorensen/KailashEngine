
layout(location = 0) in vec4 p;

out vec4 v_bokehColor;
out float v_bokehDepth;
out float v_bokehSize;


layout(size4x32, binding = 3) readonly uniform image1D sampler3;
layout(size4x32, binding = 4) readonly uniform image1D sampler4;


uniform float max_blur;


void main()
{
	// Load bokeh data
	vec4 bokeh_properties = imageLoad(sampler3, gl_InstanceID);
	vec4 bokeh_color = imageLoad(sampler4, gl_InstanceID);

	// Parse data
	float bokeh_depth = bokeh_properties.z;
	float coc = bokeh_properties.w;	

	// Bokeh Position
	vec2 point_position = bokeh_properties.xy * 2.0 - 1.0;
	gl_Position = vec4(point_position, 0.0, 1.0);
	
	// Bokeh Size
	float max_bokeh_size = max_blur;
	float bokeh_size = min(coc * max_bokeh_size, max_bokeh_size) / MATH_2_PI;

	// Bokeh Color
	float coc_area = bokeh_size * bokeh_size * MATH_PI;
	float falloff = pow(1.0 / coc_area, 0.8) * MATH_PI;
	vec4 color_mod = bokeh_color * falloff;

	v_bokehColor = color_mod;
	v_bokehDepth = bokeh_depth;
	v_bokehSize = bokeh_size;
}

