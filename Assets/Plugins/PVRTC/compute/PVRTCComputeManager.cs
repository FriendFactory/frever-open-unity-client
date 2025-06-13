using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PVRTCComputeManager
{
    private static ComputeShader _shader;
    private static int _colorKernel;
    private static int _modulationKernel;
    private static int _packingKernel;
    private static ComputeBuffer _mortonBuffer;

    private static bool _initialised;
    
    public static bool IsInitialised
    {
        get { return _initialised; }
    }
    
    public static void Initialise(ComputeShader computeShader)
    {
        _shader = computeShader;
        _colorKernel = _shader.FindKernel("ColorKernel");
        _modulationKernel = _shader.FindKernel("ModulationKernel");
        _packingKernel = _shader.FindKernel("PackingKernel");

        // Resolving the morton table and storing it in a ComputeBuffer
        _mortonBuffer = MortonTableGenerator.GenerateAsComputeBuffer(512);        // If this value is changed, it will need to be updated in the shader too, inside GetMortonNumber
        
        _initialised = true;
    }

    public static void Dispose()
    {
        if (_initialised)
        {
            if (_mortonBuffer != null)
            {
                _mortonBuffer.Dispose();
                _mortonBuffer = null;
            }

            _initialised = false;
        }
    }

    public static void Compress<T>(Texture texture, int mipmapCount, Action<NativeArray<T>> callback) where T : struct
    {
        PVRTCComputeInstance computeInstance = new PVRTCComputeInstance(texture.width, mipmapCount);
        
        // Calculating how many 64 size groups do we need to execute 
        int threads = 64;
        int initialTotalGroups = PVRTCComputeInstance.CalculateBufferSize(texture.width, mipmapCount);
        int totalGroups = initialTotalGroups / threads;
        if (totalGroups * threads < initialTotalGroups)
        {
            totalGroups++;
        }

        // Color Kernel
        _shader.SetInt("imageWidth", texture.width);
        _shader.SetInt("maxValidId", initialTotalGroups);
        _shader.SetTexture(_colorKernel, "inputImage", texture);
        _shader.SetBuffer(_colorKernel, "mortonTable", _mortonBuffer);
        _shader.SetBuffer(_colorKernel, "colorCompressedWriteBuffer", computeInstance.colorCompressedBuffer);
        _shader.SetBuffer(_colorKernel, "colorUncompressedWriteBuffer", computeInstance.colorUncompressedBuffer);
        
        _shader.Dispatch(_colorKernel, totalGroups, 1, 1);
        
        // Modulation Kernel
        _shader.SetInt("imageWidth", texture.width);
        _shader.SetInt("maxValidId", initialTotalGroups);
        _shader.SetTexture(_modulationKernel, "inputImage", texture);
        _shader.SetBuffer(_modulationKernel, "mortonTable", _mortonBuffer);
        _shader.SetBuffer(_modulationKernel, "colorUncompressedReadBuffer", computeInstance.colorUncompressedBuffer);
        _shader.SetBuffer(_modulationKernel, "modulationWriteBuffer", computeInstance.modulationBuffer);

        _shader.Dispatch(_modulationKernel, totalGroups, 1, 1);
        
        // Output Kernel
        _shader.SetInt("maxValidId", initialTotalGroups);
        _shader.SetBuffer(_packingKernel, "colorCompressedReadBuffer", computeInstance.colorCompressedBuffer);
        _shader.SetBuffer(_packingKernel, "modulationReadBuffer", computeInstance.modulationBuffer);
        _shader.SetBuffer(_packingKernel, "packWriteBuffer", computeInstance.compressedBuffer);
        
        _shader.Dispatch(_packingKernel, totalGroups, 1, 1);

        AsyncGPUReadback.Request(computeInstance.compressedBuffer, (AsyncGPUReadbackRequest request) =>
        {
            if (request.hasError)
            {
                Debug.LogWarning($"[{nameof(PVRTCComputeManager)}] GPU readback error has been detected");
                return;
            }
            
            NativeArray<T> data = request.GetData<T>();

            callback(data);

            // The buffer memory is not ours and it crashes if we try to release it
            //data.Dispose();
            computeInstance.Dispose();
        });
    }
}
