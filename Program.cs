using System;
using GLFW;
using OpenGL;
using Game.Math;
using Game.Physics;

namespace Game {
    public class Program {

        public static int WIDTH = 400;
        public static int HEIGHT = 400;
        
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
            this.InitOpenGL();
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
            this.timeLastLoop = Glfw.Time;

            Glfw.WindowHint(Hint.ClientApi, GLFW.ClientApi.OpenGL);
            Glfw.WindowHint(Hint.Resizable, GLFW.Constants.False);
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, GLFW.Profile.Core);

            // OS X
            // Glfw.WindowHint(Hint.OpenglForwardCompatible, GLFW.Constants.True);
            this.window = Glfw.CreateWindow(WIDTH, HEIGHT, "OpenGL", Monitor.None, Window.None);
        }

        private void SetFrameBufferSize(IntPtr windowPointer, int width, int height) {
            Window window = System.Runtime.InteropServices.Marshal.PtrToStructure<Window>(windowPointer);
            Console.WriteLine(window == this.window);
            Glfw.MakeContextCurrent(window);
            Gl.Viewport(0, 0, width, height);
        }

        private void InitOpenGL() {
            Gl.Initialize();
            Glfw.MakeContextCurrent(this.window);
            Gl.BindAPI();

            Glfw.MakeContextCurrent(this.window);
            Glfw.SetFramebufferSizeCallback(this.window, this.SetFrameBufferSize);
            
            Gl.Viewport(0, 0, WIDTH, HEIGHT);
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
            Glfw.DestroyWindow(this.window);
            Glfw.Terminate();
        }
    }
}
