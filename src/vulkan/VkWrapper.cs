using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Vk = Vulkan;

namespace Game.Vulkan {
    public class VkWrapper {

        public int maxFramesInFlight = 2;
        public int currentFrame = 0;
        private bool swapchainCleanedUp = true;
        private bool validationLayersEnabled = false;

        public string[] InstanceExtensions = {

        };

        public string [] DeviceExtensions = {
            VkConstants.VK_KHR_swapchain
        };

        public string[] ValidationLayers = {
            VkConstants.VK_LAYER_KHRONOS_vaidation
        };

        public string[] ValidationExtensions = {
            VkConstants.VK_EXT_debug_report
        };

        private Vk.Instance vkInstance;
        private Vk.PhysicalDevice vkPhysicalDevice;
        private List<VkHelper.PhysicalDeviceSuitabilityCheck> physicalDeviceChecks;

        private Vk.Device vkDevice;

        private QueueFamilyIndices vkQueueFamilies;
        private Vk.Queue vkGraphicsQueue;
        private Vk.Queue vkPresentQueue;

        private GLFW.Window window;
        private Vk.SurfaceKhr vkSurface;

        private List<DebugCallbackData> debugCallbacks;

        private SwapchainPipeline swapchainPipeline;

        private List<Vertex> vertices;
        private Vk.Buffer vkVertexBuffer;
        private Vk.DeviceMemory vkVertexBufferMemory;

        private Vk.CommandPool vkCommandPool;

        private Vk.Semaphore[] vkImageAvailableSemaphores;
        private Vk.Semaphore[] vkRenderFinishedSemaphores;
        private Vk.Fence[] vkInFlightFences;

        public VkWrapper() {
            this.physicalDeviceChecks = new List<VkHelper.PhysicalDeviceSuitabilityCheck>();
            this.vertices             = new List<Vertex>();
        }

        public void EnableValidationLayers() {
            bool supported = VkHelper.CheckValidationLayerSupport(this.ValidationLayers);
            this.validationLayersEnabled = supported;

            if(!supported) {
                Console.Error.WriteLine("Validation Layers not supported.");
                return;
            }

            this.debugCallbacks = new List<DebugCallbackData>();
        }

        public void RegisterDebugReportCallback(VkHelper.DebugCallback callback, 
                Vk.DebugReportFlagsExt flags) {
            if(!this.validationLayersEnabled) {
                Console.Error.WriteLine("Attempted to register debug callback without Validation Layers enabled.");
                return;
            }

            var callbackData = new DebugCallbackData();
            callbackData.callback = callback;
            callbackData.flags = flags;
            this.debugCallbacks.Add(callbackData);
        }

        public void SetWindow(GLFW.Window window) {
            this.window = window;
        }
        
        public void RegisterPhysicalDeviceSuitabilityCheck(
                VkHelper.PhysicalDeviceSuitabilityCheck check) {
            this.physicalDeviceChecks.Add(check);
        }

        public void AddVertex(Vertex vertex) {
            this.vertices.Add(vertex);
        }

        public void AddVertices(IEnumerable<Vertex> vertices) {
            this.vertices.AddRange(vertices);
        }

        public void WaitForIdle() {
            this.vkDevice.WaitIdle();
        }

        public bool EnsureQueueFamilySupport(Vk.PhysicalDevice device, Vk.QueueFlags family) =>
            VkHelper.CheckPhysicalDeviceQueueFamilySupport(device,
                    family, this.vkSurface, out this.vkQueueFamilies);

        public SwapchainSupportDetails QuerySwapchainSupport(Vk.PhysicalDevice device) =>
            VkHelper.QuerySwapchainSupport(device, this.vkSurface);

        public void InitVulkan() {
            this.createVulkanInstance();

            if(this.validationLayersEnabled) {
                this.debugCallbacks.ForEach((DebugCallbackData callbackData) => {
                    callbackData.wrapper = VkHelper.RegisterDebugReportCallback(this.vkInstance, 
                        callbackData.flags, callbackData.callback);
                });
            }

            this.createWindowSurface();

            this.vkPhysicalDevice = VkHelper.SelectPhysicalDevice(this.vkInstance, this.physicalDeviceChecks);

            this.createLogicalDevice();

            this.vkGraphicsQueue = this.vkDevice.GetQueue(this.vkQueueFamilies.GraphicsFamily.Value, 0);
            this.vkPresentQueue  = this.vkDevice.GetQueue(this.vkQueueFamilies.PresentFamily.Value, 0);
            
            this.createCommandPool();
            this.createVertexBuffer();

            this.createSwapchainPipeline();

            this.createSyncObjects();
        }

