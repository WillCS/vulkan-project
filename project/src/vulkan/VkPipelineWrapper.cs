using System.Runtime.InteropServices;
using System.Linq;
using Vk = Vulkan;
using System.Collections.Generic;
using Project.Native;

namespace Project.Vulkan {
    public class VkPipelineWrapper {
        public Vk.SwapchainKhr    Swapchain;

        public Vk.Format          Format;
        public Vk.Extent2D        Extent;

        public uint               ImageCapacity {
            get => (uint) this.Images.Length;
        }

        public Vk.Image[]         Images;
        public Vk.ImageView[]     ImageViews;
        public Vk.Framebuffer[]   Framebuffers;
        public Vk.Buffer[]        UniformBuffers;
        public Vk.DeviceMemory[]  UniformBuffersMemory;
        public Vk.DescriptorPool  DescriptorPool;
        public Vk.DescriptorSet[] DescriptorSets;

        public Vk.Image           DepthImage;
        public Vk.DeviceMemory    DepthImageMemory;
        public Vk.ImageView       DepthImageView;

        public Vk.RenderPass      RenderPass;
        public Vk.PipelineLayout  PipelineLayout;
        public Vk.Pipeline        Pipeline;

        public Vk.SharingMode     SharingMode;

        public Vk.CommandBuffer[] CommandBuffers;

        public void Cleanup(VkContext context) {
            // Destroy Framebuffers
            foreach(Vk.Framebuffer framebuffer in this.Framebuffers) {
                context.Device.DestroyFramebuffer(framebuffer);
            }

            // Free Command Buffers
            context.Device.FreeCommandBuffers(context.GraphicsCommandPool, this.CommandBuffers);

            // Destroy Pipeline
            context.Device.DestroyPipeline(this.Pipeline);

            // Destroy Pipeline Layout
            context.Device.DestroyPipelineLayout(this.PipelineLayout);

            // Destroy Render Pass
            context.Device.DestroyRenderPass(this.RenderPass);

            // Destroy Image Views
            foreach(Vk.ImageView imageView in this.ImageViews) {
                context.Device.DestroyImageView(imageView);
            }

            // Destroy Uniform Buffers and free their memory
            for(int i = 0; i < this.ImageCapacity; i++) {
                context.Device.DestroyBuffer(this.UniformBuffers[i]);
                context.Device.FreeMemory(this.UniformBuffersMemory[i]);
            }

            // Destroy Depth Image View
            context.Device.DestroyImageView(this.DepthImageView);
            context.Device.FreeMemory(this.DepthImageMemory);

            // Destroy Descriptor Pool
            context.Device.DestroyDescriptorPool(this.DescriptorPool);

            // Destroy Swapchain
            context.Device.DestroySwapchainKHR(this.Swapchain);
        }
    }

    public delegate void RenderPass(Vk.CommandBuffer buffer);

    public class GraphicsPipelineBuilder {
        private Vk.SurfaceFormatKhr       surfaceFormat;
        private Vk.PresentModeKhr         presentMode;
        private Vk.SurfaceCapabilitiesKhr capabilities;
        private Vk.Extent2D               imageExtent;
        private Vk.DescriptorSetLayout    descriptorSetLayout;

        private Vk.ShaderModule           vertexShader;
        private Vk.ShaderModule           fragmentShader;

        private uint                      graphicsFamilyQueueIndex;
        private uint                      presentFamilyQueueIndex;

        private RenderPass                renderPassCallback;
        private VkPipelineWrapper         wrapper;

        public GraphicsPipelineBuilder() {
            this.wrapper = new VkPipelineWrapper();
        }

        public void SetExtent(Vk.Extent2D extent) {
            this.wrapper.Extent = extent;
        }

        public void SetSurfaceFormat(Vk.SurfaceFormatKhr format) {
            this.surfaceFormat = format;
            this.wrapper.Format = format.Format;
        }

        public void SetPresentMode(Vk.PresentModeKhr presentMode) {
            this.presentMode = presentMode;
        }

