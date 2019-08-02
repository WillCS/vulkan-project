using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Project.Math;
using Project.Native;
using Vk = Vulkan;

namespace Project.Vulkan {
    public class VkContext {

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

        public Vk.Instance Instance;
        public Vk.PhysicalDevice PhysicalDevice;
        private List<VkHelper.PhysicalDeviceSuitabilityCheck> physicalDeviceChecks;

        public Vk.Device Device;

        private QueueFamilyIndices vkQueueFamilies;
        public Vk.Queue TransferQueue;
        public Vk.Queue GraphicsQueue;
        public Vk.Queue PresentQueue;

        public GLFW.Window Window;
        public Vk.SurfaceKhr Surface;

        private List<DebugCallbackData> debugCallbacks;

        public Vk.DescriptorSetLayout DescriptorSetLayout;
        public VkPipelineWrapper Pipeline;

        public Vk.ShaderModule VertexShader;
        public Vk.ShaderModule FragmentShader;

        private List<Vertex> vertices;
        private List<short>  indices;
        private Vk.Buffer vkVertexBuffer;
        private Vk.Buffer vkIndexBuffer;
        private Vk.DeviceMemory vkVertexBufferMemory;
        private Vk.DeviceMemory vkIndexBufferMemory;

        public Vk.CommandPool GraphicsCommandPool;
        public Vk.CommandPool TransferCommandPool;

        private Vk.Semaphore[] vkImageAvailableSemaphores;
        private Vk.Semaphore[] vkRenderFinishedSemaphores;
        private Vk.Fence[] vkInFlightFences;

        private double startTime;

        public VkContext() {
            this.physicalDeviceChecks = new List<VkHelper.PhysicalDeviceSuitabilityCheck>();
            this.vertices             = new List<Vertex>();
            this.indices              = new List<short>();
            this.startTime = GLFW.Glfw.Time;
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
            this.Window = window;
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

        public void AddIndex(short index) {
            this.indices.Add(index);
        }

        public void AddIndices(IEnumerable<short> indices) {
            this.indices.AddRange(indices);
        }

        public void WaitForIdle() {
            this.Device.WaitIdle();
        }

        public bool EnsureQueueFamilySupport(Vk.PhysicalDevice device, Vk.QueueFlags family) =>
            VkHelper.CheckPhysicalDeviceQueueFamilySupport(device,
                    family, this.Surface, out this.vkQueueFamilies);

        public SwapchainSupportDetails QuerySwapchainSupport(Vk.PhysicalDevice device) =>
            VkHelper.QuerySwapchainSupport(device, this.Surface);

        public void InitVulkan() {
            this.createVulkanInstance();

            if(this.validationLayersEnabled) {
                this.debugCallbacks.ForEach((DebugCallbackData callbackData) => {
                    callbackData.wrapper = VkHelper.RegisterDebugReportCallback(this.Instance, 
                        callbackData.flags, callbackData.callback);
                });
            }

            this.createWindowSurface();

            this.PhysicalDevice = VkHelper.SelectPhysicalDevice(this.Instance, this.physicalDeviceChecks);

            this.createLogicalDevice();

            this.createShaderModules();

            this.GraphicsQueue = this.Device.GetQueue(this.vkQueueFamilies.GraphicsFamily.Value, 0);
            this.TransferQueue = this.GraphicsQueue;
            this.PresentQueue  = this.Device.GetQueue(this.vkQueueFamilies.PresentFamily.Value, 0);
            
            this.createGraphicsCommandPool();
            this.createTransferCommandPool();
            
            this.createVertexBuffer();
            this.createIndexBuffer();
            
            this.createDescriptorSetLayout();
            this.createPipelineWrapper();

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
                this.Instance = builder.Create();
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating the Vulkan instance.", result);
            }
        }

        public Vk.CommandBuffer BeginSingleTimeCommands(Vk.CommandPool commandPool) {
            var allocInfo = new Vk.CommandBufferAllocateInfo();
            allocInfo.Level = Vk.CommandBufferLevel.Primary;
            allocInfo.CommandPool = commandPool;
            allocInfo.CommandBufferCount = 1;

            var buffer = this.Device.AllocateCommandBuffers(allocInfo)[0];

            var beginInfo = new Vk.CommandBufferBeginInfo();
            beginInfo.Flags = Vk.CommandBufferUsageFlags.OneTimeSubmit;

            buffer.Begin(beginInfo);
            return buffer;
        }

