#ifndef MATERIAL_H
#define MATERIAL_H

#include "framework.h"
#include "shader.h"
#include "camera.h"
#include "mesh.h"
#include "extra/hdre.h"

class Material {
public:

	Shader* shader = NULL;
	Texture* texture = NULL;
	vec4 color;

	virtual void setUniforms(Camera* camera, Matrix44 model) = 0;
	virtual void render(Mesh* mesh, Matrix44 model, Camera * camera) = 0;
	virtual void renderInMenu() = 0;
};

class StandardMaterial : public Material {
public:

	StandardMaterial();
	~StandardMaterial();

	void setUniforms(Camera* camera, Matrix44 model);
	void render(Mesh* mesh, Matrix44 model, Camera * camera);
	void renderInMenu();
};

class VolumeMaterial : public StandardMaterial {
public:

	Texture* volumes[4] = { NULL, NULL, NULL, NULL };
	Texture* jitterTexture;
	Texture* tfLUT;

	float ray_step = 0.005;
	float brightness = 4.0;
	float h = 0.01;

	int visualization_type = 0;
	bool transfer_function = false;
	bool phong = false;
	bool jittering = false;

	// Plane definition
	float x0 = 0.0;
	float y0 = 0.0;
	float z0 = 2.0;
	float n[3] = { 0.0, 0.0, 1.0 };
	
	float thrIsosurface = 0.4;
	float texture_width = 1.0;

	float alpha = 0.5;

	Vector3 diffuseMaterial = vec3(1.0, 1.0, 1.0);
	Vector3 specularMaterial = vec3(1.0, 1.0, 1.0);
	Vector3 ambientMaterial = vec3(1.0, 1.0, 1.0);

	VolumeMaterial();
	~VolumeMaterial();

	void setUniforms(Camera* camera, Matrix44 model);
	void render(Mesh* mesh, Matrix44 model, Camera* camera);
	void renderInMenu();
};

class WireframeMaterial : public StandardMaterial {
public:

	WireframeMaterial();
	~WireframeMaterial();

	void render(Mesh* mesh, Matrix44 model, Camera * camera);
};

#endif