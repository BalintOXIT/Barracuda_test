using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Box
{
    public Int32 x;
    public Int32 y;
    public Int32 w;
    public Int32 h;
    public Int32 refcount;

    public override string ToString()
    {
        return "Position: x=" + x.ToString() + ", y="+y.ToString() + " Size: w="+w.ToString() + ", h="+h.ToString();
    }
}