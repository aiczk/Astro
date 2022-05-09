using System;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace Astro.Helper
{
    public static class MarshalHelper
    {
        public static float ReadFloat(IntPtr ptr) => BitConverter.ToSingle(BitConverter.GetBytes(Marshal.ReadInt32(ptr, 0)));
        public static uint ReadUInt(IntPtr ptr, int ofs) => (uint) Marshal.ReadInt32(ptr, ofs);
    }
}