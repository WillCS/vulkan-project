using Project.Vulkan;
using GLFW;
using System.Collections.Generic;
using System;
using Vk = Vulkan;
using Project.Math;
using Project.Native;
using System.Text;

namespace Project {
    /// <summary>
    ///     Wrapper around GLFW Windows - because they're gross and interacting
    ///     with them requires messing with pointers that _don't work_. I don't
    ///     wanna deal with that crap so it's abstracted away in here. Window
    ///     also owns its relevant Vulkan context. If multiple windows are ever
    ///     a thing I decide to do, this might need to change; but for now it's
    ///     completely fine.
    /// </summary>
    public class Window {
        
        /// <summary>
        ///     GLFW callbacks have to be **static**, and the ony way for you to
        ///     identify where they came from is by dereferencing the pointer
        ///     they give you. The problem is, dereferencing these pointers
        ///     _doesn't work_. Instead of trying to figure out the proper way
        ///     to do it, because I'm sure there is one, I'm just storing the
        ///     pointers alongside the Windows they reference in a dictionary
        ///     that the **static** callbacks can all reach and use to access
        ///     the Window they actually need.
        /// </summary>
        private static Dictionary<IntPtr, Window> windowPointers = new Dictionary<IntPtr, Window>();


        /// <summary>
        ///     GLFW.NET's wrapper around a pointer to a GLFW Window. I don't
        ///     want to deal with this thing so I built a wrapper class around
        ///     it. Everything is better now.
        ///  </summary>
        private GLFW.Window   glfwWindow;

        /// <summary> The width of the Window. </summary>
        private int           framebufferWidth;

        /// <summary> The height of the Window. </summary>
        private int           framebufferHeight;

        /// <summary> The title of the Window. </summary>
        private string        windowTitle;

        /// <summary>
        ///     The Vulkan surface for this Window. This is where rendering to
        ///     the Window happens.
        /// </summary>
        private Vk.SurfaceKhr vkSurface;

        /// <summary>
        ///     Whether or not vkSurface has been initialised. This is used to
        ///     make sure the surface is not accessed or used before it is
        ///     initialised.
        /// </summary>
        private bool          surfaceInitialised;

        /// <summary>
        ///     The Vulkan context for this Window. Handles everything to do
        ///     with the GPU. Currently only does graphics but at some point
        ///     compute would be nice too.
        /// </summary>
        private VkContext     vkContext;

        /// <summary>
        ///     Whether or not vkContext has been initialised. This is used to
        ///     make sure the context is not accessed or used before it is
        ///     initalised.
        /// </summary>
        private bool          contextInitialised;

        /// <summary>
        ///     Whether or not the window has been resized. This is checked
        ///     during the render loop by the Vulkan context to determine if
        ///     it needs to recreate the swapchain.
        /// </summary>
        public bool           HasBeenResized;

        /// <summary>
        ///     The width of this window. 
        /// 
        ///     When this property is changed the Window is resized, which
        ///     causes the swapchain to be recreated.
        /// </summary>
        public int FramebufferWidth {
            get {
                return this.framebufferWidth;
            }

            set {
                Glfw.SetWindowSize(this.glfwWindow, value, this.framebufferHeight);
                this.framebufferHeight = value;
                this.HasBeenResized    = true;
            }
        }

        /// <summary>
        ///     The height of this window. 
        /// 
        ///     When this property is changed the Window is resized, which
        ///     causes the swapchain to be recreated.
        /// </summary>
        public int FramebufferHeight {
            get {
                return this.framebufferHeight;
            }

            set {
                Glfw.SetWindowSize(this.glfwWindow, this.framebufferWidth, value);
                this.framebufferHeight = value;
                this.HasBeenResized    = true;
            }
        }

        /// <summary>
        ///     The Vulkan context for this Window. Handles everything to do
        ///     with the GPU. Currently only does graphics but at some point
        ///     compute would be nice too.
        ///     
        ///     This property cannot be changed once initialised, and if an
        ///     attempt to access it is made before initialisation, an
        ///     exception will be raised.
        /// </summary>
        public VkContext VulkanContext {
            get {
                if(this.contextInitialised) {
                    return this.vkContext;
                } else {
                    throw new System.Exception("Vulkan Context hasn't been initialised yet.");
                }
            }
        }

        /// <summary>
        ///     The Vulkan surface for this Window. This is where rendering to
        ///     the Window happens.
        ///     
        ///     This property cannot be changed once initialised, and if an
        ///     attempt to access it is made before initialisation, an
        ///     exception will be raised.
        /// </summary>
        public Vk.SurfaceKhr VulkanSurface {
            get {
                if(this.surfaceInitialised) {
                    return this.vkSurface;
                } else {
                    throw new System.Exception("Vulkan Surface hasn't been initialised yet.");
                }
            }
        }

