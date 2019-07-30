using System;
using System.Runtime.InteropServices;
using GLFW;
using Project.Vulkan;
using Vk = Vulkan;
using System.Text;
using Project.Math;
using Project.Native;

namespace Project {
    public class Program {
        public static int WIDTH = 640;
        public static int HEIGHT = 480;
        public static bool RESIZED = false;
        
        private static int TICK_RATE = 60;
        private static double TICK_TIME = 1.0 / TICK_RATE;

        private Window window;
        private VkState vulkan;

        private double timeLastLoop = 0;
        private double accumulator = 0;

        static void Main(string[] args) {
            Program program = new Program();
            program.Run();
        }

        private void Run() {
            this.InitWindow();
            this.InitVulkan();
            this.MainLoop();
            this.Cleanup();
        }

        private void InitWindow() {
            Glfw.SetErrorCallback((GLFW.ErrorCode ErrorCode, IntPtr message) => {
                Console.WriteLine(ErrorCode);
                string errorMsg = Marshal.PtrToStringAuto(message);
                Console.WriteLine(errorMsg);
            });

            Glfw.Init();
            Glfw.WindowHint(Hint.ClientApi, GLFW.ClientApi.None);
            Glfw.WindowHint(Hint.Resizable, GLFW.Constants.False);
            
            this.timeLastLoop = Glfw.Time;

            this.window = Glfw.CreateWindow(WIDTH, HEIGHT, "Vulkan", Monitor.None, Window.None);
            Glfw.SetFramebufferSizeCallback(this.window, SetFrameBufferSize);
        }

        private static void SetFrameBufferSize(IntPtr windowPointer, int width, int height) {
            Program.RESIZED = true;
            WIDTH = width;
            HEIGHT = height;
        }

        private void InitVulkan() {
            if(GLFW.Vulkan.IsSupported) {
                this.vulkan = new VkState();

                vulkan.EnableValidationLayers();
                vulkan.RegisterDebugReportCallback(this.DebugReportCallback, 
                        Vk.DebugReportFlagsExt.Debug |
                        Vk.DebugReportFlagsExt.Error |
                        Vk.DebugReportFlagsExt.Information |
                        Vk.DebugReportFlagsExt.PerformanceWarning |
                        Vk.DebugReportFlagsExt.Warning);

                vulkan.SetWindow(this.window);
                vulkan.RegisterPhysicalDeviceSuitabilityCheck(this.CheckPhysicalDeviceSuitability);
                
                Vertex v1 = new Vertex();
                v1.Colour   = new Vector3(1.0, 0.0, 0.0);
                v1.Position = new Vector2(-0.5, -0.5);

                Vertex v2 = new Vertex();
                v2.Colour   = new Vector3(0.0, 1.0, 0.0);
                v2.Position = new Vector2( 0.5, -0.5);

                Vertex v3 = new Vertex();
                v3.Colour   = new Vector3(0.0, 0.0, 1.0);
                v3.Position = new Vector2(-0.5,  0.5);

                Vertex v4 = new Vertex();
                v4.Colour   = new Vector3(0.0, 1.0, 1.0);
                v4.Position = new Vector2( 0.5,  0.5);

                vulkan.AddVertex(v1);
                vulkan.AddVertex(v2);
                vulkan.AddVertex(v3);
                vulkan.AddVertex(v4);

                vulkan.AddIndices(new short[] { 0, 1, 3, 0, 3, 2 });

                vulkan.InitVulkan();
            } else {
                Console.WriteLine("No Vulkan :(");
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
            if(!this.vulkan.EnsureQueueFamilySupport(device, Vk.QueueFlags.Graphics)) {
                return false;
            }

            if(!VkHelper.CheckDeviceExtensionSupport(device, this.vulkan.DeviceExtensions)) {
                return false;
            }

            var swapchainSupport = this.vulkan.QuerySwapchainSupport(device);

            bool swapchainAdequate = swapchainSupport.formats.Length != 0 &&
                    swapchainSupport.presentModes.Length != 0;

            return swapchainAdequate;
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
                if(!(WIDTH == 0 || HEIGHT == 0)) {
                    this.vulkan.DrawFrame();
                    this.vulkan.currentFrame = 
                            (this.vulkan.currentFrame + 1) % this.vulkan.maxFramesInFlight;
                }
            }

            this.vulkan.WaitForIdle();
        }

        private void Cleanup() {
            if(GLFW.Vulkan.IsSupported) {
                this.vulkan.Cleanup();
            }

            // Destroy Window
            Glfw.DestroyWindow(this.window);
            Glfw.Terminate();
        }
    }
}
