using System;
using System.Runtime.InteropServices;
using GLFW;
using Game.Vulkan;
using Vk = Vulkan;
using System.Text;
using System.Collections.Generic;

namespace Game {
    public class Program {

        private string[] instanceExtensions = {

        };

        private string[] deviceExtensions = {
            VkConstants.VK_KHR_swapchain
        };

        private string[] validationLayers = {
            VkConstants.VK_LAYER_KHRONOS_vaidation
        };

        private string[] extensionsRequiredForValidationLayers = {
            VkConstants.VK_EXT_debug_report
        };

        public static int WIDTH = 400;
        public static int HEIGHT = 400;
        
        private static int TICK_RATE = 60;
        private static double TICK_TIME = 1.0 / TICK_RATE;

        private Window window;
        private Vk.Instance vkInstance;
        private Vk.PhysicalDevice vkPhysicalDevice;
        private Vk.Device vkDevice;
        private Vk.Queue vkGraphicsQueue;
        private Vk.Queue vkPresentationQueue;
        private Vk.SurfaceKhr vkSurface;
        private Vk.SwapchainKhr vkSwapchain;
        private Vk.DebugReportCallbackExt debugCallback;
        private Vk.Image[] vkSwapchainImages;
        private Vk.ImageView[] vkSwapchainImageViews;
        private QueueFamilyIndices vkQueueFamilies;
        private Vk.Format vkSwapchainImageFormat;
        private Vk.Extent2D vkSwapchainExtent;

        private double timeLastLoop = 0;
        private double accumulator = 0;

        private bool enableValidationLayers = false;
        private bool? validationLayersSupported = null;

        static void Main(string[] args) {
            Program program = new Program();
            program.Run();
        }

        private void Run() {
            this.enableValidationLayers = false;

            this.InitWindow();
            this.InitVulkan();
            this.MainLoop();
            this.Cleanup();
        }

        private void InitWindow() {
            ErrorCallback errorHandler = (GLFW.ErrorCode ErrorCode, IntPtr message) => {
                Console.WriteLine(ErrorCode);
                string errorMsg = Marshal.PtrToStringAuto(message);
                Console.WriteLine(errorMsg);
            };

            Glfw.SetErrorCallback(errorHandler);

            Glfw.Init();
            Glfw.WindowHint(Hint.ClientApi, GLFW.ClientApi.None);
            Glfw.WindowHint(Hint.Resizable, GLFW.Constants.False);

            
            this.timeLastLoop = Glfw.Time;

            this.window = Glfw.CreateWindow(WIDTH, HEIGHT, "Vulkan", Monitor.None, Window.None);
        }

        private void SetFrameBufferSize(IntPtr windowPointer, int width, int height) {
            Window window = Marshal.PtrToStructure<Window>(windowPointer);
            Console.WriteLine(window == this.window);
            Glfw.MakeContextCurrent(window);

            // Change this to Vulkan mmkay
            // Gl.Viewport(0, 0, width, height);
        }

        private void InitVulkan() {
            if(GLFW.Vulkan.IsSupported) {
                this.CreateVulkanInstance();

                if(this.ShouldUseValidationLayers()) {
                    this.debugCallback = VkHelper.RegisterDebugReportCallback(this.vkInstance, 
                        Vk.DebugReportFlagsExt.Debug, this.DebugReportCallback);
                }

                this.CreateWindowSurface();

                this.vkPhysicalDevice = VkHelper.SelectPhysicalDevice(this.vkInstance, this.CheckPhysicalDeviceSuitability);

                this.CreateLogicalDevice();

                this.vkGraphicsQueue = this.vkDevice.GetQueue(this.vkQueueFamilies.GraphicsFamily.Value, 0);
                this.vkPresentationQueue = this.vkDevice.GetQueue(this.vkQueueFamilies.PresentFamily.Value, 0);

                this.CreateSwapchain();
                this.CreateImageViews();
            } else {
                Console.WriteLine("No Vulkan :(");
            }

            //Glfw.MakeContextCurrent(this.window);
            //Glfw.SetFramebufferSizeCallback(this.window, this.SetFrameBufferSize);
        }

