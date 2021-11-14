#define N_MAX 1000
#define epsilon 1e-6

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

vec3 local_to_texture(vec3 v_local){
	vec3 v_texture = (v_local + 1.0)/2.0;
	return v_texture;
}

vec3 world_to_local(vec3 v_in){
	vec4 aux = inv_model * vec4(v_in, 1.0);
	aux /= aux.a;
	vec3 v_out = aux.xyz;
	return v_out;
}

void main()
{
	vec3 sample_pos = v_position; // Sample position
	vec3 ray_dir = normalize(v_world_position - u_camera_position); // Ray direction
	ray_dir = world_to_local(ray_dir);

	float d = texture3D(volume, local_to_texture(sample_pos)).x; // Density of sample position
	vec4 sample_color = vec4(d,d,d,d);
	vec4 final_color = vec4(0.0);

	int count = 0;
	while(sample_pos.x >= -1 && sample_pos.y >= -1 && sample_pos.z >= -1 && sample_pos.x <= 1 && sample_pos.y <= 1 && sample_pos.z <= 1 && final_color.a <= 1 && count <= N_MAX){
		
		final_color += ray_step * (1.0 - final_color.a) * sample_color;

		d = texture3D(volume, local_to_texture(sample_pos)).x;
		sample_color = vec4(d,d,d,d);
		sample_color.rgb *= sample_color.a;
		sample_pos += ray_dir * ray_step;
		
		count = count + 1;
	}
	//vec4 T = texture3D(volume,v_position.xyz);
	//gl_FragColor = vec4(T,1.0);
	gl_FragColor = final_color;
}