        private void createVulkanInstance() {
            InstanceBuilder builder = new InstanceBuilder();

            builder.SetApplicationName("Thingy");
            builder.SetApplicationVersion(Vk.Version.Make(1, 0, 0));
            builder.SetEngineName("None");
            builder.SetEngineVersion(Vk.Version.Make(1, 0, 0));
            builder.SetApiVersion(Vk.Version.Make(1, 0, 0));

            builder.EnableExtensions(VkHelper.GetGLFWRequiredInstanceExtensions());
            builder.EnableExtensions(this.InstanceExtensions);

            if(this.validationLayersEnabled) {
                builder.EnableExtensions(this.ValidationExtensions);
                builder.EnableValidationLayers(this.ValidationLayers);
            }

            try {
                this.vkInstance = builder.Create();
            } catch(Vk.ResultException result) {
                this.error(result, "An error occurred while creating the Vulkan instance.");
            }
        }

        private void createWindowSurface() {
            try {
                IntPtr allocatorPointer = IntPtr.Zero;
                IntPtr surfacePointer   = new IntPtr();
                IntPtr instancePointer  = VkHelper.InstancePointer(this.vkInstance);
                IntPtr windowPointer    = this.window;

                GLFW.Vulkan.CreateWindowSurface(instancePointer, windowPointer, allocatorPointer, out surfacePointer);

                this.vkSurface = VkHelper.CreateSurfaceFromHandle(surfacePointer);
            } catch(Vk.ResultException result) {
                this.error(result, "An error occurred creating the window surface.");
            }
        }

        private void createLogicalDevice() {
            LogicalDeviceBuilder builder = new LogicalDeviceBuilder();

            HashSet<uint> queueTypes = new HashSet<uint>(new uint[] {
                this.vkQueueFamilies.GraphicsFamily.Value,
                this.vkQueueFamilies.PresentFamily.Value
            });

            foreach(uint queueType in queueTypes) {
                Vk.DeviceQueueCreateInfo queueInfo = new Vk.DeviceQueueCreateInfo();
                queueInfo.QueueFamilyIndex = queueType;
                queueInfo.QueueCount = 1;
                queueInfo.QueuePriorities = new float[] { 1.0F };

                builder.EnableQueue(queueInfo);
            }

            builder.EnableExtensions(this.DeviceExtensions);
            
            if(this.validationLayersEnabled) {
                builder.EnableValidationLayers(this.ValidationLayers);
            }

            try {
                this.vkDevice = builder.Create(this.vkPhysicalDevice);
            } catch(Vk.ResultException result) {
                this.error(result, "An error occurred while creating the logical device.");
            }
        }

        private void createCommandPool() {
            var poolInfo = new Vk.CommandPoolCreateInfo();
            poolInfo.QueueFamilyIndex = this.vkQueueFamilies.GraphicsFamily.Value;

            try {
                this.vkCommandPool = this.vkDevice.CreateCommandPool(poolInfo);
            } catch(Vk.ResultException result) {
                this.error(result, "An error occurred while creating the command pool.");
            }
        }