        public void SetCapabilities(Vk.SurfaceCapabilitiesKhr capabilities) {
            this.capabilities = capabilities;
        }

        public void SetGraphicsFamilyQueueIndex(uint queueIndex) {
            this.graphicsFamilyQueueIndex = queueIndex;
        }

        public void SetPresentFamilyQueueIndex(uint queueIndex) {
            this.presentFamilyQueueIndex = queueIndex;
        }

        public void SetDescriptorSetLayout(Vk.DescriptorSetLayout layout) {
            this.descriptorSetLayout = layout;
        }

        public void SetVertexShader(Vk.ShaderModule shader) {
            this.vertexShader = shader;
        }

        public void SetFragmentShader(Vk.ShaderModule shader) {
            this.fragmentShader = shader;
        }

        public void SetRenderPassCallback(RenderPass renderPass) {
            this.renderPassCallback = renderPass;
        }

        public VkPipelineWrapper Create(VkContext context) {
            this.createSwapchain(context);
            this.wrapper.Images = context.Device.GetSwapchainImagesKHR(this.wrapper.Swapchain);
            this.createImageViews(context);
            this.createUniformBuffers(context);
            this.createDescriptorPool(context);
            this.createDescriptorSets(context);
            this.createRenderPass(context);
            this.createGraphicsPipeline(context);
            this.createDepthResources(context);
            this.createFramebuffers(context);
            this.createCommandBuffers(context);

            return this.wrapper;
        }

        private void createSwapchain(VkContext context) {
            var minImageCount    = this.capabilities.MinImageCount;
            var maxImageCount    = this.capabilities.MaxImageCount;
            var currentTransform = this.capabilities.CurrentTransform;

            uint imageCount = minImageCount + 1;

            if(maxImageCount > 0 && imageCount > maxImageCount) {
                imageCount = maxImageCount;
            }

            Vk.SwapchainCreateInfoKhr createInfo = new Vk.SwapchainCreateInfoKhr();
            createInfo.Surface          = context.Surface;
            createInfo.MinImageCount    = imageCount;
            createInfo.ImageFormat      = this.wrapper.Format;
            createInfo.ImageColorSpace  = this.surfaceFormat.ColorSpace;
            createInfo.ImageExtent      = this.wrapper.Extent;
            createInfo.ImageArrayLayers = 1;
            createInfo.ImageUsage       = Vk.ImageUsageFlags.ColorAttachment;

            this.wrapper.SharingMode = context.GetSharingMode();

            if(this.wrapper.SharingMode == Vk.SharingMode.Concurrent) {
                createInfo.QueueFamilyIndexCount = 2;
                createInfo.QueueFamilyIndices = new uint[] {
                    this.graphicsFamilyQueueIndex,
                    this.presentFamilyQueueIndex
                };
            } else {
                createInfo.QueueFamilyIndexCount = 0;
                createInfo.QueueFamilyIndices = null;
            }

            createInfo.ImageSharingMode = this.wrapper.SharingMode;
            createInfo.PreTransform     = currentTransform;
            createInfo.CompositeAlpha   = Vk.CompositeAlphaFlagsKhr.Opaque; // Blending with other windows? :o
            createInfo.PresentMode      = this.presentMode;
            createInfo.Clipped          = true;
            createInfo.OldSwapchain     = null;

            try {
                this.wrapper.Swapchain = context.Device.CreateSwapchainKHR(createInfo);
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating the swapchain.", result);
            }
        }

