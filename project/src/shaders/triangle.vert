#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(binding = 0) uniform UniformBufferObject {
    mat4 model;
    mat4 view;
    mat4 projection;
} ubo;

layout(location = 0) in vec2 inPosition;
layout(location = 1) in vec3 inColour;

layout(location = 0) out vec3 fragColour;

void main() {
    mat4 world = ubo.projection * ubo.view * ubo.model;
    gl_Position = world * vec4(inPosition, 0.0, 1.0);
    fragColour = inColour;
}