        private void createSwapchainPipeline() {
            if(!this.swapchainCleanedUp) {
                this.swapchainPipeline.Cleanup();
            }

            this.vkDevice.WaitIdle();

            var support = VkHelper.QuerySwapchainSupport(this.vkPhysicalDevice, this.vkSurface);
            var swapchainParameters = new SwapchainParameters();

            var format      = VkHelper.SelectSwapSurfaceFormat(support.formats);
            var presentMode = VkHelper.SelectSwapPresentMode(support.presentModes);
            var extent      = VkHelper.SelectSwapExtent(support.capabilities, this.window);
            
            swapchainParameters.SurfaceFormat    = format;
            swapchainParameters.PresentMode      = presentMode;
            swapchainParameters.Extent           = extent;
            swapchainParameters.MinImageCount    = support.capabilities.MinImageCount;
            swapchainParameters.MaxImageCount    = support.capabilities.MaxImageCount;
            swapchainParameters.CurrentTransform = support.capabilities.CurrentTransform;

            var swapchainPipeline = new SwapchainPipeline();
            swapchainPipeline.Setup(this.vkDevice, this.vkCommandPool);
            swapchainPipeline.Format = format.Format;
            swapchainPipeline.Extent = extent;

            var swapchain = this.createSwapchain(swapchainParameters);
            swapchainPipeline.Swapchain      = swapchain;
            swapchainPipeline.Images         = this.vkDevice.GetSwapchainImagesKHR(swapchain);
            swapchainPipeline.ImageViews     = this.createImageViews(swapchainPipeline);
            swapchainPipeline.RenderPass     = this.createRenderPass(swapchainPipeline);
            swapchainPipeline.Pipeline       = this.createGraphicsPipeline(swapchainPipeline);
            swapchainPipeline.Framebuffers   = this.createFramebuffers(swapchainPipeline);
            swapchainPipeline.CommandBuffers = this.createCommandBuffers(swapchainPipeline);

            this.swapchainPipeline = swapchainPipeline;
            this.swapchainCleanedUp = false;
        }

        private Vk.SwapchainKhr createSwapchain(SwapchainParameters parameters) {
            uint imageCount = parameters.MinImageCount + 1;

            if(parameters.MaxImageCount > 0 && imageCount > parameters.MaxImageCount) {
                imageCount = parameters.MaxImageCount;
            }

            Vk.SwapchainCreateInfoKhr createInfo = new Vk.SwapchainCreateInfoKhr();
            createInfo.Surface          = this.vkSurface;
            createInfo.MinImageCount    = imageCount;
            createInfo.ImageFormat      = parameters.SurfaceFormat.Format;
            createInfo.ImageColorSpace  = parameters.SurfaceFormat.ColorSpace;
            createInfo.ImageExtent      = parameters.Extent;
            createInfo.ImageArrayLayers = 1;
            createInfo.ImageUsage       = Vk.ImageUsageFlags.ColorAttachment;

            if(this.vkQueueFamilies.GraphicsFamily != this.vkQueueFamilies.PresentFamily) {
                createInfo.ImageSharingMode = Vk.SharingMode.Concurrent;
                createInfo.QueueFamilyIndexCount = 2;
                createInfo.QueueFamilyIndices = new uint[] {
                    this.vkQueueFamilies.GraphicsFamily.Value,
                    this.vkQueueFamilies.PresentFamily.Value
                };
            } else {
                createInfo.ImageSharingMode = Vk.SharingMode.Exclusive;
                createInfo.QueueFamilyIndexCount = 0;
                createInfo.QueueFamilyIndices = null;
            }

            createInfo.PreTransform = parameters.CurrentTransform;
            createInfo.CompositeAlpha = Vk.CompositeAlphaFlagsKhr.Opaque; // Blending with other windows? :o
            createInfo.PresentMode = parameters.PresentMode;
            createInfo.Clipped = true;
            createInfo.OldSwapchain = null;

            try {
                return this.vkDevice.CreateSwapchainKHR(createInfo);
            } catch(Vk.ResultException result) {
                this.error(result, "An error occurred while creating the swapchain.");
                return null;
            }
        }

        private Vk.ImageView[] createImageViews(SwapchainPipeline swapchainPipeline) {
            var imageViews = new Vk.ImageView[swapchainPipeline.ImageCapacity];

            for(int i = 0; i < swapchainPipeline.ImageCapacity; i++) {
                Vk.ImageViewCreateInfo createInfo = new Vk.ImageViewCreateInfo();
                createInfo.Image = swapchainPipeline.Images[i];
                createInfo.ViewType = Vk.ImageViewType.View2D;
                createInfo.Format = swapchainPipeline.Format;

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
                    imageViews[i] = this.vkDevice.CreateImageView(createInfo);
                } catch(Vk.ResultException result) {
                    this.error(result, $"An error occurred while creating image view {i}.");
                    return null;
                }
            }
            return imageViews;
        }

