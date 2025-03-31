// -----------------------------------------------------------------------
// <copyright file="ArrayPoolBufferWriter.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Serialization
{
    using System;
    using System.Buffers;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public sealed class ArrayPoolBufferWriter : IBufferWriter<byte>, IDisposable
    {
        private static readonly ArrayPool<byte> arrayPool = ArrayPool<byte>.Create();
        private byte[] buffer;
        private int bytesWritten;
        private bool disposedValue;

        /// <summary>
        /// Gets the number of bytes written to the buffer.
        /// </summary>
        public int BytesWritten => this.bytesWritten;

        /// <summary>
        /// Gets the size of the buffer.
        /// </summary>
        public int BufferSize => this.buffer.Length;

        /// <summary>
        /// Creates a new instance of the <see cref="ArrayPoolBufferWriter"/> class.
        /// </summary>
        public ArrayPoolBufferWriter()
        {
            this.buffer = ArrayPoolBufferWriter.arrayPool.Rent(4096);
        }

        /// <summary>
        /// The destructor.
        /// </summary>
        ~ArrayPoolBufferWriter()
        {
            this.Dispose(disposing: false);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            if (this.bytesWritten + count > this.buffer.Length)
            {
                ThrowInvalidOperationException();
            }

            this.bytesWritten += count;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            var requiredCapacity = this.bytesWritten + sizeHint;
            var currentBufferLength = this.buffer.Length;
            if (requiredCapacity >= currentBufferLength)
            {
                var newSize = Math.Max(currentBufferLength * 2, requiredCapacity);
                var newBuffer = ArrayPoolBufferWriter.arrayPool.Rent(newSize);
                var bufferSpan = this.buffer.AsSpan();
                var newBufferSpan = newBuffer.AsSpan();
                Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(newBufferSpan), ref MemoryMarshal.GetReference(bufferSpan), (uint)this.bytesWritten);
                ArrayPoolBufferWriter.arrayPool.Return(this.buffer);
                this.buffer = newBuffer;
            }

            return this.buffer.AsMemory(this.bytesWritten);
        }

        /// <summary>
        /// Returns the buffer as an array of <see cref="T:byte[]" />
        /// </summary>
        /// <returns>The buffer as a byte array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ToArray()
        {
            var bufferSpan = this.buffer.AsSpan(0, this.bytesWritten);
            var result = new byte[this.bytesWritten];
            var resultSpan = result.AsSpan();
            Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(resultSpan), ref MemoryMarshal.GetReference(bufferSpan), (uint)this.bytesWritten);
            return result;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return this.GetMemory(sizeHint).Span;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInvalidOperationException()
        {
            throw new InvalidOperationException("Cannot advance past the end of the buffer.");
        }

        /// <summary>
        /// Returns the buffer to the pool.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    ArrayPoolBufferWriter.arrayPool.Return(this.buffer);
                }

                this.disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}