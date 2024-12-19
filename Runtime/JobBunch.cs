using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace EnhanceJobSystem
{
    public static class JobHandleExtensions
    {
        /// <summary>
        ///  <para>Async way to wait for a job to complete.</para>
        /// e.g: await job.Schedule().CompleteAsync();
        /// </summary>
        /// <param name="handle"></param>
        public static async Awaitable CompleteAsync(this JobHandle handle)
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

        public async Awaitable CompleteAsync()
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

    public static class JobDataBunchExtensions
    {
        /// <summary>
        ///     <para>Schedule Parallel job but uses IJobBunch(inherited from IJob) interface.</para>
        ///     `IJobBunch.Slice` method will be called when each job before scheduled.
        ///     method can be written like this:
        ///     public void Slice(int i, int workers) {
        ///     result = result.Slice2D(i, result.Length / workers);
        ///     _i = i;
        ///     }
        ///     All your sliced data should add [NativeDisableContainerSafetyRestriction] attribute.
        /// </summary>
        /// <param name="jobData">The job(IJob struct) and data to schedule.</param>
        /// <param name="workers">The number of iterations to execute</param>
        /// <param name="dependsOn">The JobHandle of the job's dependency.</param>
        /// <typeparam name="T">NativeArray blittable type</typeparam>
        /// <returns></returns>
        public static BunchJobHandle ScheduleBunch<T>(this T jobData, int workers,
            JobHandle dependsOn = default) where T : struct, IJobBunch
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

        /// Schedule a parallel job async, but also adapt to WebGL platform (run job and yield every
        /// step if in WASM)
        /// Note: AdaptSchedule cannot use any `AsDeferredJobArray()` array.
        public static async Awaitable AdaptScheduleBunch<T>(this T jobData, int workers,
            Awaitable dependsOn = null) where T : struct, IJobBunch
        {
            if(dependsOn != null)
                await dependsOn;

#if UNITY_WEBGL
            for (var i = 0; i < workers; i++)
            {
                var paraJobData = jobData; // shallow copy
                paraJobData.Slice(i, workers);
                paraJobData.Execute();
                await Awaitable.NextFrameAsync();
            }
#else
            await jobData.ScheduleBunch(workers).CompleteAsync();
#endif
        }


    }

}
