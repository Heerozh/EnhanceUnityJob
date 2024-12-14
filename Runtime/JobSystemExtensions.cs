using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace EnhanceJobSystem
{
    public static class NativeArrayExtensions
    {
        /// <summary>
        ///  <para>Creates a slice of flatten 2D NativeArray.</para>
        /// Return view from array[i * width, (i+1) * width).
        /// </summary>
        /// <param name="array"></param>
        /// <param name="i">axis 0 index</param>
        /// <param name="width">axis 1 length</param>
        /// <typeparam name="T">NativeArray bittable type</typeparam>
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

    public static class JobHandleExtensions
    {
        /// <summary>
        ///  <para>Async way to wait for a job to complete.</para>
        /// e.g: await job.Schedule().WaitComplete();
        /// </summary>
        /// <param name="handle"></param>
        public static async Awaitable WaitComplete(this JobHandle handle)
        {
#if UNITY_EDITOR
            handle.Complete();
#else
            while (true)
            {
                if (handle.IsCompleted)
                {
                    handle.Complete();
                    return;
                }

                await Awaitable.NextFrameAsync();
            }
#endif
        }
    }


    public interface IJobBunch : IJob
    {
        void Slice(int i, int workers);
    }

    public struct BunchJobHandle
    {
        public JobHandle[] Handles;

        public void Complete()
        {
            foreach (var handle in Handles)
                handle.Complete();
        }

        public async Awaitable WaitComplete()
        {
#if UNITY_EDITOR
            Complete();
#else
            while (true)
            {
                // Debug.Log(string.Join(",", Handles.Select(h => h.IsCompleted)));
                if (Handles.All(h => h.IsCompleted))
                {
                    Complete();
                    return;
                }

                await Awaitable.NextFrameAsync();
            }
#endif
        }
    }

    public static class JobDataExtensions
    {
        /// <summary>
        ///  <para>Schedule Parallel job but uses IJobBunch(inherited from IJob) interface.</para>
        /// `IJobBunch.Slice` method will be called when each job before scheduled.
        /// method can be written like this:
        ///  public void Slice(int i, int workers) {
        ///     result = result.Slice2D(i, result.Length / workers);
        ///     _i = i;
        /// }
        /// All your sliced data should add [NativeDisableContainerSafetyRestriction] attribute.
        /// </summary>
        /// <param name="jobData">The job(IJob struct) and data to schedule.</param>
        /// <param name="workers">The number of iterations to execute</param>
        /// <param name="dependsOn">The JobHandle of the job's dependency.</param>
        /// <typeparam name="T">NativeArray bittable type</typeparam>
        /// <returns></returns>
        public static BunchJobHandle ScheduleBunch<T>(this T jobData, int workers,
            JobHandle dependsOn = default(JobHandle)) where T : struct, IJobBunch
        {
            // Dictionary<string, PropertyInfo> arrayNames =
            //     (from prop in jobData.GetType().GetProperties()
            //         where Attribute.IsDefined(prop, typeof(NativeDisableContainerSafetyRestrictionAttrib))
            //         select prop).ToDictionary(prop => prop.Name);

            // loop create job same as jobData
            var handle = new BunchJobHandle
            {
                Handles = new JobHandle[workers]
            };

            for (var i = 0; i < workers; i++)
            {
                var paraJobData = jobData; // shallow copy
                paraJobData.Slice(i, workers);
                handle.Handles[i] = paraJobData.Schedule(dependsOn);
            }

            return handle;
        }
    }
}
