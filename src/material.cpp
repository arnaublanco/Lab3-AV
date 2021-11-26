#include "material.h"
#include "texture.h"
#include "application.h"
#include "extra/hdre.h"

StandardMaterial::StandardMaterial()
{
	color = vec4(1.f, 1.f, 1.f, 1.f);
	shader = Shader::Get("data/shaders/basic.vs", "data/shaders/flat.fs");
}

StandardMaterial::~StandardMaterial()
{

}

void StandardMaterial::setUniforms(Camera* camera, Matrix44 model)
{
	//upload node uniforms
	shader->setUniform("u_viewprojection", camera->viewprojection_matrix);
	shader->setUniform("u_camera_position", camera->eye);
	shader->setUniform("u_model", model);
	shader->setUniform("u_time", Application::instance->time);
	shader->setUniform("u_output", Application::instance->output);

	shader->setUniform("u_color", color);
	shader->setUniform("u_exposure", Application::instance->scene_exposure);

	if (texture)
		shader->setUniform("u_texture", texture);
}

void StandardMaterial::render(Mesh* mesh, Matrix44 model, Camera* camera)
{
	if (mesh && shader)
	{
		//enable shader
		shader->enable();

		//upload uniforms
		setUniforms(camera, model);

		//do the draw call
		mesh->render(GL_TRIANGLES);

		//disable shader
		shader->disable();
	}
}

VolumeMaterial::VolumeMaterial() {

}

VolumeMaterial::~VolumeMaterial() {

}

void VolumeMaterial::render(Mesh* mesh, Matrix44 model, Camera* camera)
{
	if (mesh && shader)
	{
		glEnable(GL_BLEND);
		glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
		//enable shader
		shader->enable();

		//upload uniforms
		setUniforms(camera, model);

		//do the draw call
		mesh->render(GL_TRIANGLES);

		//disable shader
		shader->disable();
		glDisable(GL_BLEND);
	}
}

void VolumeMaterial::setUniforms(Camera* camera, Matrix44 model)
{

	//upload node uniforms
	shader->setUniform("u_viewprojection", camera->viewprojection_matrix);
	shader->setUniform("u_camera_position", camera->eye);
	shader->setUniform("u_model", model);
	shader->setUniform("u_time", Application::instance->time);
	shader->setUniform("u_output", Application::instance->output);
	shader->setUniform("volume", volumes[visualization_type], 0);
	shader->setUniform("u_phong", phong);
	shader->setUniform("u_transfer_function", transfer_function);

	Matrix44 inv_model = model;
	inv_model.inverse();

	shader->setUniform("inv_model", inv_model);
	shader->setUniform("ray_step", ray_step);
	shader->setUniform("brightness", brightness);

	shader->setUniform("x0", x0);
	shader->setUniform("y0", y0);
	shader->setUniform("z0", z0);

	shader->setUniform("a", n[0]);
	shader->setUniform("b", n[1]);
	shader->setUniform("c", n[2]);

	shader->setUniform("h", h);
	shader->setUniform("thrIsosurface", thrIsosurface);

	shader->setUniform("u_exposure", Application::instance->scene_exposure);
	shader->setUniform("u_jitter_texture", jitterTexture, 1);
	shader->setUniform("u_tfLUT", tfLUT, 2);

	shader->setUniform("texture_width", texture_width);
	shader->setUniform("u_jittering", jittering);

	shader->setUniform("diffuseMaterial", diffuseMaterial);
	shader->setUniform("ambientMaterial", ambientMaterial);
	shader->setUniform("specularMaterial", specularMaterial);

	shader->setUniform("alpha", alpha);

	if (texture)
		shader->setUniform("u_texture", texture);
}

void VolumeMaterial::renderInMenu()
{
	ImGui::ColorEdit3("Color", (float*)&color); // Edit 3 floats representing a color
	ImGui::DragFloat("Ray step", (float*)&ray_step, 0.005, 0.005, 2.0);
	ImGui::DragFloat("Brightness", (float*)&brightness, 0.1, 0.0, 10);
	ImGui::DragFloat3("Normal vector", (float*)&n, 0.1, 0.0, 1.0);

	float v[3] = { x0, y0, z0 };
	ImGui::DragFloat3("x0 | y0 | z0", (float*)&v, 0.1, -10.0, 10.0);
	x0 = v[0];
	y0 = v[1];
	z0 = v[2];

	ImGui::DragFloat("h", (float*)&h, 0.005, 0.001, 1.0);
	ImGui::DragFloat("Threshold Isosurface", (float*)&thrIsosurface, 0.01, 0.0, 1.0);
	ImGui::DragFloat("Texture width", (float*)&texture_width, 1.0, 0.0, 100.0);

	ImGui::DragFloat("Alpha", (float*)&alpha, 0.1, 0.1, 5.0);

	ImGui::DragFloat3("Ambient", (float*)&ambientMaterial, 0.01f, 0.0f, 1.0f);
	ImGui::DragFloat3("Diffuse", (float*)&diffuseMaterial, 0.01f, 0.0f, 1.0f);
	ImGui::DragFloat3("Specular", (float*)&specularMaterial, 0.01f, 0.0f, 1.0f);
}

void StandardMaterial::renderInMenu()
{
	ImGui::ColorEdit3("Color", (float*)&color); // Edit 3 floats representing a color
}

WireframeMaterial::WireframeMaterial()
{
	color = vec4(1.f, 1.f, 1.f, 1.f);
	shader = Shader::Get("data/shaders/basic.vs", "data/shaders/flat.fs");
}

WireframeMaterial::~WireframeMaterial()
{

}

void WireframeMaterial::render(Mesh* mesh, Matrix44 model, Camera * camera)
{
	if (shader && mesh)
	{
		glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);

		//enable shader
		shader->enable();

		//upload material specific uniforms
		setUniforms(camera, model);

		//do the draw call
		mesh->render(GL_TRIANGLES);

		glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
	}
}