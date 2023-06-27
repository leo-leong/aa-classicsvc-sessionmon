using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace aa_classicsvc_sessionmon
{
    using DWORD = System.UInt32;
    using LONG = System.Int32;

    internal enum TOKEN_INFORMATION_CLASS
    {
        TokenUser = 1,
        TokenGroups,
        TokenPrivileges,
        TokenOwner,
        TokenPrimaryGroup,
        TokenDefaultDacl,
        TokenSource,
        TokenType,
        TokenImpersonationLevel,
        TokenStatistics,
        TokenRestrictedSids,
        TokenSessionId,
        TokenGroupsAndPrivileges,
        TokenSessionReference,
        TokenSandBoxInert,
        TokenAuditPolicy,
        TokenOrigin
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LUID
    {
        internal DWORD LowPart;
        internal LONG HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TOKEN_STATISTICS
    {
        internal LUID TokenId;
        internal LUID AuthenticationId;
    }

    internal class NativeAPI
    {
        [DllImport("Advapi32.DLL", EntryPoint = "GetTokenInformation", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool GetTokenInformation(IntPtr TokenHandle, Int32 TokenInformationClass, IntPtr TokenInformation, Int32 TokenInformationLength, out Int32 ReturnLength);

        [DllImport("wtsapi32.dll", EntryPoint = "WTSQueryUserToken", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool WTSQueryUserToken(UInt32 SessionId, out IntPtr Token);

        [DllImport("Kernel32.DLL", EntryPoint = "CloseHandle", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool CloseHandle(IntPtr Handle);

    }
}