        public void EndSingleTimeCommands(Vk.Queue queue, 
                Vk.CommandPool commandPool, Vk.CommandBuffer buffer) {
            buffer.End();

            var submitInfo = new Vk.SubmitInfo();
            submitInfo.CommandBufferCount = 1;
            submitInfo.CommandBuffers = new Vk.CommandBuffer[] { buffer };
            
            queue.Submit(new Vk.SubmitInfo[] { submitInfo });
            queue.WaitIdle();
            this.Device.FreeCommandBuffer(commandPool, buffer);
        }

        public void TransitionImageLayout(Vk.Image image, Vk.Format format,
                Vk.ImageLayout oldLayout, Vk.ImageLayout newLayout) {
            var buffer = this.BeginSingleTimeCommands(this.GraphicsCommandPool);

            var subresourceRange = new Vk.ImageSubresourceRange();

            if(newLayout == Vk.ImageLayout.DepthStencilAttachmentOptimal) {
                subresourceRange.AspectMask     = Vk.ImageAspectFlags.Depth;

                if(VkHelper.HasStencilComponent(format)) {
                    subresourceRange.AspectMask |= Vk.ImageAspectFlags.Stencil;
                }
            } else {
                subresourceRange.AspectMask = Vk.ImageAspectFlags.Color;
            }
            
            subresourceRange.BaseMipLevel   = 0;
            subresourceRange.LevelCount     = 1;
            subresourceRange.BaseArrayLayer = 0;
            subresourceRange.LayerCount     = 1;

            Vk.PipelineStageFlags srcStage;
            Vk.PipelineStageFlags dstStage;

            var barrier = new Vk.ImageMemoryBarrier();
            barrier.OldLayout = oldLayout;
            barrier.NewLayout = newLayout;
            barrier.SrcQueueFamilyIndex = VkConstants.VK_QUEUE_FAMILY_IGNORED;
            barrier.DstQueueFamilyIndex = VkConstants.VK_QUEUE_FAMILY_IGNORED;

            if(oldLayout == Vk.ImageLayout.Undefined && newLayout == Vk.ImageLayout.TransferDstOptimal) {
                barrier.SrcAccessMask = 0;
                barrier.DstAccessMask = Vk.AccessFlags.TransferWrite;

                srcStage = Vk.PipelineStageFlags.TopOfPipe;
                dstStage = Vk.PipelineStageFlags.Transfer; 
            } else if(oldLayout == Vk.ImageLayout.TransferDstOptimal && newLayout == Vk.ImageLayout.ShaderReadOnlyOptimal) {
                barrier.SrcAccessMask = Vk.AccessFlags.TransferWrite;
                barrier.DstAccessMask = Vk.AccessFlags.ShaderRead;

                srcStage = Vk.PipelineStageFlags.Transfer;
                dstStage = Vk.PipelineStageFlags.FragmentShader;
            } else if(oldLayout == Vk.ImageLayout.Undefined && newLayout == Vk.ImageLayout.DepthStencilAttachmentOptimal) {
                barrier.SrcAccessMask = 0;
                barrier.DstAccessMask = Vk.AccessFlags.DepthStencilAttachmentRead
                        | Vk.AccessFlags.DepthStencilAttachmentWrite;

                srcStage = Vk.PipelineStageFlags.TopOfPipe;
                dstStage = Vk.PipelineStageFlags.EarlyFragmentTests;
            } else {
                throw new Exception("Unsupported layout transition.");
            }

            barrier.Image            = image;
            barrier.SubresourceRange = subresourceRange;

            buffer.CmdPipelineBarrier(srcStage, dstStage, 0, null, null, 
                    new Vk.ImageMemoryBarrier[] { barrier });

            this.EndSingleTimeCommands(this.GraphicsQueue, this.GraphicsCommandPool, buffer);
        }

        public Vk.SharingMode GetSharingMode() {
            if(this.vkQueueFamilies.GraphicsFamily != this.vkQueueFamilies.PresentFamily) {
                return Vk.SharingMode.Concurrent;
            } else {
                return Vk.SharingMode.Exclusive;
            }
        }

        private void createWindowSurface() {
            try {
                IntPtr allocatorPointer = IntPtr.Zero;
                IntPtr surfacePointer   = new IntPtr();
                IntPtr instancePointer  = VkHelper.InstancePointer(this.Instance);
                IntPtr windowPointer    = this.Window;

                GLFW.Vulkan.CreateWindowSurface(instancePointer, windowPointer, allocatorPointer, out surfacePointer);

                this.Surface = VkHelper.CreateSurfaceFromHandle(surfacePointer);
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred creating the window surface.", result);
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
                this.Device = builder.Create(this.PhysicalDevice);
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating the logical device.", result);
            }
        }