        private Vk.RenderPass createRenderPass(SwapchainPipeline swapchainPipeline) {
            var colourAttachment = new Vk.AttachmentDescription();
            colourAttachment.Format         = swapchainPipeline.Format;
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

            var subpass = new Vk.SubpassDescription();
            subpass.PipelineBindPoint    = Vk.PipelineBindPoint.Graphics;
            subpass.ColorAttachmentCount = 1;
            subpass.ColorAttachments     = new Vk.AttachmentReference[] {
                colourAttachmentRef
            };

            var subpassDep = new Vk.SubpassDependency();
            subpassDep.SrcSubpass    = VkConstants.VK_SUBPASS_EXTERNAL;
            subpassDep.DstSubpass    = 0;
            subpassDep.SrcStageMask  = Vk.PipelineStageFlags.ColorAttachmentOutput;
            subpassDep.DstStageMask  = Vk.PipelineStageFlags.ColorAttachmentOutput;
            subpassDep.SrcAccessMask = 0;
            subpassDep.DstAccessMask = Vk.AccessFlags.ColorAttachmentRead | Vk.AccessFlags.ColorAttachmentWrite;

            var renderPassInfo = new Vk.RenderPassCreateInfo();
            renderPassInfo.AttachmentCount = 1;
            renderPassInfo.Attachments     = new Vk.AttachmentDescription[] {
                colourAttachment
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
                return this.vkDevice.CreateRenderPass(renderPassInfo);
            } catch(Vk.ResultException result) {
                this.error(result, "An error occurred while creating a render pass.");
                return null;
            }
        }

        private Vk.Pipeline createGraphicsPipeline(SwapchainPipeline swapchainPipeline) {
            byte[] fragBytecode = VkHelper.LoadShaderCode("bin/frag.spv");
            byte[] vertBytecode = VkHelper.LoadShaderCode("bin/vert.spv");

            Vk.ShaderModule fragModule = VkHelper.CreateShaderModule(this.vkDevice, fragBytecode);
            Vk.ShaderModule vertModule = VkHelper.CreateShaderModule(this.vkDevice, vertBytecode);

            var vertShaderStageInfo = new Vk.PipelineShaderStageCreateInfo();
            vertShaderStageInfo.Stage  = Vk.ShaderStageFlags.Vertex;
            vertShaderStageInfo.Module = vertModule;
            vertShaderStageInfo.Name   = "main";

            var fragShaderStageInfo = new Vk.PipelineShaderStageCreateInfo();
            fragShaderStageInfo.Stage  = Vk.ShaderStageFlags.Fragment;
            fragShaderStageInfo.Module = fragModule;
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
            viewport.Width       = (float) swapchainPipeline.Extent.Width;
            viewport.Height      = (float) swapchainPipeline.Extent.Height;
            viewport.MinDepth    = 0.0F;
            viewport.MaxDepth    = 1.0F;

            Vk.Rect2D scissorRect = new Vk.Rect2D();
            scissorRect.Offset.X  = 0;
            scissorRect.Offset.Y  = 0;
            scissorRect.Extent    = swapchainPipeline.Extent;

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
            
            try {
                swapchainPipeline.PipelineLayout = this.vkDevice.CreatePipelineLayout(pipelineLayoutInfo);
            } catch(Vk.ResultException result) {
                this.error(result, "An error occurred while creating the pipeline layout.");
                return null;
            }

            var pipelineInfo = new Vk.GraphicsPipelineCreateInfo();
            pipelineInfo.StageCount         = 2;
            pipelineInfo.Stages             = shaderStageInfos;
            pipelineInfo.VertexInputState   = vertexInputInfo;
            pipelineInfo.InputAssemblyState = inputAssemblyInfo;
            pipelineInfo.ViewportState      = viewportStateInfo;
            pipelineInfo.RasterizationState = rasteriserInfo;
            pipelineInfo.MultisampleState   = multisamplingInfo;
            pipelineInfo.DepthStencilState  = null;
            pipelineInfo.ColorBlendState    = colourBlendStateInfo;
            pipelineInfo.DynamicState       = null;
            pipelineInfo.Layout             = swapchainPipeline.PipelineLayout;
            pipelineInfo.RenderPass         = swapchainPipeline.RenderPass;
            pipelineInfo.Subpass            = 0;
            pipelineInfo.BasePipelineHandle = null;
            pipelineInfo.BasePipelineIndex  = -1;

            var pipelineInfos = new Vk.GraphicsPipelineCreateInfo[] {
                pipelineInfo
            };

            Vk.Pipeline[] pipeline = null;

            try {
                pipeline = this.vkDevice.CreateGraphicsPipelines(null, pipelineInfos);
            } catch(Vk.ResultException result) {
                this.error(result, "An error occurred while creating the graphics pipeline.");
            }

            this.vkDevice.DestroyShaderModule(fragModule);
            this.vkDevice.DestroyShaderModule(vertModule);

            return pipeline[0];
        }

