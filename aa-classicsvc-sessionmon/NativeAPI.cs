//Copyright(c) Microsoft Corporation.

//MIT License

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
//documentation files (the "Software"), to deal in the Software without restriction,
//including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
//sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
//subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
//THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
//WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
//THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Microsoft.Win32.SafeHandles;
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
        internal static extern bool GetTokenInformation(SafeAccessTokenHandle TokenHandle, Int32 TokenInformationClass, IntPtr TokenInformation, Int32 TokenInformationLength, out Int32 ReturnLength);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        internal static extern bool WTSQueryUserToken(UInt32 SessionId, out SafeAccessTokenHandle Token);

        [DllImport("Kernel32.DLL", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr Handle);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool ImpersonateLoggedOnUser(SafeAccessTokenHandle TokenHandle);

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
            out CredHandle CredentialHandle,
            out long TimeStamp);
    }
}