        private void createShaderModules() {
            byte[] fragBytecode = VkHelper.LoadShaderCode("project/bin/frag.spv");
            byte[] vertBytecode = VkHelper.LoadShaderCode("project/bin/vert.spv");

            this.FragmentShader = VkHelper.CreateShaderModule(this.Device, fragBytecode);
            this.VertexShader   = VkHelper.CreateShaderModule(this.Device, vertBytecode);
        }

        private void createGraphicsCommandPool() {
            var poolInfo = new Vk.CommandPoolCreateInfo();
            poolInfo.QueueFamilyIndex = this.vkQueueFamilies.GraphicsFamily.Value;

            try {
                this.GraphicsCommandPool = this.Device.CreateCommandPool(poolInfo);
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating the graphics command pool.", result);
            }
        }

        private void createTransferCommandPool() {
            var poolInfo = new Vk.CommandPoolCreateInfo();
            poolInfo.QueueFamilyIndex = this.vkQueueFamilies.GraphicsFamily.Value;
            poolInfo.Flags = Vk.CommandPoolCreateFlags.Transient;

            try {
                this.TransferCommandPool = this.Device.CreateCommandPool(poolInfo);
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating the transfer command pool.", result);
            }
        }

        private void createPipelineWrapper() {
            if(!this.swapchainCleanedUp) {
                this.Pipeline.Cleanup(this);
            }

            this.Device.WaitIdle();

            var builder = new GraphicsPipelineBuilder();

            var support = VkHelper.QuerySwapchainSupport(this.PhysicalDevice, this.Surface);

            builder.SetCapabilities(support.capabilities);
            builder.SetSurfaceFormat(VkHelper.SelectSwapSurfaceFormat(support.formats));
            builder.SetPresentMode(VkHelper.SelectSwapPresentMode(support.presentModes));
            builder.SetExtent(VkHelper.SelectSwapExtent(support.capabilities, this.Window));
            builder.SetGraphicsFamilyQueueIndex(this.vkQueueFamilies.GraphicsFamily.Value);
            builder.SetPresentFamilyQueueIndex(this.vkQueueFamilies.PresentFamily.Value);
            builder.SetDescriptorSetLayout(this.DescriptorSetLayout);
            builder.SetVertexShader(this.VertexShader);
            builder.SetFragmentShader(this.FragmentShader);
            builder.SetRenderPassCallback((Vk.CommandBuffer buffer) => {
                buffer.CmdBindVertexBuffer(0, this.vkVertexBuffer, 0);
                buffer.CmdBindIndexBuffer(this.vkIndexBuffer, 0, Vk.IndexType.Uint16);
                buffer.CmdDrawIndexed((uint) this.indices.Count, 1, 0, 0, 0);
            });

            this.Pipeline = builder.Create(this);

            this.swapchainCleanedUp = false;
        }

        private void createDescriptorSetLayout() {
            var uboLayoutBinding = new Vk.DescriptorSetLayoutBinding();
            uboLayoutBinding.Binding         = 0;
            uboLayoutBinding.DescriptorType  = Vk.DescriptorType.UniformBuffer;
            uboLayoutBinding.DescriptorCount = 1;
            uboLayoutBinding.StageFlags      = Vk.ShaderStageFlags.Vertex;

            var layoutInfo = new Vk.DescriptorSetLayoutCreateInfo();
            layoutInfo.BindingCount = 1;
            layoutInfo.Bindings = new Vk.DescriptorSetLayoutBinding[] { uboLayoutBinding };

            try {
                this.DescriptorSetLayout = this.Device.CreateDescriptorSetLayout(layoutInfo);
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating the descriptor set layout.", result);
            }
        }

