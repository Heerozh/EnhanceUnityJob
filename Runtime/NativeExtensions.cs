using Unity.Collections;

namespace EnhanceJobSystem
{
    public static class NativeArrayExtensions
    {
        /// <summary>
        ///     <para>Creates a slice of flatten 2D NativeArray.</para>
        ///     Return view from array[i * width, (i+1) * width).
        /// </summary>
        /// <param name="array"></param>
        /// <param name="i">axis 0 index</param>
        /// <param name="width">axis 1 length</param>
        /// <typeparam name="T">NativeArray blittable type</typeparam>
        /// <returns></returns>
        public static NativeArray<T> Slice2D<T>(this NativeArray<T> array, int i, int width)
            where T : struct
        {
            // NativeSlice 正在被官方抛弃，因为索引速度很慢
            return array.GetSubArray(i * width, width);
            // return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray(
            //     slice.GetUnsafePtr(), slice.Length, Allocator.None);
        }
    }




}
