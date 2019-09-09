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

namespace ClickBot
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
			dispatcherTimer.Tick += dispatcherTimer_Tick;
			dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
			dispatcherTimer.Start();
		}

		private void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			var point = MouseController.GetCursorPosition();

			lblPoint.Content = $"X={point.X} Y={point.Y}";
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			MouseController.SetCursorPosition(new MouseController.MousePoint(261, 85));
			MouseController.MouseEvent(MouseController.MouseEventFlags.LeftDown | MouseController.MouseEventFlags.LeftUp, 2299, 111);

			ScreenController.CaptureAndSave(@"D:\SSM INTERNAL DEVELOPMENT\OTHER\ClickBot\Test.png", CaptureMode.Screen, ImageFormat.Png);
			var bitmap = ScreenController.Capture(CaptureMode.Screen);

			//Iterate whole bitmap to findout the picked color
			for (int i = 0; i < bitmap.Height; i++)
			{
				for (int j = 0; j < bitmap.Width; j++)
				{
					//Get the color at each pixel
					System.Drawing.Color now_color = bitmap.GetPixel(j, i);

					//Compare Pixel's Color ARGB property with the picked color's ARGB property 
					System.Drawing.Color color = new System.Drawing.Color();
					color = System.Drawing.Color.FromArgb(233, 214, 60);

					if (now_color.ToArgb() == color.ToArgb())
					{
						MessageBox.Show("Color Found!");
						break;
					}
				}
			}

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
			}
	}
}