        private Vk.Framebuffer[] createFramebuffers(SwapchainPipeline swapchainPipeline) {
            var framebuffers = new Vk.Framebuffer[swapchainPipeline.ImageCapacity];

            for(uint i = 0; i < swapchainPipeline.ImageCapacity; i++) {
                Vk.ImageView[] attachments = new Vk.ImageView[] {
                    swapchainPipeline.ImageViews[i]
                };

                var framebufferInfo = new Vk.FramebufferCreateInfo();
                framebufferInfo.RenderPass      = swapchainPipeline.RenderPass;
                framebufferInfo.AttachmentCount = 1;
                framebufferInfo.Attachments     = attachments;
                framebufferInfo.Width           = swapchainPipeline.Extent.Width;
                framebufferInfo.Height          = swapchainPipeline.Extent.Height;
                framebufferInfo.Layers          = 1;

                try {
                    framebuffers[i] = this.vkDevice.CreateFramebuffer(framebufferInfo);
                } catch (Vk.ResultException result) {
                    this.error(result, $"An error occurred while creating framebuffer {i}.");
                    return null;
                }
            }

            return framebuffers;
        }

        private void createVertexBuffer() {
            var bufferInfo = new Vk.BufferCreateInfo();
            bufferInfo.Size = (ulong) (this.vertices.Count * Marshal.SizeOf<Vertex>());
            bufferInfo.Usage = Vk.BufferUsageFlags.VertexBuffer;
            bufferInfo.SharingMode = Vk.SharingMode.Exclusive;
            
            try {
                this.vkVertexBuffer = this.vkDevice.CreateBuffer(bufferInfo);
            } catch(Vk.ResultException result) {
                this.error(result, "An error occurred while creating the vertex buffer.");
            }

            var memReqs = this.vkDevice.GetBufferMemoryRequirements(this.vkVertexBuffer);
            var memTypeIndex = VkHelper.FindMemoryType(memReqs.MemoryTypeBits, this.vkPhysicalDevice,
                    Vk.MemoryPropertyFlags.HostVisible | Vk.MemoryPropertyFlags.HostCoherent);

            var memAllocInfo = new Vk.MemoryAllocateInfo();
            memAllocInfo.AllocationSize  = memReqs.Size;
            memAllocInfo.MemoryTypeIndex = memTypeIndex;

            try {
                this.vkVertexBufferMemory = this.vkDevice.AllocateMemory(memAllocInfo);
            } catch(Vk.ResultException result) {
                this.error(result, "An error occurred while allocating vertex buffer memory.");
            }

            this.vkDevice.BindBufferMemory(this.vkVertexBuffer, this.vkVertexBufferMemory, 0);

            IntPtr memory = this.vkDevice.MapMemory(this.vkVertexBufferMemory, 0, bufferInfo.Size);
            var vertexArray = this.vertices.ToArray();

            MemoryManagement.MarshalArray<Vertex>(vertexArray, memory, false);

            Vertex read = Marshal.PtrToStructure<Vertex>(memory);
            Console.WriteLine(read.Position.X);
            Console.WriteLine(read.Position.Y);
            Console.WriteLine(read.Colour.X);
            Console.WriteLine(read.Colour.Y);
            Console.WriteLine(read.Colour.Z);

            this.vkDevice.UnmapMemory(this.vkVertexBufferMemory);
        }

