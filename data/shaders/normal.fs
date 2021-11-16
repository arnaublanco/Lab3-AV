#define N_MAX 1000

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
uniform float epsilon;
uniform float brightness;
uniform sampler2D u_jitter_texture;

uniform float a;
uniform float b;
uniform float c;

uniform float x0;
uniform float y0;
uniform float z0;


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
	float d_plane = -(a*x0 + b*y0 + c*z0);
	//float plane = a*v_world_position.x + b*v_world_position.y + c*v_world_position.z + d_plane;
	float texture_width = 1.0;
	float offset = texture2D(u_jitter_texture, gl_FragCoord.xy / texture_width).x;

	vec3 ray_dir = normalize(v_world_position - u_camera_position); // Ray direction
	ray_dir = world_to_local(ray_dir);
	vec3 sample_pos = v_position + ray_dir * offset; // Sample position

	float d = texture3D(volume, local_to_texture(sample_pos)).x; // Density of sample position
	vec4 sample_color = vec4(d,d,d,d);
	vec4 final_color = vec4(0.0);

	int count = 0;
	while(sample_pos.x > -1.0 && sample_pos.y > -1.0 && sample_pos.z > -1.0 && sample_pos.x < 1.0 && sample_pos.y < 1.0 && sample_pos.z < 1.0 && final_color.a <= 1.0 && count <= N_MAX){
		
		if(a*sample_pos.x + b*sample_pos.y + c*sample_pos.z + d_plane <= 0){
			final_color += ray_step * (1.0 - final_color.a) * sample_color;
			d = texture3D(volume, local_to_texture(sample_pos)).x;
			sample_color = vec4(d,d,d,d);
			sample_color.rgb *= sample_color.a;
		}
		sample_pos += ray_dir * ray_step;
	
		count = count + 1;
	}

	//final_color.x < epsilon || final_color.y < epsilon || 
	if(final_color.a < epsilon){
		discard;
	}
		
	//vec4 T = texture3D(volume,v_position.xyz);
	//gl_FragColor = vec4(T,1.0);
	gl_FragColor = brightness*final_color;
}
