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
        private static int TICK_RATE = 60;
        private static double TICK_TIME = 1.0 / TICK_RATE;

        private Window window;

        private double timeLastLoop = 0;
        private double accumulator = 0;

        static void Main(string[] args) {
            Program program = new Program();
            program.Run();
        }

        private void Run() {
            this.InitWindow();
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

            this.window = new Window(640, 480, "Vulkan");
            this.window.InitVulkanContext();
        }

        private void MainLoop() {
            VkContext vulkan = this.window.VulkanContext;
            while(!this.window.ShouldClose()) {
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

                if(!(this.window.FramebufferWidth == 0 || this.window.FramebufferHeight == 0)) {
                    vulkan.DrawFrame();
                    vulkan.currentFrame = 
                            (vulkan.currentFrame + 1) % vulkan.maxFramesInFlight;
                }
            }

            vulkan.WaitForIdle();
        }

        private void Cleanup() {
            // Destroy Window
            this.window.Destroy();
            Glfw.Terminate();
        }
    }
}