        private Vk.CommandBuffer[] createCommandBuffers(SwapchainPipeline swapchainPipeline) {
            var allocInfo = new Vk.CommandBufferAllocateInfo();
            allocInfo.CommandPool        = this.vkCommandPool;
            allocInfo.Level              = Vk.CommandBufferLevel.Primary;
            allocInfo.CommandBufferCount = swapchainPipeline.ImageCapacity;

            Vk.CommandBuffer[] buffers = null;

            try {
                buffers = this.vkDevice.AllocateCommandBuffers(allocInfo);
            } catch(Vk.ResultException result) {
                this.error(result, "An error occurred while creating the command buffers.");
                return null;
            }

            for(int i = 0; i < buffers.Length; i++) {
                Vk.CommandBuffer buffer = buffers[i];
                var beginInfo = new Vk.CommandBufferBeginInfo();
                beginInfo.Flags = Vk.CommandBufferUsageFlags.SimultaneousUse;

                try {
                    buffer.Begin(beginInfo);
                } catch(Vk.ResultException result) {
                    this.error(result, $"An error occurred while beginning recording for command buffer {i}.");
                }

                var renderPassInfo         = new Vk.RenderPassBeginInfo();
                renderPassInfo.RenderPass  = swapchainPipeline.RenderPass;
                renderPassInfo.Framebuffer = swapchainPipeline.Framebuffers[i];

                var clearColour  = new Vk.ClearValue();
                var renderArea   = new Vk.Rect2D();

                clearColour.Color        = new Vk.ClearColorValue(new float[] { 0.0F, 0.0F, 0.0F, 1.0F });

                renderArea.Extent.Width  = swapchainPipeline.Extent.Width;
                renderArea.Extent.Height = swapchainPipeline.Extent.Height;
                renderArea.Offset.X      = 0;
                renderArea.Offset.Y      = 0;

                renderPassInfo.RenderArea      = renderArea;
                renderPassInfo.ClearValueCount = 1;
                renderPassInfo.ClearValues     = new Vk.ClearValue[] {
                    clearColour
                };

                buffers[i].CmdBeginRenderPass(renderPassInfo, Vk.SubpassContents.Inline);
                buffers[i].CmdBindPipeline(Vk.PipelineBindPoint.Graphics, swapchainPipeline.Pipeline);
                buffers[i].CmdBindVertexBuffer(0, this.vkVertexBuffer, 0);
                buffers[i].CmdDraw((uint) this.vertices.Count, 1, 0, 0);
                buffers[i].CmdEndRenderPass();

                try {
                    buffers[i].End();
                } catch(Vk.ResultException result) {
                    this.error(result, $"An error occurred while recording for command buffer {i}.");
                    return null;
                }
            }

            return buffers;
        }

        private void createSyncObjects() {
            this.vkImageAvailableSemaphores = new Vk.Semaphore[this.maxFramesInFlight];
            this.vkRenderFinishedSemaphores = new Vk.Semaphore[this.maxFramesInFlight];
            this.vkInFlightFences           = new Vk.Fence[this.maxFramesInFlight];

            var semaphoreInfo = new Vk.SemaphoreCreateInfo();
            var fenceInfo     = new Vk.FenceCreateInfo();
            fenceInfo.Flags   = Vk.FenceCreateFlags.Signaled;

            for(int i = 0; i < this.maxFramesInFlight; i++) {
                try {
                    this.vkImageAvailableSemaphores[i] = this.vkDevice.CreateSemaphore(semaphoreInfo);
                    this.vkRenderFinishedSemaphores[i] = this.vkDevice.CreateSemaphore(semaphoreInfo);
                    this.vkInFlightFences[i]           = this.vkDevice.CreateFence(fenceInfo);
                } catch(Vk.ResultException result) {
                    this.error(result, "An error has occurred while creating sync objects.");
                }
            }
        }
        
