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

namespace ClickBot
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
		System.Windows.Threading.DispatcherTimer imageTimer = new System.Windows.Threading.DispatcherTimer();

		System.Drawing.Point oldpoint = new System.Drawing.Point(0, 0);
		System.Drawing.Point point = new System.Drawing.Point(0, 0);


		private static Bitmap image = null;

		private static int offsetWidth = 1200;
		private static  int offsetHeight = 800;

		private static Rect rec = new Rect(0 + (offsetWidth / 2), 0 + (offsetHeight / 2), 1920 - offsetWidth, 1080 - offsetHeight);

		private static System.Drawing.Color colour = System.Drawing.Color.FromArgb(70, 47, 38);


		public MainWindow()
		{
			InitializeComponent();

			lblRunning.Content = "Stopped...";

			colourActive.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)92, (byte)48, (byte)50));

			dispatcherTimer.Tick += DispatcherTimer_Tick;
			dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
			dispatcherTimer.Start();

			imageTimer.Tick += ImageTimer_Tick;
			imageTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
		    //imageTimer.Start();
		}

		bool started = false;
		private void btnStartFishing_Click(object sender, RoutedEventArgs e)
		{
			if (started == false)
			{
				started = true;

				StartFishing();

				lblRunning.Content = "Running...";
			}
			else
			{
				started = false;

				imageTimer.Stop();

				lblRunning.Content = "Stopped...";
			}
		}

		private void StartFishing()
		{
			KeyboardController.CastRod();

			Thread.Sleep(3000);

			imageTimer.Start();
		}

		private void ImgArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Bitmap bitmap = ScreenController.Capture(new Rect(0, 0, 1920, 1080), CaptureMode.Window);

			var mousePos = MouseController.GetCursorPosition();

			System.Drawing.Color currPixel = bitmap.GetPixel(mousePos.X, mousePos.Y);

			colour = currPixel;

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

		private void ImageTimer_Tick(object sender, EventArgs e)
		{
			oldpoint = point;

			//Bitmap bitmap = new Bitmap(@"C:\Users\Davies\Documents\GitHub\ClickBot\Test.png");
			//ScreenController.CaptureAndSave(rec, @"C:\Users\Davies\Documents\GitHub\ClickBot\Test.png", CaptureMode.Window, ImageFormat.Png);
			var bitmap = ScreenController.Capture(rec, CaptureMode.Screen);

			point = CalculateCenterOfPoints(FindLureInImage(bitmap));

			if ((point.X == 0 && point.Y == 0) || (oldpoint.X == 0 && oldpoint.Y == 0))
				return;
			
			// Calculate the difference between the 
			double yDiff = point.Y - oldpoint.Y;
			if (point.Y < point.X && yDiff > 4)
			{
				MouseController.SetCursorPosition(new MouseController.MousePoint(point.X + (offsetWidth / 2), point.Y + (offsetHeight / 2)));
				MouseController.MouseEvent(MouseController.MouseEventFlags.RightDown | MouseController.MouseEventFlags.RightUp, point.X + (offsetWidth / 2), point.Y + (offsetHeight / 2));
				//imageTimer.Stop();
			}	
		}

		private List<System.Drawing.Point> FindLureInImage(Bitmap bitmap)
		{
			//Iterate whole bitmap to find the picked colour
			List<System.Drawing.Point> points = new List<System.Drawing.Point>();
			for (int y = 0; y < bitmap.Height; y++)
			{
				for (int x = 0; x < bitmap.Width; x++)
				{
					//Get the colour at each pixel
					System.Drawing.Color currPixel = bitmap.GetPixel(x, y);

					//Compare Pixel's colour ARGB property with the picked colour's ARGB property 
					//if (currPixel.ToArgb() == colour.ToArgb())
					if (AreColorsSimilar(currPixel, colour, 10))
					{
						points.Add(new System.Drawing.Point(x, y));
					}
				}
			}
			return points;
		}

		public bool AreColorsSimilar(System.Drawing.Color c1, System.Drawing.Color c2, int tolerance)
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

		private void BtnDrawRect_Click(object sender, RoutedEventArgs e)
		{
			Graphics g = Graphics.FromHwnd(IntPtr.Zero);

			System.Drawing.Rectangle rect = new System.Drawing.Rectangle((int)rec.X, (int)rec.Y, (int)rec.Width, (int)rec.Height);

			System.Drawing.Brush brsh = new SolidBrush(System.Drawing.Color.Red);
			System.Drawing.Pen pen = new System.Drawing.Pen(brsh);

			g.DrawRectangle(pen, rect);

			brsh.Dispose();
			g.Dispose();
		}

		private void BtnTakeImage_Click(object sender, RoutedEventArgs e)
		{
			image = ScreenController.Capture(rec, CaptureMode.Screen);

			imgArea.Source = CreateBitmapSourceFromGdiBitmap(image);
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
