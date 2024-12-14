# Enhance Unity JobSystem
Enhance Unity Job System, add "ScheduleBunch" and other help methods

## Install

Unity -> Window -> Package Manager -> Add package from git URL

```
https://github.com/Heerozh/EnhanceUnityJob.git
```

## Parallel For IJob

### ScheduleBunch and IJobBunch

`IJobParallelFor` is parallel for individual elements of an array, 
but sometimes, 
it is more useful for parallelizing each row of a 2D array, 
allowing for loop vectorization.

Therefore, 
this extension package implemented the `IJobBunch` interface,
inherited from `IJob`.

First, we create a flatten 2D array of [10*15], 
then schedule it using `ScheduleBunch(int workers)`. 
The workers parameter corresponds to the number of rows in the array, 
also representing the number of job threads.

```csharp
var results = new NativeArray<float>(10*15, Allocator.TempJob);
var jobData = new MyJob2D {
    result = results
};
var handle = jobData.ScheduleBunch(10);
handle.Complete();
Debug.Log(String.Join(",", results));
results.Dispose();
```

Next is the implementation of `MyJob2D`, 
which inherits from `IJobBunch`. 
When scheduling with `ScheduleBunch(int workers)`, 
it calls the `Slice(int i, int workers)` method for each `IJobBunch`, 
where `i` is the job number, ranging from 0 to `workers`. 
You need to implement the `Slice` method to organize the data that the i-th thread 
will process.

Here, I directly call `NativeArray.Slice2D(int i, int width)` to select the i-th row 
from the flattened 2D array and assign it back.

```csharp
[BurstCompile]
public struct MyJob2D : IJobBunch {
    [NativeDisableContainerSafetyRestriction] public NativeArray<float> result;
    int _i;

    public void Slice(int i, int workers) {
        result = result.Slice2D(i, result.Length / workers);
        _i = i;
    }

    public void Execute() {
        for (int j = 0; j < result.Length; j++) {
            result[j] = _i;
        }
    }
}
```

Result(Formatted)：

```
0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,
3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,
4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,
5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,
6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,
7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
9,9,9,9,9,9,9,9,9,9,9,9,9,9,9
UnityEngine.Debug:Log (object)
```

You need to add the attribute `[NativeDisableContainerSafetyRestriction]`
to the array you intend to write to, 
because multiple job threads are writing to the same array in parallel. 
This disables the safety checks, 
and your Slice method is responsible for ensuring safe writes.

This method make burst able to enable loop vectorization in parallel job, 
and faster than `IJobParallelFor` in some case.
However, the allocation efficiency of `IJob` is relatively low; 
with 100 Workers, it consumes 0.1ms. For more than 100, 
it is still recommended to use IJobParallelFor.

## Other Help Methods
### NativeArray.Slice2D(int i, int width)

Return view from array[i * width, (i+1) * width)

### async Awaitable JobHandle.WaitComplete()

Allow `await` in an asynchronous function to wait for the job to complete.

## Math

### float3.Slerp

Same as vector3.slerp but with float3 type (burst happy).

### float3.FractalNoise

Simplex fractal noise with float3 type.
