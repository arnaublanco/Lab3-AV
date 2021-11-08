
varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;
varying vec4 v_color;

uniform sampler3D volume;

void main()
{
	//vec3 direction = v_world_position - u_camera_position;
	float d = texture3D(volume,vec3(1.0)).x;
	gl_FragColor = vec4(texture3D(volume,vec3(1.0).xyz,1.0);
}
