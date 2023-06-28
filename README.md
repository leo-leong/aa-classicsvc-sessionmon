# Alternate solution to SSO integration without using NPLogonNotify

C# sample code to demo:

1) Create a classic .NET windows service to monitor user logons.
2) Impersonate the user's security context.
3) Acquire user's credentials for single sign-on integration.

This sample extends the basic tutorial on creating a classic windows service app.<br>
https://learn.microsoft.com/en-us/dotnet/framework/windows-services/walkthrough-creating-a-windows-service-application-in-the-component-designer
