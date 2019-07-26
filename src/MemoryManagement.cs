using System;
using System.Runtime.InteropServices;

namespace Game {
    public static class MemoryManagement {
        public static void MarshalArray<T>(T[] array, IntPtr address, bool deleteOld) {
            int size = Marshal.SizeOf(typeof(T));

            for(int i = 0; i < array.Length; i++) {
                int offset = i * size;
                Marshal.StructureToPtr<T>(array[i], address + offset, deleteOld);
            }
        }
    }
}