        private void createImageViews(VkContext context) {
            var imageViews = new Vk.ImageView[this.wrapper.ImageCapacity];

            for(int i = 0; i < this.wrapper.ImageCapacity; i++) {
                Vk.ImageViewCreateInfo createInfo = new Vk.ImageViewCreateInfo();
                createInfo.Image = this.wrapper.Images[i];
                createInfo.ViewType = Vk.ImageViewType.View2D;
                createInfo.Format = this.wrapper.Format;

                Vk.ComponentMapping componentMapping = new Vk.ComponentMapping();
                componentMapping.R = Vk.ComponentSwizzle.Identity;
                componentMapping.G = Vk.ComponentSwizzle.Identity;
                componentMapping.B = Vk.ComponentSwizzle.Identity;
                componentMapping.A = Vk.ComponentSwizzle.Identity;

                createInfo.Components = componentMapping;

                Vk.ImageSubresourceRange subresourceRange = new Vk.ImageSubresourceRange();
                subresourceRange.AspectMask     = Vk.ImageAspectFlags.Color;
                subresourceRange.BaseMipLevel   = 0;
                subresourceRange.LevelCount     = 1;
                subresourceRange.BaseArrayLayer = 0;
                subresourceRange.LayerCount     = 1;

                createInfo.SubresourceRange = subresourceRange;

                try {
                    imageViews[i] = context.Device.CreateImageView(createInfo);
                } catch(Vk.ResultException result) {
                    throw new VkException($"An error occurred while creating image view {i}.", result);
                }
            }
            
            this.wrapper.ImageViews = imageViews;
        }

        private void createUniformBuffers(VkContext context) {
            var bufferSize = Marshal.SizeOf<UniformBufferObject>();

            var usageFlags = Vk.BufferUsageFlags.UniformBuffer;
            var memProps   = Vk.MemoryPropertyFlags.HostVisible
                    | Vk.MemoryPropertyFlags.HostCoherent;

            var buffers = new BufferWithMemory[this.wrapper.ImageCapacity];

            for(int i = 0; i < this.wrapper.ImageCapacity; i++) {
                try {
                    buffers[i] = VkHelper.CreateBuffer(context, bufferSize, usageFlags, 
                            memProps, this.wrapper.SharingMode);
                } catch(Vk.ResultException result) {
                    throw new VkException($"An error occurred while creating uniform buffer {i}.", result);
                }
            }

            this.wrapper.UniformBuffers       = new Vk.Buffer[this.wrapper.ImageCapacity];
            this.wrapper.UniformBuffersMemory = new Vk.DeviceMemory[this.wrapper.ImageCapacity];

            for(int i = 0; i < this.wrapper.ImageCapacity; i++) {
                this.wrapper.UniformBuffers[i]       = buffers[i].Buffer;
                this.wrapper.UniformBuffersMemory[i] = buffers[i].Memory;
            }
        }

        private void createDescriptorPool(VkContext context) {
            var poolSize = new Vk.DescriptorPoolSize();
            poolSize.Type = Vk.DescriptorType.UniformBuffer;
            poolSize.DescriptorCount = this.wrapper.ImageCapacity;

            var poolInfo = new Vk.DescriptorPoolCreateInfo();
            poolInfo.PoolSizeCount = 1;
            poolInfo.PoolSizes     = new Vk.DescriptorPoolSize[] { poolSize };
            poolInfo.MaxSets       = this.wrapper.ImageCapacity;

            try {
                this.wrapper.DescriptorPool = context.Device.CreateDescriptorPool(poolInfo);
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating the descriptor pool.", result);
            }
        }

        private void createDescriptorSets(VkContext context) {
            var allocInfo = new Vk.DescriptorSetAllocateInfo();
            allocInfo.DescriptorPool = this.wrapper.DescriptorPool;
            allocInfo.DescriptorSetCount = this.wrapper.ImageCapacity;
            allocInfo.SetLayouts =  (from i in Enumerable.Range(0, (int) this.wrapper.ImageCapacity) 
                select this.descriptorSetLayout).ToArray();

            try {
                this.wrapper.DescriptorSets = context.Device.AllocateDescriptorSets(allocInfo);
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating the descriptor sets.", result);
            }

            for(int i = 0; i < this.wrapper.ImageCapacity; i++) {
                var bufferInfo = new Vk.DescriptorBufferInfo();
                bufferInfo.Buffer = this.wrapper.UniformBuffers[i];
                bufferInfo.Offset = 0;
                bufferInfo.Range  = Marshal.SizeOf<UniformBufferObject>();

                var descriptorWrite = new Vk.WriteDescriptorSet();
                descriptorWrite.DstSet          = this.wrapper.DescriptorSets[i];
                descriptorWrite.DstBinding      = 0;
                descriptorWrite.DstArrayElement = 0;
                descriptorWrite.DescriptorType  = Vk.DescriptorType.UniformBuffer;
                descriptorWrite.DescriptorCount = 1;
                descriptorWrite.BufferInfo      = new Vk.DescriptorBufferInfo[] { bufferInfo };

                context.Device.UpdateDescriptorSet(descriptorWrite, null);
            }
        }

