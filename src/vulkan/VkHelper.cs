using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GLFW;
using Vk = Vulkan;

namespace Game.Vulkan {
    public static class VkHelper {
         [DllImport(Glfw.LIBRARY, EntryPoint = "glfwGetRequiredInstanceExtensions",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetRequiredInstanceExtensions(out uint count);

        /// <summary>
        ///     This is verbatim <see cref="GLFW.Vulkan.GetRequiredInstanceExtensions" />
        ///     but for whatever reason, calling it through GLFW causes segmentation
        ///     faults. I was going to tinker with it and try to fix it but I didn't
        ///     have to? Anyway, just look at GLFW's original documentation.
        /// </summary>
        public static string[] GetGLFWRequiredInstanceExtensions() {
            var ptr = GetRequiredInstanceExtensions(out var count);
            var extensions = new string[count];
            if (count > 0 && ptr != IntPtr.Zero) {
                var offset = 0;
                for (var i = 0; i < count; i++, offset += IntPtr.Size) {
                    var p = Marshal.ReadIntPtr(ptr, offset);
                    extensions[i] = Marshal.PtrToStringAnsi(p);
                }
            }

            return extensions.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        }

        public delegate bool DebugCallback(DebugCallbackArgs args);

        public delegate Vk.Bool32 LowLevelDebugCallback(
                Vk.DebugReportFlagsExt flags,
                Vk.DebugReportObjectTypeExt objType,
                UInt64 objectPointer, IntPtr locationPointer,
                Int32 messageCodePointer, IntPtr layerPrefixPointer,
                IntPtr messagePointer, IntPtr userDataPointer);

        public static Vk.DebugReportCallbackExt RegisterDebugReportCallback(Vk.Instance instance, 
                Vk.DebugReportFlagsExt flag, DebugCallback callback) {
            LowLevelDebugCallback container = BuildCallbackContainer(callback);
            IntPtr callbackPointer = Marshal.GetFunctionPointerForDelegate<LowLevelDebugCallback>(container);

            var callbackInfo = new Vk.DebugReportCallbackCreateInfoExt();
            callbackInfo.PfnCallback = callbackPointer;
            callbackInfo.Flags = flag;
            callbackInfo.UserData = IntPtr.Zero;

            return instance.CreateDebugReportCallbackEXT(callbackInfo);
        }

        private static LowLevelDebugCallback BuildCallbackContainer(DebugCallback callback) {
            return (flags, objectType, obj, location, 
                        messageCode, layerPrefixPointer, messagePointer, 
                        userDataPointer) => {
                String layerPrefix = Marshal.PtrToStringAuto(layerPrefixPointer);
                String message     = Marshal.PtrToStringAuto(messagePointer);

                // Deal with UserData later when we actually have some

                DebugCallbackArgs args = new DebugCallbackArgs();
                args.Flags           = flags;
                args.ObjectType      = objectType;
                args.LayerPrefix     = layerPrefix;
                args.MessageCode     = messageCode;
                args.Message         = message;
                args.Location        = location;
                args.UserDataPointer = userDataPointer;

                return callback.Invoke(args);
            };
        }

        public static void PrintSupportedVkExtensions() {
            Vk.ExtensionProperties[] extensionProps = Vk.Commands.EnumerateInstanceExtensionProperties();

            Console.WriteLine("Supported Extensions:");
            foreach(Vk.ExtensionProperties props in extensionProps) {
                Console.WriteLine(props.ExtensionName);
            }
        }

        public static void PrintSupportedVkValidationLayers() {
            Vk.LayerProperties[] layerProps = Vk.Commands.EnumerateInstanceLayerProperties();

            Console.WriteLine("Supported Validation Layers:");
            foreach(Vk.LayerProperties props in layerProps) {
                Console.WriteLine(props.LayerName);
            }
        }

        public static bool CheckValidationLayerSupport(IEnumerable<string> layers) {
            Vk.LayerProperties[] layerProps = Vk.Commands.EnumerateInstanceLayerProperties();

            foreach(string layerName in layers) {
                bool foundLayer = false;

                foreach(Vk.LayerProperties layer in layerProps) {
                    if(layer.LayerName.Equals(layerName)) {
                        foundLayer = true;
                        break;
                    }
                }

                if(!foundLayer) {
                    return false;
                }
            }

            return true;
        }
    }

    public struct DebugCallbackArgs {
        public Vk.DebugReportFlagsExt Flags;
        public Vk.DebugReportObjectTypeExt ObjectType;
        public string LayerPrefix;
        public int MessageCode;
        public string Message;
        public ulong Object;
        public IntPtr Location;
        public IntPtr UserDataPointer;
    }
}