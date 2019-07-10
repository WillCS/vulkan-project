using System;
using GLFW;
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

        static void Main(string[] args) {Program program = new Program();
            Circle circle = new Circle(new Vector2(2, 2), 1);
            Ray ray = new Ray(new Vector2(0, 0), new Vector2(1, 1));
            foreach(Vector2 intersection in circle.CastRay(ray)) {
                Console.WriteLine(intersection);
            }

            // Program program = new Program();
            // program.Run();
        }

        private void Run() {
            this.InitWindow();
            this.InitOpenGL();
            this.MainLoop();
            this.Cleanup();
        }

        private void InitWindow() {
            ErrorCallback errorHandler = (ErrorCode ErrorCode, IntPtr message) => {
                Console.WriteLine(ErrorCode);
                string errorMsg = System.Runtime.InteropServices.Marshal.PtrToStringUTF8(message);
                Console.WriteLine(errorMsg);
            };

            Glfw.SetErrorCallback(errorHandler);

            Glfw.Init();
            this.timeLastLoop = Glfw.Time;

            Glfw.WindowHint(Hint.ClientApi, GLFW.ClientApi.OpenGL);
            Glfw.WindowHint(Hint.Resizable, GLFW.Constants.False);

            this.window = Glfw.CreateWindow(WIDTH, HEIGHT, "OpenGL", Monitor.None, Window.None);
        }

        private void InitOpenGL() {

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
