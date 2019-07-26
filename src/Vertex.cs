using System.Runtime.InteropServices;
using Game.Math;
using Vk = Vulkan;

namespace Game {
    public static class Constants {
        public static uint DOUBLE_SIZE_BYTES = 8;
        public static uint FLOAT_SIZE_BYTES = 4;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex {
        public NativeVector2 Position;
        public NativeVector3 Colour;

        public static Vk.VertexInputBindingDescription BindingDescription {
            get {
                var description = new Vk.VertexInputBindingDescription();

                description.Binding   = 0;
                description.Stride    = (uint) Marshal.SizeOf<Vertex>();
                description.InputRate = Vk.VertexInputRate.Vertex;

                return description;
            }
        }

        public static Vk.VertexInputAttributeDescription[] AttributeDescriptions {
            get {
                var descriptions = new Vk.VertexInputAttributeDescription[2];

                // Position
                descriptions[0].Binding  = 0;
                descriptions[0].Location = 0;
                descriptions[0].Format   = Vk.Format.R32G32Sfloat;
                descriptions[0].Offset   = (uint) Marshal.OffsetOf<Vertex>("Position");

                // Colour
                descriptions[1].Binding  = 0;
                descriptions[1].Location = 1;
                descriptions[1].Format   = Vk.Format.R32G32B32Sfloat;
                descriptions[1].Offset   = (uint) Marshal.OffsetOf<Vertex>("Colour");

                return descriptions;
            }
        }
    }
}