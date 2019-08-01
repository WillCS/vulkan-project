using System;
using System.Runtime.InteropServices;
using GLFW;
using Project.Vulkan;
using Vk = Vulkan;
using System.Text;
using Project.Math;
using Project.Native;
using System.Collections.Generic;

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
                
                var vertices = new List<Vertex>();

                Vertex x1   = new Vertex();
                x1.Colour   = Vector3.UNIT_X;
                x1.Position = new Vector3(-100, 0, -100);
                vertices.Add(x1);

                Vertex x2   = new Vertex();
                x2.Colour   = Vector3.UNIT_X;
                x2.Position = new Vector3(-100, 0,  100);
                vertices.Add(x2);
                
                Vertex x3   = new Vertex();
                x3.Colour   = Vector3.UNIT_X;
                x3.Position = new Vector3( 100, 0, -100);
                vertices.Add(x3);
                
                Vertex x4   = new Vertex();
                x4.Colour   = Vector3.UNIT_X;
                x4.Position = new Vector3( 100, 0,  100);
                vertices.Add(x4);

                Vertex y1   = new Vertex();
                y1.Colour   = Vector3.UNIT_Y;
                y1.Position = new Vector3(0, -100, -100);
                vertices.Add(y1);

                Vertex y2   = new Vertex();
                y2.Colour   = Vector3.UNIT_Y;
                y2.Position = new Vector3(0, -100,  100);
                vertices.Add(y2);
                
                Vertex y3   = new Vertex();
                y3.Colour   = Vector3.UNIT_Y;
                y3.Position = new Vector3(0,  100, -100);
                vertices.Add(y3);
                
                Vertex y4   = new Vertex();
                y4.Colour   = Vector3.UNIT_Y;
                y4.Position = new Vector3(0,  100, 100);
                vertices.Add(y4);

                Vertex z1   = new Vertex();
                z1.Colour   = Vector3.UNIT_Z;
                z1.Position = new Vector3(-100, -100, 0);
                vertices.Add(z1);

                Vertex z2   = new Vertex();
                z2.Colour   = Vector3.UNIT_Z;
                z2.Position = new Vector3( 100, -100, 0);
                vertices.Add(z2);
                
                Vertex z3   = new Vertex();
                z3.Colour   = Vector3.UNIT_Z;
                z3.Position = new Vector3(-100,  100, 0);
                vertices.Add(z3);
                
                Vertex z4   = new Vertex();
                z4.Colour   = Vector3.UNIT_Z;
                z4.Position = new Vector3( 100,  100, 0);
                vertices.Add(z4);

                vulkan.AddVertices(vertices);

                vulkan.AddIndices(new short[] { 
                        0, 1, 3,
                        0, 3, 2,
                        3, 1, 0,
                        2, 3, 1,

                        4, 5, 7,
                        4, 7, 6,
                        7, 5, 4,
                        6, 7, 4,

                        8, 9, 11,
                        8, 11, 10,
                        11, 9, 8,
                        10, 11, 8 
                });

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
