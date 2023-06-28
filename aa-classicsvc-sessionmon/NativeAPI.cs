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

    [StructLayout(LayoutKind.Sequential)]
    internal struct CredHandle
    {
        internal IntPtr HandleHi;
        internal IntPtr HandleLo;
    }

    internal class NativeAPI
    {
        [DllImport("Advapi32.DLL", SetLastError = true)]
        internal static extern bool GetTokenInformation(IntPtr TokenHandle, Int32 TokenInformationClass, IntPtr TokenInformation, Int32 TokenInformationLength, out Int32 ReturnLength);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        internal static extern bool WTSQueryUserToken(UInt32 SessionId, out IntPtr Token);

        [DllImport("Kernel32.DLL", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr Handle);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool ImpersonateLoggedOnUser(IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool RevertToSelf();

        [DllImport("Secur32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int AcquireCredentialsHandle(
            string Principal,
            string Package,
            int CredentialUse,
            IntPtr LogonID,
            IntPtr AuthData,
            IntPtr KeyCallback,
            IntPtr KeyArgument,
            ref CredHandle CredentialHandle,
            out long TimeStamp);
    }
}
