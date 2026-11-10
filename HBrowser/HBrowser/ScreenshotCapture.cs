// HBrowser, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Plugin.ScreenshotCapture
using HBrowser;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

internal sealed class ScreenshotCapture
{
	private class User32
	{
		public struct Rect
		{
			public int left;

			public int top;

			public int right;

			public int bottom;
		}

		[DllImport("user32.dll")]
		public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

		[DllImport("user32.dll")]
		public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);
	}

	public static Bitmap CaptureWindow(IntPtr handle)
	{
		User32.Rect rect = default(User32.Rect);
		User32.GetWindowRect(handle, ref rect);
		int num = rect.right - rect.left;
		int num2 = rect.bottom - rect.top;
		int x = Helper.X;
		int y = Helper.Y;
		Bitmap bitmap = ((!Helper.Resize) ? new Bitmap(num + 1, num2 + 1) : new Bitmap(x + 1, y + 1));
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			IntPtr hdc = graphics.GetHdc();
			User32.PrintWindow(handle, hdc, 2u);
		}
		return bitmap;
	}
}