        private void createVertexBuffer() {
            var size  = (ulong) (this.vertices.Count * Marshal.SizeOf<Vertex>());
            var transferUsage = Vk.BufferUsageFlags.TransferSrc;
            var vertexUsage = Vk.BufferUsageFlags.VertexBuffer
                    | Vk.BufferUsageFlags.TransferDst;
            var transferMemoryProps = Vk.MemoryPropertyFlags.DeviceLocal;
            var vertexMemoryProps = Vk.MemoryPropertyFlags.HostVisible
                    | Vk.MemoryPropertyFlags.HostCoherent;
            var sharingMode = this.GetSharingMode();

            BufferWithMemory stagingBuffer;

            try {
                stagingBuffer = VkHelper.CreateBuffer(this, size, transferUsage,
                        transferMemoryProps, sharingMode);
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating the staging buffer.", result);
            }

            IntPtr memory = this.Device.MapMemory(stagingBuffer.Memory, 0, size);
            var vertexArray = this.vertices.ToArray();
            MemoryManagement.ArrayToPtr<Vertex>(vertexArray, memory, false);
            this.Device.UnmapMemory(stagingBuffer.Memory);
            
            try {
                BufferWithMemory vertexBuffer = VkHelper.CreateBuffer(this, size, vertexUsage, 
                        vertexMemoryProps, sharingMode);
                this.vkVertexBuffer = vertexBuffer.Buffer;
                this.vkVertexBufferMemory = vertexBuffer.Memory;
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating the vertex buffer.", result);
            }

            VkHelper.CopyBuffer(stagingBuffer.Buffer, this.vkVertexBuffer, size, this);

            stagingBuffer.Destroy(this.Device);
        }

