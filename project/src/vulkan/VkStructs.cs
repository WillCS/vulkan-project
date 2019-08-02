using System;
using System.Runtime.InteropServices;
using Project.Native;
using Vk = Vulkan;

namespace Project.Vulkan {
    /// <summary>
    ///     Args passed to a debug callback, in a struct so as not to require 8
    ///     seperate arguments. Carries a message from a validation layer.
    /// </summary>
    public struct DebugCallbackArgs {
        /// <summary>
        ///     The severity of the message.
        /// </summary>
        public Vk.DebugReportFlagsExt Flags;
        
        /// <summary>
        ///     The type of object from which the message originated.
        /// </summary>
        public Vk.DebugReportObjectTypeExt ObjectType;
        
        /// <summary>
        ///     A prefix identifying the validation layer that sent the message.
        /// </summary>
        public string LayerPrefix;
        
        /// <summary>
        ///     A code identifying the message. Set by the validation layer to
        ///     indicate the test that caused that triggered this event.
        /// </summary>
        public int MessageCode;
        
        /// <summary>
        ///     The actual message; human readable. Ideally should be printed to
        ///     stdout or, in the event of good practice, a log file.
        /// </summary>
        public string Message;
        
        /// <summary>
        ///     A pointer to the object with which the issue is associated with.
        ///     It's possible for this to be a null pointer, in which case there
        ///     is no associated object.
        /// </summary>
        public ulong Object;
        
        /// <summary>
        ///     A Validation-layer defined value indicating the location of the
        ///     trigger causing this event. Optional.
        /// </summary>
        public IntPtr Location;

        /// <summary>
        ///     Pointer to the userdata that was passed in when the debug
        ///     callback was registered.
        /// </summary>
        public IntPtr UserDataPointer;
    }

    /// <summary>
    ///     Struct for keeping track of what queue families exist, and what their
    ///     queue family indices are.
    /// </summary>
    public struct QueueFamilyIndices {
        /// <summary>
        ///     The queue family index of a queue capable of receiving
        ///     graphics commands, if one exists.
        /// </summary>
        public uint? GraphicsFamily;

        /// <summary>
        ///     The queue family index of a queue capable of receiving
        ///     present commands, if one exists.
        /// </summary>
        public uint? PresentFamily;

        /// <summary>
        ///     Determine whether or not all the queue families we need exist
        ///     on our hardware.
        /// </summary>
        /// <returns>
        ///     Whether or not the required queue families exist.
        /// </returns>
        public bool AllFamiliesExist() {
            return this.GraphicsFamily.HasValue
                && this.PresentFamily.HasValue;
        }
    }

    /// <summary>
    ///     Return value of <see cref="VkContext.EnsureQueueFamilySupport"/>.
    ///     Contains all the currently supported: 
    ///     <see cref="Vk.SurfaceFormatKhr"/>
    ///     <see cref="Vk.SurfaceCapabilitiesKhr"/>
    ///     <see cref="Vk.PresentModeKhr"/>
    /// </summary>
    public struct SwapchainSupportDetails {
        public Vk.SurfaceCapabilitiesKhr capabilities;
        public Vk.SurfaceFormatKhr[]     formats;
        public Vk.PresentModeKhr[]       presentModes;
    }

    /// <summary>
    ///     Struct for storing information about a debug callback, from the time
    ///     before it's actually registered. Once it's registered, also stores
    ///     the actual debug callback object so that it can be dipsosed off when
    ///     it's no longer needed.
    /// </summary>
    public struct DebugCallbackData {
        /// <summary>
        ///     The debug callback (C# level) to be registered with Vulkan once
        ///     the instance is ready for callbacks to be registered.
        /// </summary>
        public VkHelper.DebugCallback    callback;

        /// <summary>
        ///     The registered debug callback, kept around so that it can be
        ///     disposed once it's no longer needed.
        /// </summary>
        public Vk.DebugReportCallbackExt wrapper;

        /// <summary>
        ///     The severities that this debug callback receives events from.
        /// </summary>
        public Vk.DebugReportFlagsExt    flags;
    }

    /// <summary>
    ///     Simple struct to keep a buffer and its memory together in a single
    ///     object so they can be passed around together and dealt with as a
    ///     unit.
    /// </summary>
    public struct BufferWithMemory {
        public Vk.Buffer       Buffer;
        public Vk.DeviceMemory Memory;

        /// <summary>
        ///     Wrapper around <see cref="Vk.Device.BindBufferMemory"/>
        /// </summary>
        /// <param name="device">
        ///     The Vulkan logical device which created the buffer and allocated
        ///     its memory.
        /// </param>
        /// <param name="offset">
        ///     The offset in the allocated memory block at which the buffer's data
        ///     should be written.
        /// </param>
        public void Bind(Vk.Device device, Vk.DeviceSize offset) {
            device.BindBufferMemory(this.Buffer, this.Memory, offset);
        }

        /// <summary>
        ///     Wrapper around <see cref="Vk.Device.DestroyBuffer"/> and 
        ///     <see cref="Vk.Device.FreeMemory"/>
        /// </summary>
        /// <param name="device">
        ///     The Vulkan logical device which created the buffer and allocated
        ///     its memory.
        /// </param>
        public void Destroy(Vk.Device device) {
            device.DestroyBuffer(this.Buffer);
            device.FreeMemory(this.Memory);
        }
    }

    /// <summary>
    ///     Simple struct to keep an image and its memory together in a single
    ///     object so they can be passed around together and dealt with as a
    ///     unit.
    /// </summary>
    public struct ImageWithMemory {
        public Vk.Image        Image;
        public Vk.DeviceMemory Memory;

        /// <summary>
        ///     Wrapper around <see cref="Vk.Device.BindImageMemory"/>
        /// </summary>
        /// <param name="device">
        ///     The Vulkan logical device which created the image and allocated
        ///     its memory.
        /// </param>
        /// <param name="offset">
        ///     The offset in the allocated memory block at which the image's data
        ///     should be written.
        /// </param>
        public void Bind(Vk.Device device, Vk.DeviceSize offset) {
            device.BindImageMemory(this.Image, this.Memory, offset);
        }
    }
    
    /// <summary>
    ///     Struct representing the objects passed through the uniform buffer to the
    ///     shader pipeline.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct UniformBufferObject {
        public Mat4 Model;
        public Mat4 View;
        public Mat4 Projection;
    }
}