        private void createRenderPass(VkContext context) {
            var colourAttachment = new Vk.AttachmentDescription();
            colourAttachment.Format         = this.wrapper.Format;
            colourAttachment.Samples        = Vk.SampleCountFlags.Count1;
            colourAttachment.LoadOp         = Vk.AttachmentLoadOp.Clear;
            colourAttachment.StoreOp        = Vk.AttachmentStoreOp.Store;
            colourAttachment.StencilLoadOp  = Vk.AttachmentLoadOp.DontCare;
            colourAttachment.StencilStoreOp = Vk.AttachmentStoreOp.DontCare;
            colourAttachment.InitialLayout  = Vk.ImageLayout.Undefined;
            colourAttachment.FinalLayout    = Vk.ImageLayout.PresentSrcKhr;

            var colourAttachmentRef = new Vk.AttachmentReference();
            colourAttachmentRef.Attachment = 0;
            colourAttachmentRef.Layout     = Vk.ImageLayout.ColorAttachmentOptimal;

            var depthAttachment = new Vk.AttachmentDescription();
            depthAttachment.Format         = VkHelper.FindDepthFormat(context);
            depthAttachment.Samples        = Vk.SampleCountFlags.Count1;
            depthAttachment.LoadOp         = Vk.AttachmentLoadOp.Clear;
            depthAttachment.StoreOp        = Vk.AttachmentStoreOp.DontCare;
            depthAttachment.StencilLoadOp  = Vk.AttachmentLoadOp.DontCare;
            depthAttachment.StencilStoreOp = Vk.AttachmentStoreOp.DontCare;
            depthAttachment.InitialLayout  = Vk.ImageLayout.Undefined;
            depthAttachment.FinalLayout    = Vk.ImageLayout.DepthStencilAttachmentOptimal;

            var depthAttachmentRef = new Vk.AttachmentReference();
            depthAttachmentRef.Attachment = 1;
            depthAttachmentRef.Layout     = Vk.ImageLayout.DepthStencilAttachmentOptimal;

            var subpass = new Vk.SubpassDescription();
            subpass.PipelineBindPoint      = Vk.PipelineBindPoint.Graphics;
            subpass.ColorAttachmentCount   = 1;
            subpass.ColorAttachments       = new Vk.AttachmentReference[] {
                colourAttachmentRef
            };
            subpass.DepthStencilAttachment = depthAttachmentRef;

            var subpassDep = new Vk.SubpassDependency();
            subpassDep.SrcSubpass    = VkConstants.VK_SUBPASS_EXTERNAL;
            subpassDep.DstSubpass    = 0;
            subpassDep.SrcStageMask  = Vk.PipelineStageFlags.ColorAttachmentOutput;
            subpassDep.DstStageMask  = Vk.PipelineStageFlags.ColorAttachmentOutput;
            subpassDep.SrcAccessMask = 0;
            subpassDep.DstAccessMask = Vk.AccessFlags.ColorAttachmentRead | Vk.AccessFlags.ColorAttachmentWrite;

            var renderPassInfo = new Vk.RenderPassCreateInfo();
            renderPassInfo.AttachmentCount = 2;
            renderPassInfo.Attachments     = new Vk.AttachmentDescription[] {
                colourAttachment,
                depthAttachment
            };
            renderPassInfo.SubpassCount    = 1;
            renderPassInfo.Subpasses       = new Vk.SubpassDescription[] {
                subpass
            };
            renderPassInfo.DependencyCount = 1;
            renderPassInfo.Dependencies    = new Vk.SubpassDependency[] {
                subpassDep
            };

            try {
                this.wrapper.RenderPass = context.Device.CreateRenderPass(renderPassInfo);
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating a render pass.", result);
            }
        }