        private void CreateVulkanInstance() {
            InstanceBuilder builder = new InstanceBuilder();

            builder.SetApplicationName("Thingy");
            builder.SetApplicationVersion(Vk.Version.Make(1, 0, 0));
            builder.SetEngineName("None");
            builder.SetEngineVersion(Vk.Version.Make(1, 0, 0));
            builder.SetApiVersion(Vk.Version.Make(1, 0, 0));

            builder.EnableExtensions(VkHelper.GetGLFWRequiredInstanceExtensions());
            builder.EnableExtensions(this.instanceExtensions);

            if(this.ShouldUseValidationLayers()) {
                builder.EnableExtensions(this.extensionsRequiredForValidationLayers);
                builder.EnableValidationLayers(this.validationLayers);
            }

            try {
                this.vkInstance = builder.Create();
            } catch(Vk.ResultException result) {
                Console.Error.WriteLine("An error occurred while creating the Vulkan instance.");
                Console.Error.WriteLine(result.Result);
            }
        }

        private void CreateWindowSurface() {
            try {
                IntPtr allocatorPointer = IntPtr.Zero;
                IntPtr surfacePointer   = new IntPtr();
                IntPtr instancePointer  = VkHelper.InstancePointer(this.vkInstance);
                IntPtr windowPointer    = this.window;

                GLFW.Vulkan.CreateWindowSurface(instancePointer, windowPointer, allocatorPointer, out surfacePointer);

                this.vkSurface = VkHelper.CreateSurfaceFromHandle(surfacePointer);
            } catch(Vk.ResultException result) {
                Console.Error.WriteLine("An error occurred creating the window surface.");
                Console.Error.WriteLine(result.Result);
            }
        }

        private bool ShouldUseValidationLayers() {
            if(this.enableValidationLayers) {
                if(this.validationLayersSupported == null) {
                    bool supported = VkHelper.CheckValidationLayerSupport(this.validationLayers);
                    this.validationLayersSupported = supported;

                    if(!supported) {
                        Console.Error.WriteLine("Validation Layers not supported.");
                    }

                    return supported;
                } else {
                    return this.validationLayersSupported.Value;
                }
            } else {
                return false;
            }
        }

        private bool DebugReportCallback(DebugCallbackArgs args) {
            StringBuilder builder = new StringBuilder();
            builder.Append($"[{args.Flags}] [{args.LayerPrefix}]");
            builder.Append($" ");
            builder.Append($"[{Enum.GetName(args.ObjectType.GetType(), args.ObjectType)} at ({args.Location})]");
            builder.Append($"{args.Message} ({args.MessageCode})");

            Console.WriteLine(builder.ToString());

            return false;
        }

        private bool CheckPhysicalDeviceSuitability(Vk.PhysicalDevice device) {
            bool queueSupport = VkHelper.CheckPhysicalDeviceQueueFamilySupport(device, 
                    Vk.QueueFlags.Graphics, this.vkSurface, out this.vkQueueFamilies);

            if(!queueSupport) return false;

            bool extensionSupport = VkHelper.CheckDeviceExtensionSupport(device,
                    this.deviceExtensions);

            if(!extensionSupport) return false;

            SwapchainSupportDetails swapchainSupport = 
                    VkHelper.QuerySwapchainSupport(device, this.vkSurface);

            bool swapchainAdequate = swapchainSupport.formats.Length != 0 &&
                    swapchainSupport.presentModes.Length != 0;

            return swapchainAdequate;
        }

        private void CreateLogicalDevice() {
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

            builder.EnableExtensions(this.deviceExtensions);

            // Vk.PhysicalDeviceFeatures deviceFeatures = new Vk.PhysicalDeviceFeatures();
            // builder.SetFeatures(deviceFeatures);
            
            if(this.ShouldUseValidationLayers()) {
                builder.EnableValidationLayer(VkConstants.VK_LAYER_KHRONOS_vaidation);
            }

            try {
                this.vkDevice = builder.Create(this.vkPhysicalDevice);
            } catch(Vk.ResultException result) {
                Console.Error.WriteLine("An error occurred while creating the logical device.");
                Console.Error.WriteLine(result.Result);
            }
        }

