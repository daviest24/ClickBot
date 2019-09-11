using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClickBot
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		System.Windows.Threading.DispatcherTimer screenCoordsTimer = new System.Windows.Threading.DispatcherTimer();

		System.Drawing.Point oldpoint = new System.Drawing.Point(0, 0);
		System.Drawing.Point point = new System.Drawing.Point(0, 0);

		private static Bitmap imgCapture = null;

		private static int offsetWidth = 1200;
		private static  int offsetHeight = 800;

		private static Rect scanArea = new Rect(0 + (offsetWidth / 2), 0 + (offsetHeight / 2), 1920 - offsetWidth, 1080 - offsetHeight);

		//private static System.Drawing.Color colour = System.Drawing.Color.FromArgb(70, 47, 38);
		private static System.Drawing.Color colourCheck = System.Drawing.Color.FromArgb(0, 132, 125);


		System.Windows.Threading.DispatcherTimer loopTimer = new System.Windows.Threading.DispatcherTimer();
		private static bool isStarted = false;
		private static bool inLoop = false;
		private static bool isScanning = false;
		private void Start()
		{
			loopTimer.Tick += LoopTimer_Tick;
			loopTimer.Interval = new TimeSpan(0, 0, 30);

			while (isStarted)
			{
				//KeyboardController.Press7Key();

				loopTimer.Start();
				inLoop = true;

				while (inLoop && isStarted)
				{
					isScanning = true;

					while (isScanning && inLoop && isStarted)
					{
						if (ObjectMoved())
						{
							MouseController.SetCursorPosition(new MouseController.MousePoint(point.X + (offsetWidth / 2), point.Y + (offsetHeight / 2)));
							MouseController.MouseEvent(MouseController.MouseEventFlags.RightDown | MouseController.MouseEventFlags.RightUp, point.X + (offsetWidth / 2), point.Y + (offsetHeight / 2));
							isScanning = false;
							inLoop = false;
						}
					}
				}
			}

			if (loopTimer.IsEnabled) loopTimer.Stop();

			return;
		}

		private bool ObjectMoved()
		{
			Bitmap bitmap = null;

			try
			{
				oldpoint = point;

				bitmap = ScreenController.Capture(scanArea, CaptureMode.Screen);

				point = CalculateCenterOfPoints(FindColourInImage(bitmap));

				if ((point.X == 0 && point.Y == 0) || (oldpoint.X == 0 && oldpoint.Y == 0))
					return false;

				DrawPoint(point.X + (offsetWidth / 2), point.Y + (offsetHeight / 2), 6);

				// Calculate the difference between the 
				double yDiff = point.Y - oldpoint.Y;
				if (point.Y < point.X && yDiff > 4)
				{
					return true;
				}
				return false;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				bitmap.Dispose();
			}	
		}

		private void LoopTimer_Tick(object sender, EventArgs e)
		{
			inLoop = false;
		}

		private void btnStart_Click(object sender, RoutedEventArgs e)
		{
			if (isStarted == false)
			{
				isStarted = true;
				new Task(Start).Start();
				lblRunning.Content = "Running...";
			}
			else
			{
				isStarted = false;
				lblRunning.Content = "Stopped...";
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			colourActive.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)colourCheck.R, (byte)colourCheck.G, (byte)colourCheck.B));

			screenCoordsTimer.Tick += DispatcherTimer_Tick;
			screenCoordsTimer.Interval = new TimeSpan(0, 0, 1);
			screenCoordsTimer.Start();
		}

		private void ImgArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Bitmap bitmap = ScreenController.Capture(new Rect(0, 0, 1920, 1080), CaptureMode.Window);

			var mousePos = MouseController.GetCursorPosition();

			System.Drawing.Color currPixel = bitmap.GetPixel(mousePos.X, mousePos.Y);

			colourCheck = currPixel;

			colourActive.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)currPixel.R, (byte)currPixel.G, (byte)currPixel.B));
		}

		private void ImgArea_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			Bitmap bitmap = ScreenController.Capture(new Rect(0, 0, 1920, 1080), CaptureMode.Window);

			var mousePos = MouseController.GetCursorPosition();

			System.Drawing.Color currPixel = bitmap.GetPixel(mousePos.X, mousePos.Y);

			colourInactive.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)currPixel.R, (byte)currPixel.G, (byte)currPixel.B));
		}

		private void DispatcherTimer_Tick(object sender, EventArgs e)
		{
			var point = MouseController.GetCursorPosition();

			lblPoint.Content = $"X={point.X} Y={point.Y}";
		}

		private List<System.Drawing.Point> FindColourInImage(Bitmap bitmap)
		{
			BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);  // make sure you check the pixel format as you will be looking directly at memory

			List<System.Drawing.Point> points = new List<System.Drawing.Point>();
			unsafe
			{
				// example assumes 24bpp image.  You need to verify your pixel depth
				// loop by row for better data locality
				for (int y = 0; y < data.Height; ++y)
				{
					byte* pRow = (byte*)data.Scan0 + y * data.Stride;
					for (int x = 0; x < data.Width; ++x)
					{
						// windows stores images in BGR pixel order
						byte r = pRow[2];
						byte g = pRow[1];
						byte b = pRow[0];

						if (IsColorSimilar(System.Drawing.Color.FromArgb(r, g, b), colourCheck, 10))
						{
							points.Add(new System.Drawing.Point(x, y));
						}

						// next pixel in the row
						pRow += 3;
					}
				}
			}

			bitmap.UnlockBits(data);

			////Iterate whole bitmap to find the picked colour
			//List<System.Drawing.Point> points = new List<System.Drawing.Point>();
			//for (int y = 0; y < bitmap.Height; y++)
			//{
			//	for (int x = 0; x < bitmap.Width; x++)
			//	{
			//		//Get the colour at each pixel
			//		System.Drawing.Color currPixel = bitmap.GetPixel(x, y);

			//		//Compare Pixel's colour ARGB property with the picked colour's ARGB property 
			//		//if (currPixel.ToArgb() == colour.ToArgb())
			//		if (AreColorsSimilar(currPixel, colour, 10))
			//		{
			//			points.Add(new System.Drawing.Point(x, y));
			//		}
			//	}
			//}

			return points;
		}

		public bool IsColorSimilar(System.Drawing.Color c1, System.Drawing.Color c2, int tolerance)
		{
			return Math.Abs(c1.R - c2.R) < tolerance &&
				   Math.Abs(c1.G - c2.G) < tolerance &&
				   Math.Abs(c1.B - c2.B) < tolerance;
		}

		private System.Drawing.Point CalculateCenterOfPoints(List<System.Drawing.Point> points)
		{
			if (points.Count <= 0) return new System.Drawing.Point(0, 0);

			int totalX = 0, totalY = 0;
			foreach (System.Drawing.Point p in points)
			{
				totalX += p.X;
				totalY += p.Y;
			}
			int centerX = totalX / points.Count;
			int centerY = totalY / points.Count;

			return new System.Drawing.Point(centerX, centerY);
		}

		private void DrawPoint(int x, int y, int size)
		{
			Graphics g = Graphics.FromHwnd(IntPtr.Zero);

			System.Drawing.Rectangle rect = new System.Drawing.Rectangle((int)x - (size / 2), (int)y - (size / 2), size, size);

			System.Drawing.Brush brsh = new SolidBrush(System.Drawing.Color.Red);

			g.FillEllipse(brsh, rect);

			brsh.Dispose();
			g.Dispose();
		}

		private void BtnDrawRect_Click(object sender, RoutedEventArgs e)
		{
			Graphics g = Graphics.FromHwnd(IntPtr.Zero);

			System.Drawing.Rectangle rect = new System.Drawing.Rectangle((int)scanArea.X, (int)scanArea.Y, (int)scanArea.Width, (int)scanArea.Height);

			System.Drawing.Brush brsh = new SolidBrush(System.Drawing.Color.Red);
			System.Drawing.Pen pen = new System.Drawing.Pen(brsh);

			g.DrawRectangle(pen, rect);

			brsh.Dispose();
			g.Dispose();
		}

		private void BtnTakeImage_Click(object sender, RoutedEventArgs e)
		{
			imgCapture = ScreenController.Capture(scanArea, CaptureMode.Screen);

			imgArea.Source = CreateBitmapSourceFromGdiBitmap(imgCapture);
		}


		public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
		{
			if (bitmap == null)
				throw new ArgumentNullException("bitmap");

			var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

			var bitmapData = bitmap.LockBits(
				rect,
				ImageLockMode.ReadWrite,
				System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			try
			{
				var size = (rect.Width * rect.Height) * 4;

				return BitmapSource.Create(
					bitmap.Width,
					bitmap.Height,
					bitmap.HorizontalResolution,
					bitmap.VerticalResolution,
					PixelFormats.Bgra32,
					null,
					bitmapData.Scan0,
					size,
					bitmapData.Stride);
			}
			finally
			{
				bitmap.UnlockBits(bitmapData);
			}
		}

		bool dropperEnabled = false;
		private void BtnEyeDropper_Click(object sender, RoutedEventArgs e)
		{
			if (dropperEnabled == false) dropperEnabled = true;
			else dropperEnabled = false;
		}

		private System.Drawing.Bitmap BitmapFromSource(BitmapSource bitmapsource)
		{
			System.Drawing.Bitmap bitmap;
			using (MemoryStream outStream = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();

				enc.Frames.Add(BitmapFrame.Create(bitmapsource));
				enc.Save(outStream);
				bitmap = new System.Drawing.Bitmap(outStream);
			}
			return bitmap;
		}
	}
}

