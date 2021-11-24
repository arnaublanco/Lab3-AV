#include "scenenode.h"
#include "application.h"
#include "texture.h"
#include "utils.h"

unsigned int SceneNode::lastNameId = 0;
int visualization_type = 0;
bool transfer_function = false;
bool phong = false;

SceneNode::SceneNode()
{
	this->name = std::string("Node" + std::to_string(lastNameId++));
}


SceneNode::SceneNode(const char * name)
{
	this->name = name;
}

SceneNode::~SceneNode()
{

}

void SceneNode::render(Camera* camera)
{
	if (material)
		material->render(mesh, model, camera);
}

void SceneNode::renderWireframe(Camera* camera)
{
	WireframeMaterial mat = WireframeMaterial();
	mat.render(mesh, model, camera);
}

void SceneNode::renderInMenu()
{
	//Model edit
	if (ImGui::TreeNode("Model")) 
	{
		float matrixTranslation[3], matrixRotation[3], matrixScale[3];
		ImGuizmo::DecomposeMatrixToComponents(model.m, matrixTranslation, matrixRotation, matrixScale);
		ImGui::DragFloat3("Position", matrixTranslation, 0.1f);
		ImGui::DragFloat3("Rotation", matrixRotation, 0.1f);
		ImGui::DragFloat3("Scale", matrixScale, 0.1f);
		ImGuizmo::RecomposeMatrixFromComponents(matrixTranslation, matrixRotation, matrixScale, model.m);
		
		ImGui::TreePop();
	}

	//Material
	if (material && ImGui::TreeNode("Material"))
	{
		material->renderInMenu();

		bool changed_vis = false;
		changed_vis |= ImGui::Combo("Type of visualization", (int*)&visualization_type, "BONSAI\0FOOT\0BRAIN\0TEAPOT\0");

		if (changed_vis) {
			VolumeMaterial* mat = (VolumeMaterial*)material;
			mat->visualization_type = visualization_type;
		}

		bool changed_tp = false;
		changed_tp |= ImGui::Checkbox("Transfer function", (bool*)&transfer_function);
		changed_tp |= ImGui::Checkbox("Phong", (bool*)&phong);

		if (changed_tp) {
			VolumeMaterial* mat = (VolumeMaterial*)material;
			mat->transfer_function = transfer_function;
			mat->phong = phong;
		}

		ImGui::TreePop();
	}
}
