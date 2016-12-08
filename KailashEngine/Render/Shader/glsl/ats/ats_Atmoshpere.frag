out vec4 color;

in vec3 ray;
in vec2 v_TexCoord;


void main() 
{
    color = vec4(ray, 1.0);
	color = vec4(v_TexCoord, 0.0, 1.0);

}