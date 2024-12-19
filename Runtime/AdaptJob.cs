using Unity.Jobs;
using UnityEngine;

namespace EnhanceJobSystem
{

    public static class AdaptJobDataExtensions
    {
        /// The IJob interface is always single-threaded and does not require the AdaptSchedule
        /// help, here included for future and consistency.
        public static async Awaitable AdaptSchedule<T>(this T jobData,
            Awaitable dependsOn = null) where T : struct, IJob
        {
            if(dependsOn != null)
                await dependsOn;
#if UNITY_WEBGL
            jobData.Execute();
#else
            await jobData.Schedule().CompleteAsync();
#endif
        }

        /// Schedule a parallel job async, but also adapt to WebGL platform (run job and yield every
        /// innerloopBatchCount step if in WASM)
        /// Note: AdaptSchedule cannot use any `AsDeferredJobArray()` array.
        public static async Awaitable AdaptSchedule<T>(
            this T jobData,
            int arrayLength,
            int innerloopBatchCount = 0,
            Awaitable dependsOn = null)
            where T : struct, IJobParallelFor
        {
            if(dependsOn != null)
                await dependsOn;
            if (innerloopBatchCount <= 0)
            {
                // On a multithreaded platform, it runs on 32 works.
                // and on webgl, it is executed in 32 frames.
                innerloopBatchCount = arrayLength / 32;
            }
#if UNITY_WEBGL
            for (int i = 0; i < arrayLength; i += innerloopBatchCount)
            {
                for (int j = 0; j < innerloopBatchCount && (i+j) < arrayLength; j++)
                {
                    jobData.Execute(i + j);
                }
                await Awaitable.NextFrameAsync();
            }
#else
            await jobData.Schedule(arrayLength, innerloopBatchCount).CompleteAsync();
#endif
        }
    }


}
