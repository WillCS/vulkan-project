using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Project.Math;
using GLFW;
using System.IO;
using Vk = Vulkan;
using Microsoft.Win32.SafeHandles;
using Project.Native;

namespace Project.Vulkan {
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
                IEnumerable<PhysicalDeviceSuitabilityCheck> checks) {
            Vk.PhysicalDevice[] devices = instance.EnumeratePhysicalDevices();

            foreach(Vk.PhysicalDevice device in devices) {
                foreach(var check in checks) {
                    if(!check.Invoke(device)) {
                        break;
                    }

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

        public static Vk.Buffer CreateBuffer(VkState vulkan, Vk.DeviceSize size,
                Vk.BufferUsageFlags usage, Vk.MemoryPropertyFlags memoryProps, 
                Vk.SharingMode sharingMode, out Vk.DeviceMemory bufferMemory) {
            var bufferInfo         = new Vk.BufferCreateInfo();
            bufferInfo.Size        = size;
            bufferInfo.Usage       = usage;
            bufferInfo.SharingMode = sharingMode;

            var buffer = vulkan.Device.CreateBuffer(bufferInfo);

            var memoryReqs = vulkan.Device.GetBufferMemoryRequirements(buffer);
            var allocInfo  = new Vk.MemoryAllocateInfo();
            allocInfo.AllocationSize  = memoryReqs.Size;
            allocInfo.MemoryTypeIndex = FindMemoryType(memoryReqs.MemoryTypeBits, 
                    vulkan.PhysicalDevice, memoryProps);

            bufferMemory = vulkan.Device.AllocateMemory(allocInfo);
            vulkan.Device.BindBufferMemory(buffer, bufferMemory, 0);

            return buffer;
        }

        public static void CopyBuffer(Vk.Buffer src, Vk.Buffer dest, Vk.DeviceSize size,
                VkState state) {
            var allocInfo = new Vk.CommandBufferAllocateInfo();
            allocInfo.Level              = Vk.CommandBufferLevel.Primary;
            allocInfo.CommandPool        = state.TransferCommandPool;
            allocInfo.CommandBufferCount = 1;

            var commandBuffer = state.Device.AllocateCommandBuffers(allocInfo)[0];

            var beginInfo   = new Vk.CommandBufferBeginInfo();
            beginInfo.Flags = Vk.CommandBufferUsageFlags.OneTimeSubmit;

            commandBuffer.Begin(beginInfo);

            var copyRegion       = new Vk.BufferCopy();
            copyRegion.SrcOffset = 0;
            copyRegion.DstOffset = 0;
            copyRegion.Size      = size;

            commandBuffer.CmdCopyBuffer(src, dest, copyRegion);
            commandBuffer.End();

            var submitInfo = new Vk.SubmitInfo();
            submitInfo.CommandBufferCount = 1;
            submitInfo.CommandBuffers = new Vk.CommandBuffer[] { commandBuffer };

            state.TransferQueue.Submit(submitInfo);
            state.TransferQueue.WaitIdle();

            state.Device.FreeCommandBuffer(state.TransferCommandPool, commandBuffer);
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
            FieldInfo handleField = surfaceType.GetField("m", BindingFlags.FlattenHierarchy |
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            handleField.SetValue(surface, (ulong) handle.ToInt64());
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
                Window window) {
            if(capabilities.CurrentExtent.Width != Int32.MaxValue) {
                return capabilities.CurrentExtent;
            } else {
                int width, height;
                Glfw.GetFramebufferSize(window, out width, out height);

                Vk.Extent2D actualExtent = new Vk.Extent2D();
                actualExtent.Width  = (uint) width;
                actualExtent.Height = (uint) height;
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

        public static uint FindMemoryType(uint typeFilter, Vk.PhysicalDevice physicalDevice, 
                Vk.MemoryPropertyFlags properties) {
            var memProps = physicalDevice.GetMemoryProperties();

            for(int i = 0; i < memProps.MemoryTypeCount; i++) {
                bool bitFieldSet = (typeFilter & (1 << i)) != 0;
                bool hasProperties = (memProps.MemoryTypes[i].PropertyFlags & properties) == properties;
                if(bitFieldSet && hasProperties) {
                    return (uint) i;
                }
            }

            throw new System.Exception("Failed to find suitable memory type!");
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
            return this.GraphicsFamily.HasValue
                && this.PresentFamily.HasValue;
        }
    }

    public struct SwapchainSupportDetails {
        public Vk.SurfaceCapabilitiesKhr capabilities;
        public Vk.SurfaceFormatKhr[]     formats;
        public Vk.PresentModeKhr[]       presentModes;
    }

    public struct DebugCallbackData {
        public VkHelper.DebugCallback    callback;
        public Vk.DebugReportCallbackExt wrapper;
        public Vk.DebugReportFlagsExt    flags;
    }

    public struct SwapchainPipeline {
        public Vk.SwapchainKhr    Swapchain;

        public Vk.Format          Format;
        public Vk.Extent2D        Extent;

        public uint               ImageCapacity {
            get => (uint) this.Images.Length;
        }

        public Vk.Image[]         Images;
        public Vk.ImageView[]     ImageViews;
        public Vk.Framebuffer[]   Framebuffers;
        public Vk.Buffer[]        UniformBuffers;
        public Vk.DeviceMemory[]  UniformBuffersMemory;
        public Vk.DescriptorPool  DescriptorPool;
        public Vk.DescriptorSet[] DescriptorSets;

        public Vk.RenderPass      RenderPass;
        public Vk.PipelineLayout  PipelineLayout;
        public Vk.Pipeline        Pipeline;

        public Vk.CommandBuffer[] CommandBuffers;

        private Vk.CommandPool    commandPool;
        private Vk.Device         device;

        public void Setup(Vk.Device device, Vk.CommandPool commandPool) {
            this.commandPool = commandPool;
            this.device = device;
        }

        public void Cleanup() {
            // Destroy Framebuffers
            foreach(Vk.Framebuffer framebuffer in this.Framebuffers) {
                this.device.DestroyFramebuffer(framebuffer);
            }

            // Free Command buffers
            this.device.FreeCommandBuffers(this.commandPool, this.CommandBuffers);

            // Destroy Pipeline
            this.device.DestroyPipeline(this.Pipeline);

            // Destroy Pipeline Layout
            this.device.DestroyPipelineLayout(this.PipelineLayout);

            // Destroy Render Pass
            this.device.DestroyRenderPass(this.RenderPass);

            // Destroy Image Views
            foreach(Vk.ImageView imageView in this.ImageViews) {
                this.device.DestroyImageView(imageView);
            }

            // Destroy Uniform Buffers and free their memory
            for(int i = 0; i < this.ImageCapacity; i++) {
                this.device.DestroyBuffer(this.UniformBuffers[i]);
                this.device.FreeMemory(this.UniformBuffersMemory[i]);
            }

            // Destroy Descriptor Pool
            this.device.DestroyDescriptorPool(this.DescriptorPool);

            // Destroy Swapchain
            this.device.DestroySwapchainKHR(this.Swapchain);
        }
    }

    public struct SwapchainParameters {
        public Vk.SurfaceFormatKhr         SurfaceFormat;
        public Vk.PresentModeKhr           PresentMode;
        public Vk.Extent2D                 Extent;
        public uint                        MinImageCount;
        public uint                        MaxImageCount;
        public Vk.SurfaceTransformFlagsKhr CurrentTransform;
    }

    
    [StructLayout(LayoutKind.Sequential)]
    public struct UniformBufferObject {
        public Mat4 Model;
        public Mat4 View;
        public Mat4 Projection;
    }
}