using System;
using GLFW;
using OpenGL;
using Game.Math;
using Game.Physics;
using Game.Vulkan;
using Vk = Vulkan;

namespace Game {
    public class Program {

        private string[] requiredExtensions = {
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

        private double timeLastLoop = 0;
        private double accumulator = 0;

        private bool enableValidationLayers = false;
        private bool? validationLayersSupported = null;

        static void Main(string[] args) {
            Program program = new Program();
            program.Run();
        }

        private void Run() {
            this.enableValidationLayers = true;

            this.InitWindow();
            this.InitVulkan();
            this.MainLoop();
            this.Cleanup();
        }

        private void InitWindow() {
            ErrorCallback errorHandler = (GLFW.ErrorCode ErrorCode, IntPtr message) => {
                Console.WriteLine(ErrorCode);
                string errorMsg = System.Runtime.InteropServices.Marshal.PtrToStringAuto(message);
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
            Window window = System.Runtime.InteropServices.Marshal.PtrToStructure<Window>(windowPointer);
            Console.WriteLine(window == this.window);
            Glfw.MakeContextCurrent(window);

            // Change this to Vulkan mmkay
            // Gl.Viewport(0, 0, width, height);
        }

        private void InitVulkan() {
            if(GLFW.Vulkan.IsSupported) {
                this.CreateVulkanInstance();

                if(this.ShouldUseValidationLayers()) {
                    VkHelper.RegisterDebugReportCallback(this.vkInstance, 
                        Vk.DebugReportFlagsExt.Debug, this.DebugReportCallback);
                }
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
            builder.EnableExtensions(this.requiredExtensions);

            if(this.ShouldUseValidationLayers()) {
                builder.EnableExtensions(this.extensionsRequiredForValidationLayers);
                builder.EnableValidationLayers(this.validationLayers);
            }

            try {
                this.vkInstance = builder.Create();
            } catch(Vk.ResultException result) {
                Console.Error.WriteLine("An error occurred while creating the Vulkan instance.");
                Console.Error.WriteLine(result.Message);
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
            Console.WriteLine($"Vulkan Debug Call by Layer {args.LayerPrefix}");
            Console.WriteLine($"Object Type: {args.ObjectType} at {args.Location}");
            Console.WriteLine($"Flag: {args.Flags}");
            Console.WriteLine($"{args.MessageCode}: {args.Message}");

            return false;
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
                this.vkInstance.Dispose();
            }

            Glfw.DestroyWindow(this.window);
            Glfw.Terminate();
        }
    }
}
