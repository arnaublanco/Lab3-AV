#ifndef SCENENODE_H
#define SCENENODE_H

#include "framework.h"

#include "shader.h"
#include "mesh.h"
#include "camera.h"
#include "material.h"

class SceneNode {
public:

	static unsigned int lastNameId;

	SceneNode();
	SceneNode(const char* name);
	~SceneNode();

	Material * material = NULL;
	std::string name;

	Mesh* mesh = NULL;
	Matrix44 model;

	virtual void render(Camera* camera);
	virtual void renderWireframe(Camera* camera);
	virtual void renderInMenu();
};

class Light : public SceneNode {
public:
	Vector3 position = model.getTranslation();
	Vector3 diffuseLight = vec3(1.0, 1.0, 1.0);
	Vector3 specularLight = vec3(1.0, 1.0, 1.0);
	Vector3 ambientLight = vec3(1.0, 1.0, 1.0);

	Shader* shader = NULL;

	Light(const char* name);

	void setUniforms();
	void renderInMenu();

};

#endif