        private void createIndexBuffer() {
            var size = Marshal.SizeOf(typeof(short)) * this.indices.Count;
            var transferUsage = Vk.BufferUsageFlags.TransferSrc;
            var indexUsage = Vk.BufferUsageFlags.IndexBuffer
                    | Vk.BufferUsageFlags.TransferDst;
            var transferMemoryProps = Vk.MemoryPropertyFlags.DeviceLocal;
            var indexMemoryProps = Vk.MemoryPropertyFlags.HostVisible
                    | Vk.MemoryPropertyFlags.HostCoherent;
            var sharingMode = this.GetSharingMode();

            BufferWithMemory stagingBuffer;

            try {
                stagingBuffer = VkHelper.CreateBuffer(this, size, transferUsage,
                        transferMemoryProps, sharingMode);
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating the staging buffer.", result);
            }

            IntPtr memory = this.Device.MapMemory(stagingBuffer.Memory, 0, size);
            var indexArray = this.indices.ToArray();
            MemoryManagement.ArrayToPtr<short>(indexArray, memory, false);
            this.Device.UnmapMemory(stagingBuffer.Memory);
            
            try {
                BufferWithMemory indexBuffer = VkHelper.CreateBuffer(this, size, indexUsage,
                        indexMemoryProps, sharingMode);
                this.vkIndexBuffer = indexBuffer.Buffer;
                this.vkIndexBufferMemory = indexBuffer.Memory;
            } catch(Vk.ResultException result) {
                throw new VkException("An error occurred while creating the index buffer.", result);
            }

            VkHelper.CopyBuffer(stagingBuffer.Buffer, this.vkIndexBuffer, size, this);

            stagingBuffer.Destroy(this.Device);
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
                    this.vkImageAvailableSemaphores[i] = this.Device.CreateSemaphore(semaphoreInfo);
                    this.vkRenderFinishedSemaphores[i] = this.Device.CreateSemaphore(semaphoreInfo);
                    this.vkInFlightFences[i]           = this.Device.CreateFence(fenceInfo);
                } catch(Vk.ResultException result) {
                    throw new VkException("An error has occurred while creating sync objects.", result);
                }
            }
        }

        private void updateUniformBuffer(uint index) {
            double timeNow = GLFW.Glfw.Time;
            double dt = timeNow - this.startTime;

            var ubo   = new UniformBufferObject();

            var model = Matrix4.IDENTITY; //Matrices.YRotationMatrix4(dt);
            ubo.Model = model;
            
            var view = Matrices.LookAtMatrix(new Vector3(100, 100, 100), Vector3.ZERO);
            ubo.View = view;

            var projection = Matrices.PerspectiveProjectionMatrix(0.01, 500, 1.25,
                    System.Math.PI / 2);
            // var projection = Matrices.OrthographicProjectionMatrix(0.01, 100, 3.2, 2.4);
            ubo.Projection = projection;

            var memory  = this.Pipeline.UniformBuffersMemory[index];
            var address = this.Device.MapMemory(memory, 0, Marshal.SizeOf<UniformBufferObject>());

            Marshal.StructureToPtr<UniformBufferObject>(ubo, address, false);

            this.Device.UnmapMemory(memory);
        }
        
        public void DrawFrame() {
            this.Device.WaitForFence(this.vkInFlightFences[this.currentFrame], true, UInt64.MaxValue);

            uint imageIndex = 0;
            
            try {
                imageIndex = this.Device.AcquireNextImageKHR(this.Pipeline.Swapchain, 
                        UInt64.MaxValue, this.vkImageAvailableSemaphores[this.currentFrame]);
            } catch(Vk.ResultException result) {
                if(result.Result == Vk.Result.ErrorOutOfDateKhr) {
                    this.createPipelineWrapper();
                    return;
                } else {
                    throw new VkException("An error occurred while acquiring a swapchain image.", result);
                }
            }

            this.Device.ResetFence(this.vkInFlightFences[this.currentFrame]);

            var waitSemaphores   = new Vk.Semaphore[] { this.vkImageAvailableSemaphores[this.currentFrame] };
            var signalSemaphores = new Vk.Semaphore[] { this.vkRenderFinishedSemaphores[this.currentFrame] };

            this.updateUniformBuffer(imageIndex);

            var submitInfo = new Vk.SubmitInfo();
            submitInfo.CommandBufferCount = 1;
            submitInfo.WaitSemaphoreCount = 1;
            submitInfo.SignalSemaphoreCount = 1;

            submitInfo.CommandBuffers = new Vk.CommandBuffer[] {
                this.Pipeline.CommandBuffers[imageIndex]
            };

            submitInfo.WaitSemaphores = waitSemaphores;
            submitInfo.WaitDstStageMask = new Vk.PipelineStageFlags[] {
                Vk.PipelineStageFlags.ColorAttachmentOutput
            };
            submitInfo.SignalSemaphores = signalSemaphores;

            try {
                this.GraphicsQueue.Submit(new Vk.SubmitInfo[] { submitInfo }, 
                        this.vkInFlightFences[this.currentFrame]);
            } catch(Vk.ResultException result) {
                throw new VkException("An error has occurred while submitting a command buffer.", result);
            }

            var presentInfo = new Vk.PresentInfoKhr();
            presentInfo.WaitSemaphoreCount = 1;
            presentInfo.WaitSemaphores     = signalSemaphores;
            presentInfo.SwapchainCount     = 1;
            presentInfo.Swapchains         = new Vk.SwapchainKhr[] { 
                this.Pipeline.Swapchain
            };

            presentInfo.ImageIndices       = new uint[] {
                imageIndex
            };

            try {
                this.PresentQueue.PresentKHR(presentInfo);
            } catch(Vk.ResultException result) {
                if(result.Result == Vk.Result.ErrorOutOfDateKhr || 
                        result.Result == Vk.Result.SuboptimalKhr ||
                        Program.RESIZED) {
                    this.createPipelineWrapper();
                    Program.RESIZED = false;
                } else {
                    throw new VkException("An error occurred while presenting an image.", result);
                }
            }
        }

        public void Cleanup() {
            this.Pipeline.Cleanup(this);

            // Destroy Descriptor Set Layout
            this.Device.DestroyDescriptorSetLayout(this.DescriptorSetLayout);

            // Destroy Sync Objects
            for(int i = 0; i < this.maxFramesInFlight; i++) {
                this.Device.DestroySemaphore(this.vkImageAvailableSemaphores[i]);
                this.Device.DestroySemaphore(this.vkRenderFinishedSemaphores[i]);
                this.Device.DestroyFence(this.vkInFlightFences[i]);
            }

            // Destroy Vertex Buffer and free its allocated memory
            this.Device.DestroyBuffer(this.vkVertexBuffer);
            this.Device.FreeMemory(this.vkVertexBufferMemory);

            // Destroy the Index Buffer and free its allocated memory
            this.Device.DestroyBuffer(this.vkIndexBuffer);
            this.Device.FreeMemory(this.vkIndexBufferMemory);

            // Destroy Command Pools
            this.Device.DestroyCommandPool(this.GraphicsCommandPool);
            this.Device.DestroyCommandPool(this.TransferCommandPool);

            // Destroy Fragment Shaders
            this.Device.DestroyShaderModule(this.VertexShader);
            this.Device.DestroyShaderModule(this.FragmentShader);

            // Destroy Logical Device
            this.Device.Destroy();
            
            // Destroy Debug Callback
            if(this.validationLayersEnabled) {
                this.debugCallbacks.ForEach((DebugCallbackData callback) => {
                    this.Instance.DestroyDebugReportCallbackEXT(callback.wrapper);
                });
            }

            // Destroy Drawing Surface
            this.Instance.DestroySurfaceKHR(this.Surface);

            // Destroy Instance
            this.Instance.Dispose();
        }
    }
}