using System.Runtime.InteropServices;
using Vk = Vulkan;

namespace Game.Native {
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex {
        public Vec2 Position;
        public Vec3 Colour;

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

    [StructLayout(LayoutKind.Sequential)]
    public class Vec2 {
        public float X;
        public float Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec3 {
        public float X;
        public float Y;
        public float Z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec4 {
        public float X;
        public float Y;
        public float Z;
        public float W;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class DVec2 {
        public double X;
        public double Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DVec3 {
        public double X;
        public double Y;
        public double Z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DVec4 {
        public double X;
        public double Y;
        public double Z;
        public double W;
    }

    /// <summary>
    ///  Column-major
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class Mat2 {
        public float A;
        public float D;
        public float B;
        public float C;
    }

    /// <summary>
    ///  Column-major
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class Mat3 {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public float[] elems;

        public Mat3() {
            this.elems = new float[9];
        }
    }

    /// <summary>
    ///  Column-major
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class Mat4 {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] elems;

        public Mat4() {
            this.elems = new float[16];
        }
    }
}