        /// <summary>
        ///     Create a new Window. The Window will use **Vulkan**. I might
        ///     implement OpenGL support at a later date. Until then, this
        ///     constructor automatically sets the Window's ClientApi to Vulkan.
        /// </summary>
        /// <param name="width"> The width of the Window to create. </param>
        /// <param name="height"> The height of the Window to create. </param>
        /// <param name="title"> The title of the Window to create. </param>
        public Window(int width, int height, string title) {
            Glfw.WindowHint(Hint.ClientApi, GLFW.ClientApi.None);

            this.glfwWindow = Glfw.CreateWindow(width, height, title, 
                    GLFW.Monitor.None, GLFW.Window.None);

            windowPointers.Add(this.glfwWindow, this);

            this.framebufferWidth  = width;
            this.framebufferHeight = height;
            this.windowTitle       = title;

            this.contextInitialised = false;
            this.surfaceInitialised = false;
            this.HasBeenResized     = false;

            Glfw.SetFramebufferSizeCallback(this.glfwWindow, FramebufferSizeCallback);
        }

        /// <summary>
        ///     Initialise the Vulkan context for this window. This is not
        ///     initialised when the Window is constructed.
        /// 
        ///     Attempting to initialise the Vulkan context after it has already
        ///     been initialised will raise an exception.
        /// 
        ///     If Vulkan is not supported, or an error occurs during
        ///     initialisation, an exception will be raised.
        /// </summary>
        public void InitVulkanContext() {
            if(this.surfaceInitialised) {
                throw new System.Exception("Vulkan Context already created.");
            }

            if(!GLFW.Vulkan.IsSupported) {
                throw new System.Exception("Vulkan is not supported.");
            }

            this.vkContext = new VkContext();

            this.vkContext.EnableValidationLayers();
            this.vkContext.RegisterDebugReportCallback(this.DebugReportCallback, 
                    Vk.DebugReportFlagsExt.Debug |
                    Vk.DebugReportFlagsExt.Error |
                    Vk.DebugReportFlagsExt.Information |
                    Vk.DebugReportFlagsExt.PerformanceWarning |
                    Vk.DebugReportFlagsExt.Warning);

            this.vkContext.RegisterPhysicalDeviceSuitabilityCheck(this.CheckPhysicalDeviceSuitability);
            
            var rooms = new List<Tuple<Vector2, Vector2>>();
            rooms.Add(new Tuple<Vector2, Vector2>(new Vector2(-15, -15), new Vector2(15, 15)));
            rooms.Add(new Tuple<Vector2, Vector2>(new Vector2(-10, 20), new Vector2(10, 40)));
            rooms.Add(new Tuple<Vector2, Vector2>(new Vector2(-5, 40), new Vector2(5, 50)));
            rooms.Add(new Tuple<Vector2, Vector2>(new Vector2(-10, 50), new Vector2(60, 75)));
            rooms.Add(new Tuple<Vector2, Vector2>(new Vector2(15, -5), new Vector2(50, 5)));
            rooms.Add(new Tuple<Vector2, Vector2>(new Vector2(40, 5), new Vector2(50, 50)));

            var floorColour = new Vector3(0.75, 0.75, 0.75);

            short index = 0;
            foreach(var room in rooms) {
                var corner1 = room.Item1;
                var corner2 = room.Item2;

                var v1 = new Vertex();
                v1.Position = new Vector3(corner1.X, 0, corner1.Y); 
                v1.Colour   = floorColour;
                this.vkContext.AddVertex(v1);
                short i1 = index;

                var v2 = new Vertex();
                v2.Position = new Vector3(corner2.X, 0, corner1.Y);
                v2.Colour   = floorColour;
                this.vkContext.AddVertex(v2);
                short i2 = (short) (index + 1);

                var v3 = new Vertex();
                v3.Position = new Vector3(corner1.X, 0, corner2.Y);
                v3.Colour   = floorColour;
                this.vkContext.AddVertex(v3);
                short i3 = (short) (index + 2);

                var v4 = new Vertex();
                v4.Position = new Vector3(corner2.X, 0, corner2.Y);
                v4.Colour   = floorColour;
                this.vkContext.AddVertex(v4);
                short i4 = (short) (index + 3);

                this.vkContext.AddIndices(new short[] {
                    i4, i3, i1,
                    i2, i4, i1
                });

                index += 4;
            }

            this.vkContext.SetWindow(this);

            this.vkContext.InitVulkan();
            this.contextInitialised = true;
        }