        private void createGraphicsPipeline(VkContext context) {
            var vertShaderStageInfo = new Vk.PipelineShaderStageCreateInfo();
            vertShaderStageInfo.Stage  = Vk.ShaderStageFlags.Vertex;
            vertShaderStageInfo.Module = this.vertexShader;
            vertShaderStageInfo.Name   = "main";

            var fragShaderStageInfo = new Vk.PipelineShaderStageCreateInfo();
            fragShaderStageInfo.Stage  = Vk.ShaderStageFlags.Fragment;
            fragShaderStageInfo.Module = this.fragmentShader;
            fragShaderStageInfo.Name   = "main";

            var shaderStageInfos = new Vk.PipelineShaderStageCreateInfo[] {
                vertShaderStageInfo,
                fragShaderStageInfo
            };

            var attributes = Vertex.AttributeDescriptions;
            var bindings   = new Vk.VertexInputBindingDescription[] {
                Vertex.BindingDescription
            };

            var vertexInputInfo = new Vk.PipelineVertexInputStateCreateInfo();
            vertexInputInfo.VertexBindingDescriptionCount   = (uint) bindings.Length;
            vertexInputInfo.VertexBindingDescriptions       = bindings;
            vertexInputInfo.VertexAttributeDescriptionCount = (uint) attributes.Length;
            vertexInputInfo.VertexAttributeDescriptions     = attributes;

            var inputAssemblyInfo = new Vk.PipelineInputAssemblyStateCreateInfo();
            inputAssemblyInfo.Topology = Vk.PrimitiveTopology.TriangleList;
            inputAssemblyInfo.PrimitiveRestartEnable = false;

            Vk.Viewport viewport = new Vk.Viewport();
            viewport.X           = 0.0F;
            viewport.Y           = 0.0F;
            viewport.Width       = (float) this.wrapper.Extent.Width;
            viewport.Height      = (float) this.wrapper.Extent.Height;
            viewport.MinDepth    = 0.0F;
            viewport.MaxDepth    = 1.0F;

            Vk.Rect2D scissorRect = new Vk.Rect2D();
            scissorRect.Offset.X  = 0;
            scissorRect.Offset.Y  = 0;
            scissorRect.Extent    = this.wrapper.Extent;

            var viewportStateInfo = new Vk.PipelineViewportStateCreateInfo();
            viewportStateInfo.ViewportCount = 1;
            viewportStateInfo.Viewports     = new Vk.Viewport[] { viewport };
            viewportStateInfo.ScissorCount  = 1;
            viewportStateInfo.Scissors      = new Vk.Rect2D[] { scissorRect };

            var rasteriserInfo = new Vk.PipelineRasterizationStateCreateInfo();
            rasteriserInfo.DepthClampEnable        = false;
            rasteriserInfo.RasterizerDiscardEnable = false;
            rasteriserInfo.PolygonMode             = Vk.PolygonMode.Fill;
            rasteriserInfo.LineWidth               = 1.0F;
            rasteriserInfo.CullMode                = Vk.CullModeFlags.Back;
            rasteriserInfo.FrontFace               = Vk.FrontFace.Clockwise;
            rasteriserInfo.DepthBiasEnable         = false;

            var multisamplingInfo = new Vk.PipelineMultisampleStateCreateInfo();
            multisamplingInfo.SampleShadingEnable = false;
            multisamplingInfo.RasterizationSamples = Vk.SampleCountFlags.Count1;

            var colourBlendAttachmentInfo = new Vk.PipelineColorBlendAttachmentState();
            colourBlendAttachmentInfo.ColorWriteMask = 
                Vk.ColorComponentFlags.R | 
                Vk.ColorComponentFlags.G | 
                Vk.ColorComponentFlags.B | 
                Vk.ColorComponentFlags.A;
            colourBlendAttachmentInfo.BlendEnable         = true;
            colourBlendAttachmentInfo.SrcColorBlendFactor = Vk.BlendFactor.SrcAlpha;
            colourBlendAttachmentInfo.DstColorBlendFactor = Vk.BlendFactor.OneMinusSrcAlpha;
            colourBlendAttachmentInfo.ColorBlendOp        = Vk.BlendOp.Add;
            colourBlendAttachmentInfo.SrcAlphaBlendFactor = Vk.BlendFactor.One;
            colourBlendAttachmentInfo.DstAlphaBlendFactor = Vk.BlendFactor.Zero;
            colourBlendAttachmentInfo.AlphaBlendOp        = Vk.BlendOp.Add;

            var colourBlendAttachmentInfos = new Vk.PipelineColorBlendAttachmentState[] {
                colourBlendAttachmentInfo
            };

            var colourBlendStateInfo = new Vk.PipelineColorBlendStateCreateInfo();
            colourBlendStateInfo.LogicOpEnable   = false;
            colourBlendStateInfo.LogicOp         = Vk.LogicOp.Copy;
            colourBlendStateInfo.AttachmentCount = 1;
            colourBlendStateInfo.Attachments     = colourBlendAttachmentInfos;

            var pipelineLayoutInfo = new Vk.PipelineLayoutCreateInfo();
            pipelineLayoutInfo.SetLayoutCount = 1;
            pipelineLayoutInfo.SetLayouts     = new Vk.DescriptorSetLayout[] { 
                this.descriptorSetLayout 
            };
            
            try {
                this.wrapper.PipelineLayout = context.Device.CreatePipelineLayout(pipelineLayoutInfo);
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating the pipeline layout.", result);
            }

            var depthStencil = new Vk.PipelineDepthStencilStateCreateInfo();
            depthStencil.DepthTestEnable = true;
            depthStencil.DepthWriteEnable = true;
            depthStencil.DepthCompareOp = Vk.CompareOp.Less;
            depthStencil.DepthBoundsTestEnable = false;
            depthStencil.StencilTestEnable = false;

            var pipelineInfo = new Vk.GraphicsPipelineCreateInfo();
            pipelineInfo.StageCount         = 2;
            pipelineInfo.Stages             = shaderStageInfos;
            pipelineInfo.VertexInputState   = vertexInputInfo;
            pipelineInfo.InputAssemblyState = inputAssemblyInfo;
            pipelineInfo.ViewportState      = viewportStateInfo;
            pipelineInfo.RasterizationState = rasteriserInfo;
            pipelineInfo.MultisampleState   = multisamplingInfo;
            pipelineInfo.DepthStencilState  = depthStencil;
            pipelineInfo.ColorBlendState    = colourBlendStateInfo;
            pipelineInfo.DynamicState       = null;
            pipelineInfo.Layout             = this.wrapper.PipelineLayout;
            pipelineInfo.RenderPass         = this.wrapper.RenderPass;
            pipelineInfo.Subpass            = 0;
            pipelineInfo.BasePipelineHandle = null;
            pipelineInfo.BasePipelineIndex  = -1;

            var pipelineInfos = new Vk.GraphicsPipelineCreateInfo[] {
                pipelineInfo
            };

            try {
                this.wrapper.Pipeline = context.Device.CreateGraphicsPipelines(null, pipelineInfos)[0];
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating the graphics pipeline.", result);
            }
        }