//MouseController.SetCursorPosition(new MouseController.MousePoint(261, 85));
//MouseController.MouseEvent(MouseController.MouseEventFlags.LeftDown | MouseController.MouseEventFlags.LeftUp, 2299, 111);

//int offsetWidth = 0;
//int offsetHeight = 0;
//Rect rec = new Rect(0 + (offsetWidth / 2), 0 + (offsetHeight / 2), 1920 - offsetWidth, 1080 - offsetHeight);

//ScreenController.CaptureAndSave(rec, @"C:\Users\tdavies\Documents\GitHub\ClickBot\Test.png", CaptureMode.Window, ImageFormat.Png);
//var bitmap = ScreenController.Capture(rec, CaptureMode.Screen);

////Iterate whole bitmap to findout the picked color
//List<System.Drawing.Point> points = new List<System.Drawing.Point>();
//for (int y = 0; y < bitmap.Height; y++)
//{
//	for (int x = 0; x < bitmap.Width; x++)
//	{
//		//Get the color at each pixel
//		System.Drawing.Color now_color = bitmap.GetPixel(x, y);

//		//Compare Pixel's Color ARGB property with the picked color's ARGB property 
//		System.Drawing.Color color = new System.Drawing.Color();
//		color = System.Drawing.Color.FromArgb(253, 203, 120);

//		if (now_color.ToArgb() == color.ToArgb())
//		{
//			points.Add(new System.Drawing.Point(x, y));
//		}
//	}
//}

//System.Drawing.Point center = CalculateCenterOfPoints(points);

//MouseController.SetCursorPosition(new MouseController.MousePoint(center.X, center.Y));

//BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);  // make sure you check the pixel format as you will be looking directly at memory

//List<Point> points = new List<Point>();
//unsafe
//{
//	// example assumes 24bpp image.  You need to verify your pixel depth
//	// loop by row for better data locality
//	for (int y = 0; y < data.Height; ++y)
//	{
//		byte* pRow = (byte*)data.Scan0 + y * data.Stride;
//		for (int x = 0; x < data.Width; ++x)
//		{
//			// windows stores images in BGR pixel order
//			byte r = pRow[2];
//			byte g = pRow[1];
//			byte b = pRow[0];

//			if(g == 204)
//			{
//				points.Add(new Point(x, y));
//			}

//			if(r == 255 && g == 204 && g == 0)
//			{
//				//points.Add(new Point(x, y));
//			}

//			// next pixel in the row
//			pRow += 3;
//		}
//	}
//}

//bitmap.UnlockBits(data);
