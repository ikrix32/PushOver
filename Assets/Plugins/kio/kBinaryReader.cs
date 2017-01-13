using UnityEngine;
using System.Collections;
using System.IO;

public class kBinaryReader : BinaryReader
{
	private bool  bigEndianStream = false;
	private const int byteSize = sizeof(byte) * 8;
	private const int shortSize= sizeof(ushort) * 8;
	private const int intSize  = sizeof(uint) * 8;
	
	
    public kBinaryReader(Stream stream,bool bigEndianStream):base(stream){
		this.bigEndianStream = bigEndianStream;
	}
	
	public kBinaryReader(Stream s, System.Text.Encoding e,bool bigEndianStream): base(s, e){
		this.bigEndianStream = bigEndianStream;
	}
	
	public override ushort ReadUInt16()
	{
		ushort a = ReadByte();
		ushort b = ReadByte();
	
		return (ushort)(bigEndianStream? a|(b<< byteSize) : (a<<byteSize)|b);
    }

    public override uint ReadUInt32()
    {
		uint a = ReadUInt16();
		uint b = ReadUInt16();
		
		return (bigEndianStream? a|(b<<shortSize) : (a<<shortSize)|b);
    }

    public override ulong ReadUInt64(){
		ulong a = ReadUInt32();
		ulong b = ReadUInt32();
		
		return (ulong)(bigEndianStream? a|(b<<intSize) : (a<<intSize)|b);
	}

    public override short ReadInt16(){
		return (short)ReadUInt16();
	}

    public override int ReadInt32()
    {
		return (int) ReadUInt32(); 
	}

    public override long ReadInt64()
	{
		return (long)ReadUInt64();
	}
	
	public virtual float ReadFloat(){
		byte[] fBuff = new byte[sizeof(float)];
		for(int i = 0;i < sizeof(float);i++){
			int index = bigEndianStream? i : sizeof(float) - 1 - i;
			fBuff[index] = ReadByte();
		}
		
		return System.BitConverter.ToSingle(fBuff,0);
	}
	
	public override string ReadString()
	{
		base.ReadString();
		return base.ReadString();
	}
}