        private void createDepthResources(VkContext context) {
            var depthFormat = VkHelper.FindDepthFormat(context);
            var depthImage = VkHelper.CreateImage(context,
                    this.wrapper.Extent.Width, this.wrapper.Extent.Height,
                    depthFormat, Vk.ImageTiling.Optimal, Vk.ImageUsageFlags.DepthStencilAttachment,
                    Vk.MemoryPropertyFlags.DeviceLocal);

            var depthImageView = VkHelper.CreateImageView(context, depthImage.Image, depthFormat,
                    Vk.ImageAspectFlags.Depth);

            context.TransitionImageLayout(depthImage.Image, depthFormat, Vk.ImageLayout.Undefined,
                    Vk.ImageLayout.DepthStencilAttachmentOptimal);

            this.wrapper.DepthImage       = depthImage.Image;
            this.wrapper.DepthImageMemory = depthImage.Memory;
            this.wrapper.DepthImageView   = depthImageView;
        }

        private void createFramebuffers(VkContext context) {
            this.wrapper.Framebuffers = new Vk.Framebuffer[this.wrapper.ImageCapacity];

            for(uint i = 0; i < this.wrapper.ImageCapacity; i++) {
                Vk.ImageView[] attachments = new Vk.ImageView[] {
                    this.wrapper.ImageViews[i],
                    this.wrapper.DepthImageView
                };

                var framebufferInfo = new Vk.FramebufferCreateInfo();
                framebufferInfo.RenderPass      = this.wrapper.RenderPass;
                framebufferInfo.AttachmentCount = 2;
                framebufferInfo.Attachments     = attachments;
                framebufferInfo.Width           = this.wrapper.Extent.Width;
                framebufferInfo.Height          = this.wrapper.Extent.Height;
                framebufferInfo.Layers          = 1;

                try {
                    this.wrapper.Framebuffers[i] = context.Device.CreateFramebuffer(framebufferInfo);
                } catch (Vk.ResultException result) {
                    throw new VkException($"An error occurred while creating framebuffer {i}.", result);
                }
            }
        }

