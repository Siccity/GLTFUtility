using System;
using System.IO;
using UnityEngine;

// This is a modified version of the script from: https://jacksondunstan.com/articles/3568
/// <summary> Much faster than BinaryReader </summary>
public class BufferedBinaryReader : IDisposable {
	private readonly Stream stream;
	private readonly byte[] buffer;
	private readonly int bufferSize;
	private int bufferOffset;
	private int bufferedBytes;

	public long Position { get { return stream.Position + bufferOffset; } set { stream.Position = value; bufferedBytes = 0; bufferOffset = 0; } }

	public BufferedBinaryReader(Stream stream, int bufferSize) {
		this.stream = stream;
		this.bufferSize = bufferSize;
		buffer = new byte[bufferSize];
		bufferOffset = 0;
		bufferedBytes = 0;
	}

	private void FillBuffer(int byteCount) {
		int unreadBytes = bufferedBytes - bufferOffset;

		if (unreadBytes < byteCount) {
			// If not enough bytes left in buffer
			if (unreadBytes != 0) {
				// If buffer still has unread bytes, move them to start of buffer
				Buffer.BlockCopy(buffer, bufferOffset, buffer, 0, unreadBytes);
			}
			bufferedBytes = stream.Read(buffer, unreadBytes, bufferSize - unreadBytes) + unreadBytes;
			bufferOffset = 0;
		}
	}

	public byte ReadByte() {
		FillBuffer(1);
		return buffer[bufferOffset++];
	}

	public sbyte ReadSByte() {
		FillBuffer(1);
		return (sbyte) buffer[bufferOffset++];
	}

	public ushort ReadUInt16() {
		FillBuffer(sizeof(ushort));
		var val = BitConverter.ToUInt16(buffer, bufferOffset);
		bufferOffset += sizeof(ushort);
		return val;
	}

	public short ReadInt16() {
		FillBuffer(sizeof(short));
		var val = BitConverter.ToInt16(buffer, bufferOffset);
		bufferOffset += sizeof(short);
		return val;
	}

	public uint ReadUInt32() {
		FillBuffer(sizeof(uint));
		var val = BitConverter.ToUInt32(buffer, bufferOffset);
		bufferOffset += sizeof(uint);
		return val;
	}

	public int ReadInt32() {
		FillBuffer(sizeof(int));
		var val = BitConverter.ToInt32(buffer, bufferOffset);
		bufferOffset += sizeof(int);
		return val;
	}

	public float ReadSingle() {
		FillBuffer(sizeof(float));
		var val = BitConverter.ToSingle(buffer, bufferOffset);
		bufferOffset += sizeof(float);
		return val;
	}

	public void Dispose() {
		stream.Close();
	}
}