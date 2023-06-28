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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace aa_classicsvc_sessionmon
{
    public partial class SampleSessionLogonService : ServiceBase
    {
        EventLog eventLog1;

        public SampleSessionLogonService()
        {
            InitializeComponent();
            eventLog1 = new EventLog();

            // this is required for OnSessionChange method to be called for handling terminal services session changes events
            // https://learn.microsoft.com/en-us/dotnet/api/system.serviceprocess.servicebase.canhandlesessionchangeevent?view=dotnet-plat-ext-7.0
            this.CanHandleSessionChangeEvent = true;

            // this is for writing to a custom log instead of the default Application log
            // https://learn.microsoft.com/en-us/dotnet/framework/windows-services/how-to-log-information-about-services?source=recommendations
            this.AutoLog = false;

            if (!EventLog.SourceExists("SampleSessionLogonSource"))
            {
                EventLog.CreateEventSource("SampleSessionLogonSource", "SampleSessionLogonLog");
            }
            eventLog1.Source = "SampleSessionLogonSource";
            eventLog1.Log = "SampleSessionLogonLog";
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("SampleSessionLogonService start...");
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("SampleSessionLogonService stop...");
        }

        // Handle a session change notice
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            IntPtr token;
            Int32 tokeninfolength;
            TOKEN_STATISTICS tokenstats = new TOKEN_STATISTICS();
            const int SECPKG_CRED_OUTBOUND = 2;
            CredHandle credhandle = new CredHandle();
            long timestamp;

            switch (changeDescription.Reason)
            {
                case SessionChangeReason.SessionLogon:
                    eventLog1.WriteEntry("SessionLogon, Session ID: " +
                        changeDescription.SessionId.ToString());

                    if (NativeAPI.WTSQueryUserToken(Convert.ToUInt32(changeDescription.SessionId),
                        out token))
                    {
                        // first call on GetTokenInformation to get the length of the token info
                        NativeAPI.GetTokenInformation(token, (Int32)TOKEN_INFORMATION_CLASS.TokenStatistics, IntPtr.Zero, 0, out tokeninfolength);

                        IntPtr tokeninfo = Marshal.AllocHGlobal((IntPtr)tokeninfolength);
                        if (NativeAPI.GetTokenInformation(token, (Int32)TOKEN_INFORMATION_CLASS.TokenStatistics, tokeninfo, tokeninfolength, out tokeninfolength))
                        {
                            // get user LUID within token statistics
                            tokenstats = ((TOKEN_STATISTICS)(Marshal.PtrToStructure(tokeninfo, tokenstats.GetType())));
                            eventLog1.WriteEntry("TokenId: " + tokenstats.TokenId.LowPart +
                                "\tAuthenticationId: " + tokenstats.AuthenticationId.LowPart
                                );

                            // acquire user credentials
                            if (NativeAPI.ImpersonateLoggedOnUser(token))
                            {
                                NativeAPI.AcquireCredentialsHandle(null, "kerberos", SECPKG_CRED_OUTBOUND, 
                                    IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 
                                    ref credhandle, out timestamp);
                                eventLog1.WriteEntry("Call to AcquireCredentialsHandle succeeded.");
                            }
                            else
                            {
                                eventLog1.WriteEntry("ImpersonateLoggedOnUser failed with: " +
                                    Marshal.GetLastWin32Error().ToString()
                                    );
                            }

                            // revert thread's security context
                            NativeAPI.RevertToSelf();
                        }
                        else
                        {
                            eventLog1.WriteEntry("GetTokenInformation failed with: " +
                                Marshal.GetLastWin32Error().ToString()
                                );
                        }

                        // release the handle to user's token
                        NativeAPI.CloseHandle(token);
                    }
                    else
                    {
                        eventLog1.WriteEntry("WTSQueryUserToken failed with: " +
                            Marshal.GetLastWin32Error().ToString());
                    }

                    break;

                case SessionChangeReason.SessionLogoff:
                    eventLog1.WriteEntry("SessionLogoff, Session ID: " +
                        changeDescription.SessionId.ToString()
                        );
                    break;

                default:
                    break;
            }
        }
    }
}
