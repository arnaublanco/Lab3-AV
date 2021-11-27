#define N_MAX 1000000
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

uniform bool u_transfer_function;
uniform bool u_phong;
uniform float thrIsosurface;

uniform vec4 u_color;

uniform bool u_jittering;
uniform float alpha;

uniform vec3 ambientLight;
uniform vec3 specularLight;
uniform vec3 diffuseLight;

uniform vec3 ambientMaterial;
uniform vec3 specularMaterial;
uniform vec3 diffuseMaterial;

uniform vec3 light_pos;

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

// Function to convert from local to world coordinates
vec3 local_to_world(vec3 v_in){
	vec4 aux = model * vec4(v_in, 1.0);
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

vec4 computePhong(vec3 N, vec3 sample_pos, vec4 color){

	// 1st term
	vec3 first_term = ambientMaterial*ambientLight;

	// 2nd term
	vec3 lightVector = normalize(world_to_local(light_pos) - sample_pos);
	float L_dot_N = max(0.0,dot(lightVector,N));
	vec3 second_term = diffuseMaterial*L_dot_N*diffuseLight;

	// 3rd term
	vec3 R = reflect(lightVector,N);
	vec3 V = normalize(world_to_local(u_camera_position) - sample_pos);
	float R_dot_V = max(0.0,dot(R,V));
	float power = pow(R_dot_V,alpha);
	vec3 third_term = specularMaterial*power*specularLight;
	
	return color * vec4(first_term + second_term + third_term,1.0);
}

void main()
{
	vec4 final_color = vec4(0.0);
	float d_plane = -(a*x0 + b*y0 + c*z0); // d in the plane equation

	vec3 u_camera_position_local = world_to_local(u_camera_position);
	vec3 ray_dir = normalize(v_position - u_camera_position_local); // Ray direction
	vec3 sample_pos = v_position;

	float offset = 0.0;
	float texture_width = 1.0;
	if(u_jittering){
		offset = texture2D(u_jitter_texture, gl_FragCoord.xy/texture_width).x; // Offset for the jittering
	}

	sample_pos += ray_dir * offset * ray_step; // Sample position with jittering offset

	float d = texture3D(volume, local_to_texture(sample_pos)).x; // Density of sample position
	vec2 v = vec2(d,0.0); // 2-D vector for retrieving color based on the density
	vec4 sample_color = vec4(0.0);

	for(int count = 0; count <= N_MAX; count++){

		// Check if the sample position is inside the boundaries of the box
		if(any(lessThan(sample_pos.xyz, vec3(-1.0))) || any(greaterThan(sample_pos.xyz, vec3(1.0))) || final_color.a >= 1.0){
			break;
		}
		
		// If the samples are located below (<0) the plane
		if(a*sample_pos.x + b*sample_pos.y + c*sample_pos.z + d_plane < 0){
			d = texture3D(volume, local_to_texture(sample_pos)).x; // Calculate density based on the sample position
			v = vec2(d,0.0); // 2-D for color retrieval

			if(u_transfer_function){
				sample_color = texture2D(u_tfLUT,v); // Sample color based on the density
			}else{
				sample_color = vec4(d,d,d,d);
			}

			if(u_phong && d >= thrIsosurface){
				vec3 gradient = normalize(computeGradient(sample_pos));
				final_color = computePhong(-gradient,sample_pos,u_color);
			}else if(!u_phong){
				final_color += ray_step * (1.0 - final_color.a) * sample_color; // Compute final color (accumulation)
			}
			
			sample_color.rgb *= sample_color.a; // Homogeneous coordinates
		}
		sample_pos += ray_dir * ray_step; // Update sample position
	} 

	if(!u_phong)
		final_color *= brightness; // Add a brightness factor to final color

	gl_FragColor = final_color;

}
