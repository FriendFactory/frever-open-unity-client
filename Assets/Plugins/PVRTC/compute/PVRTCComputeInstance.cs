using System;
using Unity.Mathematics;
using UnityEngine;

public class PVRTCComputeInstance : IDisposable 
{
    public struct ColorSet
    {
        public int4 colorA;
        public int4 colorB;
    }

    // Compute buffers used for a compression instance
    public ComputeBuffer colorCompressedBuffer;
    public ComputeBuffer colorUncompressedBuffer;
    public ComputeBuffer modulationBuffer;
    public ComputeBuffer compressedBuffer;

    public PVRTCComputeInstance(int textureWidth, int mipmaps)
    {
        int bufferSize = CalculateBufferSize(textureWidth, mipmaps);

        int sizeofColorSet = sizeof(int) * 8;
        colorCompressedBuffer = new ComputeBuffer(bufferSize, sizeofColorSet);
        colorUncompressedBuffer = new ComputeBuffer(bufferSize, sizeofColorSet);
        modulationBuffer = new ComputeBuffer(bufferSize, sizeof(int));
        compressedBuffer = new ComputeBuffer(bufferSize, sizeof(int) * 2);
    }
    
    public void Dispose() => Dispose(true);

    protected virtual void Dispose(bool disposing)
    {
        DisposeBuffer(ref colorCompressedBuffer);
        DisposeBuffer(ref colorUncompressedBuffer);
        DisposeBuffer(ref modulationBuffer);
        DisposeBuffer(ref compressedBuffer);
    }

    private void DisposeBuffer(ref ComputeBuffer buffer)
    {
        if (buffer != null)
        {
            buffer.Dispose();
            buffer = null;
        } 
    }
    
    // Helper function to calculate efficiently the log2 of a uint
    public static int ilog2(uint value)
    {
        return sizeof(uint) * 8 - math.lzcnt(value) - 1;
    }

    // Functions co calculate the max number of currently supported Mipmaps
    public static int MaxPVRTCMipmapSupported(int textureWidth)
    {
        int fullNumberOfMipmaps = ilog2((uint) textureWidth) + 1;
        return math.max(1, fullNumberOfMipmaps);     // Change this line to stop supporting lower mipmaps. For example the following code will skip the 5 lower mipmaps: return math.max(1, fullNumberOfMipmaps - 5); 
    }
    
    public static int PVRTCMipmapCount(int textureWidth, int mipmaps)
    {
        int fullNumberOfMipmaps = ilog2((uint) textureWidth) + 1;
        int targetMipmaps = math.min(fullNumberOfMipmaps, mipmaps);        
        return targetMipmaps;
    }
    
    // Function to calculate the size of the buffers based on the number of mipmaps
    public static int CalculateBufferSize(int textureWidth, int mipmaps)
    {
        int blocks = textureWidth / 4;

        int totalGroups = 0;
        
        int targetMipmaps = PVRTCMipmapCount(textureWidth, mipmaps);
        for (int mipmapIndex = 0; mipmapIndex < targetMipmaps; ++mipmapIndex)
        {
            totalGroups += blocks * blocks;
            blocks >>= 1;
            blocks = Mathf.Max(blocks, 2);    // The format doesn't go lower than this 8x8 texture => 2x2 block
        }

        return totalGroups;
    }
}
