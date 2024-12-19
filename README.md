# Enhance Unity JobSystem
Enhance Unity Job System

## Install

Unity -> Window -> Package Manager -> Add package from git URL

```
https://github.com/Heerozh/EnhanceUnityJob.git
```

## Adaptive Job Schedule for WebGL single thread mode

In WebGL single thread mode, job system will block game.

Use `await JobData.AdaptSchedule()` to schedule job. If in multi-thread mode, 
it will call `Schedule()` then `return CompleteAsync()` method normally, 
otherwise, `AdaptSchedule` will `yield` back on each step,
just like using coroutines to perform large tasks. 

Only support for `IJobParallelForBunch` or `IJobParallelFor`.

## Other Help Methods
### NativeArray.Slice2D(int i, int width)

Return view from array[i * width, (i+1) * width)

### async Awaitable JobHandle.CompleteAsync()

Allow `await` in an asynchronous function to wait for the job to complete.

## Math

### float3.Slerp

Same as vector3.slerp but with float3 type (burst happy).

### float3.FbmNoise

Simplex fractional Brownian motion noise with float3 type.
