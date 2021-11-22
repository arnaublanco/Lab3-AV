#define N_MAX 1000000

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
uniform float h;

uniform sampler2D u_jitter_texture;
uniform sampler2D u_tfLUT;

uniform float a;
uniform float b;
uniform float c;

uniform float x0;
uniform float y0;
uniform float z0;

uniform float u_vis_type;

// Function to convert from local to texture coordinates
vec3 local_to_texture(vec3 v_local){
	vec3 v_texture = (v_local + 1.0)/2.0;
	return v_texture;
}

// Function to convert from world to local coordinates
vec3 world_to_local(vec3 v_in){
	vec4 aux = inv_model * vec4(v_in, 1.0);
	aux /= aux.a;
	vec3 v_out = aux.xyz;
	return v_out;
}

vec3 computeGradient(vec3 sample_pos){

	float n_x = texture3D(volume, local_to_texture(vec3(sample_pos.x + h, sample_pos.y, sample_pos.z))).x - texture3D(volume, local_to_texture(vec3(sample_pos.x - h, sample_pos.y, sample_pos.z))).x;
	float n_y = texture3D(volume, local_to_texture(vec3(sample_pos.x, sample_pos.y + h, sample_pos.z))).x - texture3D(volume, local_to_texture(vec3(sample_pos.x, sample_pos.y - h, sample_pos.z))).x;
	float n_z = texture3D(volume, local_to_texture(vec3(sample_pos.x, sample_pos.y, sample_pos.z + h))).x - texture3D(volume, local_to_texture(vec3(sample_pos.x, sample_pos.y, sample_pos.z - h))).x;

	return vec3(n_x,n_y,n_z)/(2.0*h); 
}

void main()
{
	vec4 final_color = vec4(0.0);
	float d_plane = -(a*x0 + b*y0 + c*z0); // d in the plane equation

	vec3 ray_dir = normalize(v_world_position - u_camera_position); // Ray direction
	ray_dir = world_to_local(ray_dir); // Convert ray direction in world coordinates to local coordinates
	vec3 sample_pos = v_position;

	float texture_width = 1.0; // Texture width
	float offset = texture2D(u_jitter_texture, gl_FragCoord.xy / texture_width).x; // Offset for the jittering

	sample_pos += ray_dir * offset; // Sample position with jittering offset

	float d = texture3D(volume, local_to_texture(sample_pos)).x; // Density of sample position
	vec2 v = vec2(clamp(d,0.01,0.99),0.0); // 2-D vector for retrieving color based on the density

	vec4 sample_color = vec4(0.0);

	// Check if the sample position is inside the boundaries of the box
	for(int count = 0; count <= N_MAX; count++){
		
		// If the samples are located below (<0) the plane
		if(a*sample_pos.x + b*sample_pos.y + c*sample_pos.z + d_plane <= 0.0){

			final_color += ray_step * (1.0 - final_color.a) * sample_color; // Compute final color (accumulation)

			d = texture3D(volume, local_to_texture(sample_pos)).x; // Calculate density based on the sample position

			if(u_vis_type == 1.0){
				sample_color = texture2D(u_tfLUT,v);
			}else{
				sample_color = vec4(d,d,d,d);
			}
			
			v = vec2(clamp(d,0.01,0.99),0.0); // 2-D for color retrieval
			sample_color.rgb *= sample_color.a; // Homogeneous coordinates
		}
		sample_pos += ray_dir * ray_step; // Update sample position
	
		if(any(lessThanEqual(sample_pos.xyz, vec3(-1.0))) || any(greaterThanEqual(sample_pos.xyz, vec3(1.0))) || final_color.a >= 1.0){
			break;
		}

		final_color *= brightness; // Add a brightness factor to final color
		
	}

		//while(all(greaterThan(sample_pos.xyz, vec3(-1.0))) && all(lessThan(sample_pos.xyz, vec3(1.0))) && final_color.a <= 1.0){
		//	if(a*sample_pos.x + b*sample_pos.y + c*sample_pos.z + d_plane <= 0 ){
		//		final_color = vec4(computeGradient(sample_pos),1.0);
		//	}
		//	sample_pos += ray_dir * ray_step; // Update sample position
		//	count = count + 1; // Add up counter
		//}

	gl_FragColor = final_color;

}