        private void createCommandBuffers(VkContext context) {
            var allocInfo = new Vk.CommandBufferAllocateInfo();
            allocInfo.CommandPool        = context.GraphicsCommandPool;
            allocInfo.Level              = Vk.CommandBufferLevel.Primary;
            allocInfo.CommandBufferCount = this.wrapper.ImageCapacity;

            try {
                this.wrapper.CommandBuffers = context.Device.AllocateCommandBuffers(allocInfo);
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating the command buffers.", result);
            }

            for(int i = 0; i < this.wrapper.ImageCapacity; i++) {
                Vk.CommandBuffer buffer = this.wrapper.CommandBuffers[i];
                var beginInfo = new Vk.CommandBufferBeginInfo();
                beginInfo.Flags = Vk.CommandBufferUsageFlags.SimultaneousUse;

                try {
                    buffer.Begin(beginInfo);
                } catch(Vk.ResultException result) {
                    throw new VkException($"An error occurred while beginning recording for command buffer {i}.", result);
                }

                var renderPassInfo         = new Vk.RenderPassBeginInfo();
                renderPassInfo.RenderPass  = this.wrapper.RenderPass;
                renderPassInfo.Framebuffer = this.wrapper.Framebuffers[i];

                var clearColour  = new Vk.ClearValue();
                var renderArea   = new Vk.Rect2D();

                clearColour.Color = new Vk.ClearColorValue(new float[] { 0.0F, 0.0F, 0.0F, 1.0F });

                renderArea.Extent.Width  = this.wrapper.Extent.Width;
                renderArea.Extent.Height = this.wrapper.Extent.Height;
                renderArea.Offset.X      = 0;
                renderArea.Offset.Y      = 0;

                var clearDepth = new Vk.ClearValue();

                var depth = new Vk.ClearDepthStencilValue();
                depth.Depth   = 1;

                clearDepth.DepthStencil = depth;

                renderPassInfo.RenderArea      = renderArea;
                renderPassInfo.ClearValueCount = 2;
                renderPassInfo.ClearValues     = new Vk.ClearValue[] {
                    clearColour,
                    clearDepth
                };

                buffer.CmdBeginRenderPass(renderPassInfo, Vk.SubpassContents.Inline);
                buffer.CmdBindPipeline(Vk.PipelineBindPoint.Graphics, this.wrapper.Pipeline);
                buffer.CmdBindDescriptorSet(Vk.PipelineBindPoint.Graphics,
                        this.wrapper.PipelineLayout, 0, this.wrapper.DescriptorSets[i], null);

                this.renderPassCallback.Invoke(buffer);

                buffer.CmdEndRenderPass();

                try {
                    buffer.End();
                } catch(Vk.ResultException result) {
                    throw new VkException($"An error occurred while recording for command buffer {i}.", result);
                }
            }
        }
    }
}