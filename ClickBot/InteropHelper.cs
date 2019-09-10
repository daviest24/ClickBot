﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClickBot
{
	public class InteropHelper
	{
		[DllImport("user32.dll")]
		public static extern IntPtr GetDesktopWindow();

		// http://msdn.microsoft.com/en-us/library/dd144871(VS.85).aspx
		[DllImport("user32.dll")]
		public static extern IntPtr GetDC(IntPtr hwnd);

		// http://msdn.microsoft.com/en-us/library/dd183370(VS.85).aspx
		[DllImport("gdi32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, Int32 dwRop);

		// http://msdn.microsoft.com/en-us/library/dd183488(VS.85).aspx
		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

		// http://msdn.microsoft.com/en-us/library/dd183489(VS.85).aspx
		[DllImport("gdi32.dll", SetLastError = true)]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		// http://msdn.microsoft.com/en-us/library/dd162957(VS.85).aspx
		[DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

		// http://msdn.microsoft.com/en-us/library/dd183539(VS.85).aspx
		[DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		// http://msdn.microsoft.com/en-us/library/dd162920(VS.85).aspx
		[DllImport("user32.dll")]
		public static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

		[DllImport("user32.dll")]
		public static extern IntPtr LoadCursorFromFile(string lpFileName);

		[DllImport("user32.dll")]
		public static extern IntPtr SetCursor(IntPtr hCursor);

		[DllImport("user32.dll")]
		public static extern bool SetSystemCursor(IntPtr hcur, uint id);

		[DllImport("user32.dll")]
		public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

		public const int SPIF_UPDATEINIFILE = 0x01;

		public const int SPIF_SENDCHANGE = 0x02;

		public const int SPI_SETCURSORS = 0x0057;

		[DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
		public static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);


		public const uint OCR_NORMAL = 32512;

		public static Cursor ColoredCursor;

		public const int SRCCOPY = 0xCC0020;

		public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

	}

	enum IDC_STANDARD_CURSORS
	{
		IDC_ARROW = 32512,
		IDC_IBEAM = 32513,
		IDC_WAIT = 32514,
		IDC_CROSS = 32515,
		IDC_UPARROW = 32516,
		IDC_SIZE = 32640,
		IDC_ICON = 32641,
		IDC_SIZENWSE = 32642,
		IDC_SIZENESW = 32643,
		IDC_SIZEWE = 32644,
		IDC_SIZENS = 32645,
		IDC_SIZEALL = 32646,
		IDC_NO = 32648,
		IDC_HAND = 32649,
		IDC_APPSTARTING = 32650,
		IDC_HELP = 32651
	}
}
