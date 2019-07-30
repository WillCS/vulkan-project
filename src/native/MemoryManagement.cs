using System;
using System.Runtime.InteropServices;

namespace Project {
    public static class MemoryManagement {
        public static void ArrayToPtr<T>(T[] array, IntPtr address, bool deleteOld) {
            int size = Marshal.SizeOf(typeof(T));

            for(int i = 0; i < array.Length; i++) {
                int offset = i * size;
                Marshal.StructureToPtr<T>(array[i], address + offset, deleteOld);
            }
        }

        public static T[] PtrToArray<T>(IntPtr address, int length) {
            int size = Marshal.SizeOf(typeof(T));
            T[] newArray = new T[length];

            for(int i = 0; i < length; i++) {
                int offset = i * size;
                newArray[i] = Marshal.PtrToStructure<T>(address + offset);
            }

            return newArray;
        }
    }
}