using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Game.Math;
using GLFW;
using System.IO;
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

        public delegate bool PhysicalDeviceSuitabilityCheck(Vk.PhysicalDevice device);

        public static Vk.PhysicalDevice SelectPhysicalDevice(Vk.Instance instance, 
                PhysicalDeviceSuitabilityCheck check) {
            Vk.PhysicalDevice[] devices = instance.EnumeratePhysicalDevices();

            foreach(Vk.PhysicalDevice device in devices) {
                if(check.Invoke(device)) {
                    return device;
                }
            }

            return null;
        }

        public static bool CheckPhysicalDeviceQueueFamilySupport(Vk.PhysicalDevice device, 
                Vk.QueueFlags flags, Vk.SurfaceKhr surface, out QueueFamilyIndices queueFamilies) {
            Vk.QueueFamilyProperties[] props = device.GetQueueFamilyProperties();
            queueFamilies = new QueueFamilyIndices();

            for(uint i = 0; i < props.Length; i++) {
                Vk.QueueFamilyProperties family = props[i];
                if(family.QueueCount > 0) {
                    if(family.QueueFlags.HasFlag(flags)) {
                        queueFamilies.GraphicsFamily = i;
                    }

                    if(device.GetSurfaceSupportKHR(i, surface)) {
                        queueFamilies.PresentFamily = i;
                    }

                    if(queueFamilies.AllFamiliesExist()) {
                        return true;
                    }
                }
            }

            return false;
        }

        public static IntPtr InstancePointer(Vk.Instance instance) =>
            ((Vk.IMarshalling) instance).Handle;

        /// <summary>
        ///     VulkanSharp provides no way to make a SurfaceKhr given a pointer
        ///     to one, so there was no way to use GLFW's Vulkan compatibility
        ///     surface creation.
        ///
        ///     The solution is this awful thing.
        /// </summary>
        public static Vk.SurfaceKhr CreateSurfaceFromHandle(IntPtr handle) {
            Type surfaceType = typeof(Vk.SurfaceKhr);

            Vk.SurfaceKhr surface = (Vk.SurfaceKhr) FormatterServices.GetUninitializedObject(surfaceType);
            FieldInfo[] info = surfaceType.GetFields(BindingFlags.FlattenHierarchy |
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach(var i in info) {
                if(i.Name.Equals("m")) {
                    i.SetValue(surface, (ulong) handle.ToInt64());
                    break;
                }
            }

            return surface;
        }

        public static bool CheckDeviceExtensionSupport(Vk.PhysicalDevice device, IEnumerable<string> extensions) {
            Vk.ExtensionProperties[] props = device.EnumerateDeviceExtensionProperties();

            return extensions.All<string>((string requiredExtension) => 
                props.Any<Vk.ExtensionProperties>((Vk.ExtensionProperties extension) => 
                    extension.ExtensionName.Equals(requiredExtension)
                )
            );
        }

        public static SwapchainSupportDetails QuerySwapchainSupport(Vk.PhysicalDevice device, Vk.SurfaceKhr surface) {
            SwapchainSupportDetails support = new SwapchainSupportDetails();
            support.capabilities = device.GetSurfaceCapabilitiesKHR(surface);
            support.formats      = device.GetSurfaceFormatsKHR(surface);
            support.presentModes = device.GetSurfacePresentModesKHR(surface);

            return support;
        }

        public static Vk.SurfaceFormatKhr SelectSwapSurfaceFormat(Vk.SurfaceFormatKhr[] formats) {
            foreach(Vk.SurfaceFormatKhr format in formats) {
                if(format.Format == Vk.Format.R8G8B8A8Unorm 
                    && format.ColorSpace == Vk.ColorSpaceKhr.SrgbNonlinear) {
                    return format;        
                }
            }

            return formats[0];
        }

        public static Vk.PresentModeKhr SelectSwapPresentMode(Vk.PresentModeKhr[] presentModes) {
            Vk.PresentModeKhr fallback = Vk.PresentModeKhr.Fifo;

            foreach(Vk.PresentModeKhr presentMode in presentModes) {
                if(presentMode == Vk.PresentModeKhr.Mailbox) {
                    return presentMode;
                } else if(presentMode == Vk.PresentModeKhr.Immediate) {
                    fallback = presentMode;
                }
            }

            return fallback;
        }
        
        public static Vk.Extent2D SelectSwapExtent(Vk.SurfaceCapabilitiesKhr capabilities, 
                int width, int height) {
            if(capabilities.CurrentExtent.Width != Int32.MaxValue) {
                return capabilities.CurrentExtent;
            } else {
                Vk.Extent2D actualExtent = new Vk.Extent2D();
                actualExtent.Width = MathHelper.Clamp(capabilities.MinImageExtent.Width, 
                        (uint) width, capabilities.MaxImageExtent.Width);
                actualExtent.Height = MathHelper.Clamp(capabilities.MinImageExtent.Height, 
                        (uint) height, capabilities.MaxImageExtent.Height);
                return actualExtent;
            }
        }

        public static byte[] LoadShaderCode(string name) {
            byte[] bytecode = File.ReadAllBytes(name);
            return bytecode;
        }

        public static Vk.ShaderModule CreateShaderModule(Vk.Device device, byte[] bytecode) {
            Vk.ShaderModuleCreateInfo moduleInfo = new Vk.ShaderModuleCreateInfo();
            moduleInfo.CodeBytes = bytecode;
            moduleInfo.CodeSize = new UIntPtr((uint) bytecode.Length);
            
            return device.CreateShaderModule(moduleInfo);
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

    public struct QueueFamilyIndices {
        public uint? GraphicsFamily;
        public uint? PresentFamily;

        public bool AllFamiliesExist() {
            return this.GraphicsFamily.HasValue && this.PresentFamily.HasValue;
        }
    }

    public struct SwapchainSupportDetails {
        public Vk.SurfaceCapabilitiesKhr capabilities;
        public Vk.SurfaceFormatKhr[]     formats;
        public Vk.PresentModeKhr[]       presentModes;
    }
}