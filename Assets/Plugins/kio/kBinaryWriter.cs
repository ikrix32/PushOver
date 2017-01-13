using UnityEngine;
using System.Collections;
using System.IO;

public class kBinaryWriter : BinaryWriter
{
	private bool  bigEndianStream = false;
	private const int byteSize = sizeof(byte) * 8;
	private const int shortSize= sizeof(ushort) * 8;
	private const int intSize  = sizeof(uint) * 8;
	
	
    public kBinaryWriter(Stream stream,bool bigEndianStream):base(stream){
		this.bigEndianStream = bigEndianStream;
	}
	
	public kBinaryWriter(Stream s, System.Text.Encoding e,bool bigEndianStream): base(s, e){
		this.bigEndianStream = bigEndianStream;
	}
	
	public override void Write(ushort val)
	{
		byte a = (byte)( val & 0xFF);
		byte b = (byte)( val >> byteSize);
		
		Write(bigEndianStream ? a : b );
		Write(bigEndianStream ? b : a);
    }

    public override void Write(uint val)
    {
		ushort a = (ushort)(val & 0xFFFF);
		ushort b = (ushort)(val >> shortSize);
		
		Write(bigEndianStream ? a : b );
		Write(bigEndianStream ? b : a);
    }

    public override void Write(ulong val){
		uint a = (uint)(val & 0xFFFFFFFF);
		uint b = (uint)(val >> intSize);
		
		Write(bigEndianStream ? a : b );
		Write(bigEndianStream ? b : a);
	}

    public override void Write(short val){
		Write((ushort)val);
	}

    public override void Write(int val)
    {
		Write((uint)val); 
	}

    public override void Write(long val)
	{
		Write((ulong)val);
	}
	
	public override void Write(float val){
		uint buf;
		buf = System.BitConverter.ToUInt32(System.BitConverter.GetBytes(val),0);
		
		Write(buf);
	}
	
	public override void Write(string val)
	{
		base.Write(val);
	}
}
