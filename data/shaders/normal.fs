
varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;
varying vec4 v_color;

uniform vec3 u_camera_position;
uniform float ray_step;
uniform sampler3D volume;
uniform mat4 model;
uniform mat4 inv_model;

void main()
{
	vec3 sample_pos = vec3(0.0); // Sample position
	vec3 ray_dir = v_world_position - u_camera_position; // Ray direction
	float d = texture3D(volume,sample_pos).x; // Density of sample position
	vec4 sample_color = vec4(d,d,d,d);
	vec4 finalColor = vec4(0.0);
	vec4 sample_pos_local = inv_model * vec4(sample_pos, 1.0);
	float i = 0;
	while((sample_pos_local.x > -1 || sample_pos_local.y > -1 || sample_pos_local.z > -1 || sample_pos_local.x < 1 || sample_pos_local.y < 1 || sample_pos_local.z < 1) && i){
		sample_pos += ray_dir * ray_step;
		sample_pos_local = inv_model * vec4(sample_pos, 1.0);
		finalColor += ray_step * (1.0 - finalColor.a) * sample_color;
	}
	//gl_FragColor = vec4(texture3D(volume,vec3(1.0).xyz,1.0);
	gl_FragColor = vec4(1.0);
}
