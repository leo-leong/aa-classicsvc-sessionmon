using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
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
            switch (changeDescription.Reason)
            {
                case SessionChangeReason.SessionLogon:
                    eventLog1.WriteEntry("SessionLogon, Session ID: " +
                        changeDescription.SessionId.ToString());

                    IntPtr token = IntPtr.Zero;
                    if (!WTSQueryUserToken(Convert.ToUInt32(changeDescription.SessionId),
                        out token))
                    {
                        eventLog1.WriteEntry("WTSQueryUserToken failed with: " +
                            Marshal.GetLastWin32Error().ToString());
                        break;
                    }

                    uint tokeninfolength = 0;
                    // first call on GetTokenInformation to get the length of the token info
                    GetTokenInformation(token, TOKEN_INFORMATION_CLASS.TokenUser, IntPtr.Zero, tokeninfolength, out tokeninfolength);

                    IntPtr TokenInformation = Marshal.AllocHGlobal((IntPtr)tokeninfolength);
                    if (GetTokenInformation(token, TOKEN_INFORMATION_CLASS.TokenUser, TokenInformation, tokeninfolength, out tokeninfolength))
                    {
                        eventLog1.WriteEntry("WTSQueryUserToken failed with: " +
                            Marshal.GetLastWin32Error().ToString());
                        break;
                    }

                    //TODO: get user LUID within token statistics

                    //TODO: acquire user credentials

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
