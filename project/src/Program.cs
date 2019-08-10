using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GLFW;
using Project.Vulkan;

namespace Project {
    public class Program {
        private static int TICK_RATE = 60;
        private static double TICK_TIME = 1.0 / TICK_RATE;

        private Window window;

        private Stack<IState> states;

        private double timeLastLoop = 0;
        private double accumulator = 0;

        public IState State {
            get => this.states.Peek();
        }

        static void Main(string[] args) {
            Program program = new Program();
            program.Run();
        }

        private void Run() {
            this.InitState();
            this.InitWindow();
            this.MainLoop();
            this.Cleanup();
        }

        private void InitState() {
            this.states = new Stack<IState>();
            this.states.Push(new TestState());
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

            this.window = new Window(this, 640, 480, "Vulkan");
            this.window.InitVulkanContext();
        }

        private void MainLoop() {
            VkContext vulkan = this.window.VulkanContext;
            while(!this.window.ShouldClose()) {
                double currentTime = Glfw.Time;
                double deltaTime   = currentTime - this.timeLastLoop;

                this.accumulator  += deltaTime;
                this.timeLastLoop = currentTime;

                if(this.accumulator >= TICK_TIME * 4) {
                    this.accumulator = TICK_TIME * 4;
                }

                while(this.accumulator >= TICK_TIME) {
                    this.accumulator -= TICK_TIME;
                }

                Glfw.PollEvents();

                if(!this.window.IsMinimised) {
                    vulkan.DrawFrame();
                    vulkan.NextFrame();
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
