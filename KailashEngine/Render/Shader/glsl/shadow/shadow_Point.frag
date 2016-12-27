

layout(location = 0) out vec4 color;

in vec3 g_viewPosition;
in vec4 g_clipPosition;


void main()
{
	float depth = length(g_viewPosition);

	vec2 depth_moments = computeMoments(depth);
	vec4 depth_packed = vec4(pack2(depth_moments.x),pack2(depth_moments.y));

	color = depth_packed;
}