        public void DrawFrame() {
            this.vkDevice.WaitForFence(this.vkInFlightFences[this.currentFrame], true, UInt64.MaxValue);

            uint imageIndex = 0;
            
            try {
                imageIndex = this.vkDevice.AcquireNextImageKHR(this.swapchainPipeline.Swapchain, 
                        UInt64.MaxValue, this.vkImageAvailableSemaphores[this.currentFrame]);
            } catch(Vk.ResultException result) {
                if(result.Result == Vk.Result.ErrorOutOfDateKhr) {
                    this.createSwapchainPipeline();
                    return;
                } else {
                    this.error(result, "An error occurred while acquiring a swapchain image.");
                }
            }

            this.vkDevice.ResetFence(this.vkInFlightFences[this.currentFrame]);

            var waitSemaphores   = new Vk.Semaphore[] { this.vkImageAvailableSemaphores[this.currentFrame] };
            var signalSemaphores = new Vk.Semaphore[] { this.vkRenderFinishedSemaphores[this.currentFrame] };

            var submitInfo = new Vk.SubmitInfo();
            submitInfo.CommandBufferCount = 1;
            submitInfo.WaitSemaphoreCount = 1;
            submitInfo.SignalSemaphoreCount = 1;

            submitInfo.CommandBuffers = new Vk.CommandBuffer[] {
                this.swapchainPipeline.CommandBuffers[imageIndex]
            };

            submitInfo.WaitSemaphores = waitSemaphores;
            submitInfo.WaitDstStageMask = new Vk.PipelineStageFlags[] {
                Vk.PipelineStageFlags.ColorAttachmentOutput
            };
            submitInfo.SignalSemaphores = signalSemaphores;

            try {
                this.vkGraphicsQueue.Submit(new Vk.SubmitInfo[] { submitInfo }, 
                        this.vkInFlightFences[this.currentFrame]);
            } catch(Vk.ResultException result) {
                this.error(result, "An error has occurred while submitting a command buffer.");
            }

            var presentInfo = new Vk.PresentInfoKhr();
            presentInfo.WaitSemaphoreCount = 1;
            presentInfo.WaitSemaphores     = signalSemaphores;
            presentInfo.SwapchainCount     = 1;
            presentInfo.Swapchains         = new Vk.SwapchainKhr[] { 
                this.swapchainPipeline.Swapchain
            };

            presentInfo.ImageIndices       = new uint[] {
                imageIndex
            };

            try {
                this.vkPresentQueue.PresentKHR(presentInfo);
            } catch(Vk.ResultException result) {
                if(result.Result == Vk.Result.ErrorOutOfDateKhr || 
                        result.Result == Vk.Result.SuboptimalKhr ||
                        Program.RESIZED) {
                    this.createSwapchainPipeline();
                    Program.RESIZED = false;
                } else {
                    this.error(result, "An error occurred while presenting an image.");
                }
            }
        }

        public void Cleanup() {
            this.swapchainPipeline.Cleanup();

            // Destroy Sync Objects
            for(int i = 0; i < this.maxFramesInFlight; i++) {
                this.vkDevice.DestroySemaphore(this.vkImageAvailableSemaphores[i]);
                this.vkDevice.DestroySemaphore(this.vkRenderFinishedSemaphores[i]);
                this.vkDevice.DestroyFence(this.vkInFlightFences[i]);
            }

            // Destroy Vertex Buffer and free its allocated memory
            this.vkDevice.DestroyBuffer(this.vkVertexBuffer);
            this.vkDevice.FreeMemory(this.vkVertexBufferMemory);

            // Destroy Command Pool
            this.vkDevice.DestroyCommandPool(this.vkCommandPool);

            // Destroy Logical Device
            this.vkDevice.Destroy();
            
            // Destroy Debug Callback
            if(this.validationLayersEnabled) {
                this.debugCallbacks.ForEach((DebugCallbackData callback) => {
                    this.vkInstance.DestroyDebugReportCallbackEXT(callback.wrapper);
                });
            }

            // Destroy Drawing Surface
            this.vkInstance.DestroySurfaceKHR(this.vkSurface);

            // Destroy Instance
            this.vkInstance.Dispose();
        }

        private void error(Vk.ResultException result, string message) {
            Console.Error.WriteLine(result.Result);
            throw new Exception($"[{result.Result}] {message}");
        }
    }
}