        /// <summary>
        ///     Wrapper around <see cref="Glfw.WindowShouldClose" />.
        /// </summary>
        /// <returns> Whether or not this Window is due to close. </returns>
        public bool ShouldClose() {
            return Glfw.WindowShouldClose(this.glfwWindow);
        }

        /// <summary>
        ///     Create the Vulkan surface for this Window.
        /// 
        ///     Attempting to create the Vulkan surface after it has already
        ///     been created will raise an exception.
        /// 
        ///     If an error occurs during creation of the Vulkan surface,
        ///     an exception will be raised.
        /// </summary>
        public void CreateVulkanSurface() {
            if(this.surfaceInitialised) {
                throw new System.Exception("Vulkan Surface already created.");
            } else {
                try {
                    IntPtr allocatorPointer = IntPtr.Zero;
                    IntPtr surfacePointer   = new IntPtr();
                    IntPtr instancePointer  = VkHelper.InstancePointer(this.vkContext.Instance);
                    IntPtr windowPointer    = this.glfwWindow;

                    GLFW.Vulkan.CreateWindowSurface(instancePointer, windowPointer, allocatorPointer, out surfacePointer);

                    this.vkSurface = VkHelper.CreateSurfaceFromHandle(surfacePointer);
                    this.surfaceInitialised = true;
                } catch(Vk.ResultException result) {
                    throw new VkException("An error occurred creating the window surface.", result);
                }
            }
        }

        /// <summary>
        ///     Destroy the GLFW Window, the Vulkan surface, and the Vulkan
        ///     context. Should only be used when this window is no longer
        ///     needed. This will also remove this Window from the pointer
        ///     dictionary.
        /// </summary>
        public void Destroy() {
            windowPointers.Remove(this.glfwWindow);
            Glfw.DestroyWindow(this.glfwWindow);

            this.vkContext.Instance.DestroySurfaceKHR(this.vkSurface);
            this.vkContext.Destroy();
        }

        /// <summary>
        ///     The callback passed to the Vulkan debug report extension to get
        ///     debug output from validation layers.
        /// </summary>
        /// <param name="args">
        ///     A struct defining the properties of the callback.
        /// </param>
        /// <returns>
        ///     false - because if it returns true, the application will crash.
        /// </returns>
        private bool DebugReportCallback(DebugCallbackArgs args) {
            StringBuilder builder = new StringBuilder();
            builder.Append($"[{args.Flags}] [{args.LayerPrefix}]");
            builder.Append($" ");
            builder.Append($"[{Enum.GetName(args.ObjectType.GetType(), args.ObjectType)} at ({args.Location})]");
            builder.Append($"{args.Message} ({args.MessageCode})");

            Console.WriteLine(builder.ToString());

            return false;
        }

        /// <summary>
        ///     Callback function defining how a physical device should be
        ///     chosen when constructing the Vulkan context.
        /// </summary>
        /// <param name="device">
        ///     A Vulkan physical device to be analysed for suitability.
        /// </param>
        /// <returns>
        ///     Whether or not the given physical device is suitable.
        /// </returns>
        private bool CheckPhysicalDeviceSuitability(Vk.PhysicalDevice device) {
            if(!this.vkContext.EnsureQueueFamilySupport(device, Vk.QueueFlags.Graphics)) {
                return false;
            }

            if(!VkHelper.CheckDeviceExtensionSupport(device, this.vkContext.DeviceExtensions)) {
                return false;
            }

            var swapchainSupport = this.vkContext.QuerySwapchainSupport(device);

            bool swapchainAdequate = swapchainSupport.formats.Length != 0 &&
                    swapchainSupport.presentModes.Length != 0;

            return swapchainAdequate;
        }

        /// <summary>
        ///     Callback function for when the Window is resized. This function,
        ///     and others like it, are the reason for the pointer dictonary's
        ///     existence.
        /// </summary>
        /// <param name="windowPointer">
        ///     A pointer referencing the GLFW Window that was resized.
        /// </param>
        /// <param name="width"> The new width of the Window. </param>
        /// <param name="height"> The new height of the Window. </param>
        private static void FramebufferSizeCallback(IntPtr windowPointer, int width, int height) {
            if(windowPointers.ContainsKey(windowPointer)) {
                Window relevantWindow = windowPointers[windowPointer];
                relevantWindow.framebufferWidth  = width;
                relevantWindow.framebufferHeight = height;
                relevantWindow.HasBeenResized    = true;
            } else {
                throw new System.Exception("Framebuffer Size Callback called on a window that doesn't exist.");
            }
        }
    }
}