        private void CreateSwapchain() {
            SwapchainSupportDetails support = VkHelper.QuerySwapchainSupport(this.vkPhysicalDevice, this.vkSurface);
            Vk.SurfaceFormatKhr format      = VkHelper.SelectSwapSurfaceFormat(support.formats);
            Vk.PresentModeKhr presentMode   = VkHelper.SelectSwapPresentMode(support.presentModes);
            Vk.Extent2D extent              = VkHelper.SelectSwapExtent(support.capabilities, WIDTH, HEIGHT);

            uint imageCount = support.capabilities.MinImageCount + 1;

            if(support.capabilities.MaxImageCount > 0 && imageCount > support.capabilities.MaxImageCount) {
                imageCount = support.capabilities.MaxImageCount;
            }

            Vk.SwapchainCreateInfoKhr createInfo = new Vk.SwapchainCreateInfoKhr();
            createInfo.Surface          = this.vkSurface;
            createInfo.MinImageCount    = imageCount;
            createInfo.ImageFormat      = format.Format;
            createInfo.ImageColorSpace  = format.ColorSpace;
            createInfo.ImageExtent      = extent;
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

            createInfo.PreTransform = support.capabilities.CurrentTransform;
            createInfo.CompositeAlpha = Vk.CompositeAlphaFlagsKhr.Opaque; // Blending with other windows? :o
            createInfo.PresentMode = presentMode;
            createInfo.Clipped = true;
            createInfo.OldSwapchain = null;

            try {
                this.vkSwapchain = this.vkDevice.CreateSwapchainKHR(createInfo);
            } catch(Vk.ResultException result) {
                Console.Error.WriteLine("An error occurred while creating the swapchain.");
                Console.Error.WriteLine(result.Result);
            }

            this.vkSwapchainImages = this.vkDevice.GetSwapchainImagesKHR(this.vkSwapchain);
        }

        private void CreateImageViews() {
            this.vkSwapchainImageViews = new Vk.ImageView[this.vkSwapchainImages.Length];

            for(int i = 0; i < this.vkSwapchainImageViews.Length; i++) {
                Vk.ImageViewCreateInfo createInfo = new Vk.ImageViewCreateInfo();
                createInfo.Image = this.vkSwapchainImages[i];
                createInfo.ViewType = Vk.ImageViewType.View2D;
                createInfo.Format = this.vkSwapchainImageFormat;

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
                    this.vkSwapchainImageViews[i] = this.vkDevice.CreateImageView(createInfo);
                } catch(Vk.ResultException result) {
                    Console.Error.WriteLine($"An error occurred while creating image view {i}.");
                    Console.Error.WriteLine(result.Result);
                }
            }
        }

        private void MainLoop() {
            while(!Glfw.WindowShouldClose(this.window)) {
                double currentTime = Glfw.Time;
                double deltaTime = currentTime - this.timeLastLoop;

                this.accumulator += deltaTime;
                this.timeLastLoop = currentTime;

                if(this.accumulator >= TICK_TIME * 4) {
                    this.accumulator = TICK_TIME * 4;
                }

                while(this.accumulator >= TICK_TIME) {
                    this.accumulator -= TICK_TIME;
                }

                Glfw.PollEvents();
            }
        }

        private void Cleanup() {
            if(GLFW.Vulkan.IsSupported) {
                foreach(Vk.ImageView imageView in this.vkSwapchainImageViews) {
                    this.vkDevice.DestroyImageView(imageView);
                }

                this.vkDevice.Destroy();
                
                if(this.ShouldUseValidationLayers()) {
                    this.vkInstance.DestroyDebugReportCallbackEXT(this.debugCallback);
                }

                this.vkDevice.DestroySwapchainKHR(this.vkSwapchain);
                this.vkDevice.Destroy();
                this.vkInstance.Dispose();
            }

            Glfw.DestroyWindow(this.window);
            Glfw.Terminate();
        }
    }
}
