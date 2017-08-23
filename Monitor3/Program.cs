using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Monitor3
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern uint GetPixel(IntPtr dc, int x, int y);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr window, IntPtr dc);

        /**
         * Mouse functions
         */
        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern long mouse_event(Int32 dwFlags, Int32 dx, Int32 dy, Int32 cButtons, Int32 dwExtraInfo);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern void SetCursorPos(Int32 x, Int32 y);

        public const Int32 MOUSEEVENTF_ABSOLUTE = 0x8000;
        public const Int32 MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const Int32 MOUSEEVENTF_LEFTUP = 0x0004;
        public const Int32 MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public const Int32 MOUSEEVENTF_MIDDLEUP = 0x0040;
        public const Int32 MOUSEEVENTF_MOVE = 0x0001;
        public const Int32 MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const Int32 MOUSEEVENTF_RIGHTUP = 0x0010;

        public static void SendMLeftClick(Int32 x, Int32 y)
        {
            Random random = new Random(DateTime.Now.Millisecond);

            SetCursorPos(x, y);

            Thread.Sleep(random.Next(700, 850));

            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);

            Thread.Sleep(random.Next(110, 140));

            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public static void SendMRightClick(Int32 x, Int32 y)
        {
            Random random = new Random(DateTime.Now.Millisecond);

            SetCursorPos(x, y);

            Thread.Sleep(random.Next(400, 550));

            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);

            Thread.Sleep(random.Next(80, 120));
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        [Flags]
        public enum InputType
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [Flags]
        public enum KeyEventF
        {
            KeyDown = 0x0000,
            ExtendedKey = 0x0001,
            KeyUp = 0x0002,
            Unicode = 0x0004,
            Scancode = 0x0008,
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        public static void SendKey(ushort key, KeyEventF keyEvent)
        {
            Input[] inputs =
            {
                new Input
                {
                    type = (int) InputType.Keyboard,
                    u = new InputUnion
                    {
                        ki = new KeyboardInput
                        {
                            wVk = 0,
                            wScan = key,
                            dwFlags = (uint) (keyEvent | KeyEventF.Scancode),
                            dwExtraInfo = GetMessageExtraInfo()
                        }
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }

        private struct Input
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public readonly MouseInput mi;
            [FieldOffset(0)] public KeyboardInput ki;
            [FieldOffset(0)] public readonly HardwareInput hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MouseInput
        {
            public readonly int dx;
            public readonly int dy;
            public readonly uint mouseData;
            public readonly uint dwFlags;
            public readonly uint time;
            public readonly IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public readonly uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HardwareInput
        {
            public readonly uint uMsg;
            public readonly ushort wParamL;
            public readonly ushort wParamH;
        }

        // Intimacy realted
        private const Boolean INTIMACY_MODE = true;
        private static readonly Point[] INTIMACY_WHEEL_ITEMS = { new Point(1335, 967), new Point(1442, 926), new Point(1549, 909), new Point(1660, 921), new Point(1756, 970) };
        private static readonly Point INTIMACY_WHEEL_LEFT = new Point(1266, 1040);
        private static readonly Point INTIMACY_WHEEL_RIGHT = new Point(1828, 1042);
        private static readonly Point INTIMACY_WHEEL_CONFIRM = new Point(1556, 1043);

        public enum MouseKey
        {
            LEFT,
            RIGHT,
            MIDDLE
        }

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        private const UInt32 WM_KEYDOWN = 0x0100;
        private const int VK_F5 = 0x74;
        private const int VK_SPACEBAR = 0x20;
        private const int DIK_SPACE = 0x39;
        private const int DIK_LEFT = 0xCB;
        private const int DIK_RIGHT = 0xCD;
        private const int DIK_A = 0x1E;
        private const int DIK_D = 0x20;

        private const double KEY_MESSAGE_OFFSET = 7000;
        private static readonly Point POS_FISHING_SIGN = new Point(1385, 105);
        private static readonly Point POS_DETECTION_SIGN = new Point(1385, 155);

        static Feature btnSpace;
        static Feature icFishing;

        //static Dictionary<Point, Color> btnSpaceFeatures;
        //static Dictionary<Point, Color> icFishingFeatures;

        static Form form;

        static Stopwatch sw = new Stopwatch();

        static long getColorTimeSum = 0;
        static int getColorTimeCount = 0;

        static Boolean isKeyPending = false;
        static double lastKeyMessageTimeStamp = ToMillisecondsWith(DateTime.Now);

        static readonly Boolean isBenchmarkMode = false;

        class Feature
        {
            static Stopwatch stopWatch = new Stopwatch();

            public readonly String name;
            public readonly Dictionary<Point, Color> map;
            public int tollerance;
            public int boundingBoxWidth;
            public int boundingBoxHeight;
            public int boundingBoxLineWidth;
            public float matchRateThreshold;
            public Boolean wasOnScreen;
            public int scanWindowSize;

            private readonly Dictionary<Point, Boolean> onScreenMap;

            public Feature(String name)
            {
                this.name = name;
                map = new Dictionary<Point, Color>();
                tollerance = 30;
                boundingBoxWidth = 4;
                boundingBoxHeight = 4;
                boundingBoxLineWidth = 1;
                scanWindowSize = 1;

                matchRateThreshold = 50.0f;

                onScreenMap = new Dictionary<Point, bool>();
            }

            public void DrawBoundingBox(Graphics g)
            {
                // Initialize bounding boxes
                //RectangleF[] boxes;

                if (map.Count == 0)
                {
                    return;
                }
                //else
                //{
                //    boxes = new RectangleF[map.Count];

                //    int index = 0;
                //    foreach (KeyValuePair<Point, Color> feature in map)
                //    {
                //        boxes[index++] = CreateBoundingBox(feature.Key);
                //    }
                //}

                // Prepare
                SolidBrush noMatchBrush = new SolidBrush(Color.Red);
                Pen noMatchPen = new Pen(noMatchBrush, boundingBoxLineWidth);

                SolidBrush matchBrush = new SolidBrush(Color.Blue);
                Pen matchPen = new Pen(matchBrush, boundingBoxLineWidth);

                //Dictionary<Point, RectangleF> boxMap = new Dictionary<Point, RectangleF>();
                foreach (KeyValuePair<Point, Color> feature in map)
                {
                    // Decide the pen
                    Pen pen = noMatchPen;
                    if (onScreenMap.ContainsKey(feature.Key))
                    {
                        if (onScreenMap[feature.Key])
                        {
                            pen = matchPen;
                        }
                        else
                        {
                            pen = noMatchPen;
                        }
                    }

                    // Create box
                    RectangleF rectF = CreateBoundingBox(feature.Key);

                    // Draw
                    g.DrawRectangle(pen, rectF.Left, rectF.Top, rectF.Width, rectF.Height);
                }

                // Draw
                //g.DrawRectangles(noMatchPen, boxes);

                // Release
                noMatchBrush.Dispose();
                noMatchPen.Dispose();
                matchBrush.Dispose();
                matchPen.Dispose();
            }

            public Boolean IsOnScreen()
            {
                if (isBenchmarkMode)
                {
                    stopWatch.Reset();

                    stopWatch.Start();
                }

                Boolean result = true;
                float matchRate = 0.0f;

                onScreenMap.Clear();

                if (map.Count == 0)
                {
                    result = false;
                }
                else
                {
                    int matchCount = 0;

                    foreach (KeyValuePair<Point, Color> feature in map)
                    {
                        Color fC = feature.Value;

                        Point[] scanPoints = CreateScanWindowPoints(feature.Key);

                        // Scan whole window for matching one
                        Boolean isMatch = false;
                        for (int i = 0; i < scanPoints.Length; i++)
                        {
                            Color c = GetColorAt(scanPoints[i]);

                            if (WithinTollerance(fC, c, tollerance))
                            {
                                isMatch = true;
                                break;
                            }
                        }

                        if (isMatch)
                        {
                            matchCount++;
                        }

                        onScreenMap[feature.Key] = isMatch;

                        if (!isMatch)
                        {
                            //Console.WriteLine("Feature:{0} has unmathed point({1}, {2}) with color({3}, {4}, {5})",
                            //    name, feature.Key.X, feature.Key.Y, fC.R, fC.G, fC.B);
                            //Console.WriteLine("Feature:{0} has unmathed point({1}, {2}), with color({3}, {4}, {5}) not match with color({6}, {7}, {8})", 
                            //    name, feature.Key.X, feature.Key.Y, c.R, c.G, c.B, fC.R, fC.G, fC.B);
                        }
                    }

                    matchRate = (float)Math.Round(Math.Round(((float)matchCount) / ((float)onScreenMap.Count), 1) * 100d);

                    if (matchRate >= matchRateThreshold)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }

                long costTime = -1;
                if (isBenchmarkMode)
                {
                    stopWatch.Stop();

                    costTime = stopWatch.ElapsedMilliseconds;
                }

                if (result)
                {
                    Console.WriteLine("★ Feature:{0} is on screen, rate is {1}%, takes {2} ms", name, matchRate, costTime);
                }
                else
                {
                    Console.WriteLine("★ Feature:{0} is NOT on screen, rate is {1}%, takes {2} ms", name, matchRate, costTime);
                }

                wasOnScreen = result;

                return result;
            }

            private RectangleF CreateBoundingBox(Point p)
            {
                float startPointX = ((float)p.X) - ((float)boundingBoxWidth) / 2.0f;
                float startPointY = ((float)p.Y) - ((float)boundingBoxHeight) / 2.0f;

                return new RectangleF(new PointF(startPointX, startPointY), new SizeF(boundingBoxWidth, boundingBoxHeight));
            }

            private Point[] CreateScanWindowPoints(Point p)
            {
                int width = scanWindowSize * 2 + 1;
                int height = scanWindowSize * 2 + 1;

                Point[] result = new Point[width * height];

                //Console.WriteLine("p({0}, {1}) = ", p.X, p.Y);

                // Y-axis
                for (int i = -scanWindowSize; i <= scanWindowSize; i++)
                {
                    // X-axis
                    for (int j = -scanWindowSize; j <= scanWindowSize; j++)
                    {
                        int index = (i + scanWindowSize) * scanWindowSize + (j + scanWindowSize);
                        result[index] = new Point(p.X + j, p.Y + i);
                        //Console.Write("window {0} = ({1}, {2})\n", index, result[index].X, result[index].Y);
                    }
                }

                return result;
            }

            private Boolean WithinTollerance(Color src, Color target, int tollerance)
            {
                return Math.Abs(src.R - target.R) <= tollerance &&
                    Math.Abs(src.G - target.G) <= tollerance &&
                    Math.Abs(src.B - target.B) <= tollerance;
            }
        }

        static void Main(string[] args)
        {
            // Prepare features
            btnSpace = new Feature("Btn Space");
            Dictionary<Point, Color> btnSpaceFeatures = btnSpace.map;

            //btnSpaceFeatures = new Dictionary<Point, Color>();
            //btnSpaceFeatures[new Point(907, 208)] = Color.FromArgb(255, 102, 84, 9);
            //btnSpaceFeatures[new Point(1010, 202)] = Color.FromArgb(255, 134, 105, 7);
            //btnSpaceFeatures[new Point(949, 194)] = Color.FromArgb(255, 133, 102, 6);
            //btnSpaceFeatures[new Point(919, 192)] = Color.FromArgb(255, 122, 91, 10);
            //btnSpaceFeatures[new Point(1005, 79)] = Color.FromArgb(255, 8, 23, 34);
            //btnSpaceFeatures[new Point(1051, 89)] = Color.FromArgb(255, 22, 37, 39);
            btnSpaceFeatures[new Point(1052, 80)] = Color.FromArgb(255, 228, 201, 122);
            btnSpaceFeatures[new Point(1008, 76)] = Color.FromArgb(255, 236, 215, 110);
            btnSpaceFeatures[new Point(1022, 83)] = Color.FromArgb(255, 244, 215, 97);
            btnSpaceFeatures[new Point(1026, 74)] = Color.FromArgb(255, 238, 215, 103);
            btnSpaceFeatures[new Point(1044, 79)] = Color.FromArgb(255, 244, 214, 92);
            btnSpaceFeatures[new Point(1066, 79)] = Color.FromArgb(255, 237, 204, 97);

            btnSpaceFeatures[new Point(1037, 84)] = Color.FromArgb(255, 235, 215, 120);
            btnSpaceFeatures[new Point(1073, 79)] = Color.FromArgb(255, 238, 216, 107);
             
            icFishing = new Feature("Icon Fishing");
            Dictionary<Point, Color> icFishingFeatures = icFishing.map;

            //icFishingFeatures = new Dictionary<Point, Color>();
            //icFishingFeatures[new Point(958, 48)] = Color.FromArgb(255, 255, 255, 255);
            icFishingFeatures[new Point(955, 39)] = Color.FromArgb(255, 255, 255, 255);

            string option = "0";

            if (args.Length > 0)
            {
                option = args[0];
            }

            if ("0".Equals(option))
            {
                // Create a Timer object that knows to call our TimerCallback
                // method once every 2000 milliseconds.
                System.Threading.Timer t = new System.Threading.Timer(TimerCallback, null, 0, 2000);

                form = new Form {
                    BackColor = Color.Magenta,
                    TransparencyKey = Color.Magenta,
                    FormBorderStyle = FormBorderStyle.None,
                    Bounds = Screen.PrimaryScreen.Bounds,
                    TopMost = true,
                };

                form.Load += Form_Load;
                form.Paint += Form_Paint;

                Application.EnableVisualStyles();
                Application.Run(form);
            }
            else
            {
                // Intimacy Mode
                Dictionary<Point, MouseKey> intimacyMouseKeyMap = new Dictionary<Point, MouseKey>();
                foreach (Point p in INTIMACY_WHEEL_ITEMS)
                {
                    intimacyMouseKeyMap.Add(p, MouseKey.RIGHT);
                }
                intimacyMouseKeyMap.Add(INTIMACY_WHEEL_LEFT, MouseKey.LEFT);
                intimacyMouseKeyMap.Add(INTIMACY_WHEEL_RIGHT, MouseKey.LEFT);

                List<Point> sequence = new List<Point>();

                switch (option)
                {
                    case "1":
                        // Purple set 1 -> 4 -> L -> 1 -> 5 -> R -> 3 -> L -> 4
                        //sequence = {
                        //    INTIMACY_WHEEL_ITEMS[0], INTIMACY_WHEEL_ITEMS[3], INTIMACY_WHEEL_LEFT,
                        //    INTIMACY_WHEEL_ITEMS[0], INTIMACY_WHEEL_ITEMS[4], INTIMACY_WHEEL_RIGHT,
                        //    INTIMACY_WHEEL_ITEMS[2], INTIMACY_WHEEL_LEFT, INTIMACY_WHEEL_ITEMS[3]};
                        sequence.Add(INTIMACY_WHEEL_ITEMS[0]);
                        sequence.Add(INTIMACY_WHEEL_ITEMS[3]);
                        sequence.Add(INTIMACY_WHEEL_LEFT);
                        sequence.Add(INTIMACY_WHEEL_ITEMS[0]);
                        sequence.Add(INTIMACY_WHEEL_ITEMS[4]);
                        sequence.Add(INTIMACY_WHEEL_RIGHT);
                        sequence.Add(INTIMACY_WHEEL_ITEMS[2]);
                        sequence.Add(INTIMACY_WHEEL_LEFT);
                        sequence.Add(INTIMACY_WHEEL_ITEMS[3]);
                        break;

                    case "2":
                        // RAW L -> 3 -> 4 -> L -> 1 -> L -> 4 -> 3 -> R -> R -> R -> 2 -> L -> 1
                        // NEW L -> 2 -> 3 -> L -> 0 -> L -> 3 -> 2 -> R -> R -> R -> 1 -> L -> 0
                        sequence.Add(INTIMACY_WHEEL_LEFT);
                        sequence.Add(INTIMACY_WHEEL_ITEMS[2]);
                        sequence.Add(INTIMACY_WHEEL_ITEMS[3]);
                        sequence.Add(INTIMACY_WHEEL_LEFT);
                        sequence.Add(INTIMACY_WHEEL_ITEMS[0]);
                        sequence.Add(INTIMACY_WHEEL_LEFT);
                        sequence.Add(INTIMACY_WHEEL_ITEMS[3]);
                        sequence.Add(INTIMACY_WHEEL_ITEMS[2]);
                        sequence.Add(INTIMACY_WHEEL_RIGHT);
                        sequence.Add(INTIMACY_WHEEL_RIGHT);
                        sequence.Add(INTIMACY_WHEEL_RIGHT);
                        sequence.Add(INTIMACY_WHEEL_ITEMS[1]);
                        sequence.Add(INTIMACY_WHEEL_LEFT);
                        sequence.Add(INTIMACY_WHEEL_ITEMS[0]);
                        break;

                    default:
                        return;
                }

                Random random = new Random(DateTime.Now.Millisecond);

                SendMLeftClick(INTIMACY_WHEEL_ITEMS[0].X, INTIMACY_WHEEL_ITEMS[0].Y);

                Thread.Sleep(random.Next(100, 250));

                SendMLeftClick(INTIMACY_WHEEL_ITEMS[0].X, INTIMACY_WHEEL_ITEMS[0].Y);

                foreach (Point p in sequence)
                {
                    if (INTIMACY_WHEEL_LEFT.Equals(p))
                    {
                        SendKey(DIK_A, KeyEventF.KeyDown);

                        Thread.Sleep(random.Next(120, 200));

                        SendKey(DIK_A, KeyEventF.KeyUp);
                    }
                    else if (INTIMACY_WHEEL_RIGHT.Equals(p))
                    {
                        SendKey(DIK_D, KeyEventF.KeyDown);

                        Thread.Sleep(random.Next(120, 200));

                        SendKey(DIK_D, KeyEventF.KeyUp);
                    }
                    else
                    {
                        MouseKey key = intimacyMouseKeyMap[p];
                        if (MouseKey.LEFT == key)
                        {
                            SendMLeftClick(p.X, p.Y);
                        }
                        else if (MouseKey.RIGHT == key)
                        {
                            SendMRightClick(p.X, p.Y);
                        }
                        else
                        {
                            Console.WriteLine("[IntimacyMode] Unknown key type");
                        }
                    }

                    Thread.Sleep(random.Next(700, 1200));
                }

                // Confirm
                SendMLeftClick(INTIMACY_WHEEL_CONFIRM.X, INTIMACY_WHEEL_CONFIRM.Y);

                Thread.Sleep(random.Next(50, 90));

                SendMLeftClick(INTIMACY_WHEEL_CONFIRM.X, INTIMACY_WHEEL_CONFIRM.Y);

                Thread.Sleep(random.Next(50, 90));

                SendMLeftClick(INTIMACY_WHEEL_CONFIRM.X, INTIMACY_WHEEL_CONFIRM.Y);

                Thread.Sleep(random.Next(50, 90));

                SendMLeftClick(INTIMACY_WHEEL_CONFIRM.X, INTIMACY_WHEEL_CONFIRM.Y);
            }
        }

        //static Point shift = new Point(1, 0);
        private static void Form_Paint(object sender, PaintEventArgs e)
        {
            //Console.Write("Form_Paint");

            //Pen myPen = new Pen(Color.Red);
            //myPen.Width = 30;
            //e.Graphics.DrawLine(myPen, new Point(0, 0), new Point(700 + shift.X, 700 + shift.Y));

            //shift.X += 10;

            //DrawStringInternal(e.Graphics, new Point(50, 50), "HAHAHA");

            if (isBenchmarkMode)
            {
                sw.Reset();

                sw.Start();
            }

            Boolean isFishing = icFishing.wasOnScreen;

            if (isFishing)
            {
                DrawStringInternal(e.Graphics, POS_FISHING_SIGN, "FISHING", Color.BlueViolet);
                icFishing.DrawBoundingBox(e.Graphics);
            }
            else
            {
                DrawStringInternal(e.Graphics, POS_FISHING_SIGN, "NOT FISHING", Color.OrangeRed);
                icFishing.DrawBoundingBox(e.Graphics);
            }

            if (btnSpace.wasOnScreen)
            {
                DrawStringInternal(e.Graphics, POS_DETECTION_SIGN, "DETECTED", Color.BlueViolet);
                btnSpace.DrawBoundingBox(e.Graphics);
            }
            else
            {
                DrawStringInternal(e.Graphics, POS_DETECTION_SIGN, "NO DETECTED", Color.OrangeRed);
                btnSpace.DrawBoundingBox(e.Graphics);
            }

            if (isBenchmarkMode)
            {
                sw.Stop();

                Console.WriteLine("★ Draw time {0}ms", sw.ElapsedMilliseconds);
            }
        }

        private static void Form_Load(object sender, EventArgs e)
        {
            Console.WriteLine("Form_Load");
            if (form == sender)
            {
                Console.WriteLine("form is sender!");
            }
            Form sourceForm = (Form)sender;
            SetWindowPos(sourceForm.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }

        public static void DrawStringInternal(Graphics graphics, Point startPoint, String text, Color color)
        {
            Graphics formGraphics = graphics;

            string drawString = text;

            FontFamily fontFamily = new FontFamily("Times New Roman");
            Font drawFont = new Font(
               fontFamily,
               35,
               FontStyle.Regular,
               GraphicsUnit.Pixel);

            SolidBrush drawBrush = new SolidBrush(color);

            float x = startPoint.X;
            float y = startPoint.Y;

            StringFormat drawFormat = new StringFormat();

            formGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            //formGraphics.DrawString(drawString, drawFont, drawBrush, x, y, drawFormat);
            formGraphics.DrawString(drawString, drawFont, drawBrush, startPoint);

            // Release
            drawFont.Dispose();
            drawBrush.Dispose();
            //formGraphics.Dispose();
        }

        static Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        public static Color GetColorAt(Point location)
        {
            if (isBenchmarkMode) { 
                sw.Reset();
                sw.Start();
            }

            Color result = Color.Black;

            try
            {
                using (Graphics gdest = Graphics.FromImage(screenPixel))
                {
                    using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        IntPtr hSrcDC = gsrc.GetHdc();
                        IntPtr hDC = gdest.GetHdc();
                        int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                        gdest.ReleaseHdc();
                        gsrc.ReleaseHdc();
                    }
                }

                result = screenPixel.GetPixel(0, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetColorAt() - Got exception : {0}", ex);
            }

            /*
            // Option 2
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);

            int a = (int)GetPixel(dc, location.X, location.Y);

            ReleaseDC(desk, dc);

            Color result = Color.FromArgb(255, (a >> 0) & 0xff, (a >> 8) & 0xff, (a >> 16) & 0xff);
            */

            if (isBenchmarkMode)
            {
                sw.Stop();

                getColorTimeSum += sw.ElapsedMilliseconds;
                getColorTimeCount++;
            }

            return result;
        }

        public static Color GetCursorColor()
        {
            /*
            Point cursor = new Point();
            GetCursorPos(ref cursor);

            var c = GetColorAt(cursor);
            */

            Point position = Cursor.Position;
            Point newPosition = new Point((int)(position.X * 1.75), (int)(position.Y * 1.75));

            Console.WriteLine("Position = {0}x{1}, newPosition = {2}x{3}", position.X, position.Y, newPosition.X, newPosition.Y);

            return GetColorAt(position);
        }

        private static double ToMillisecondsWith(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        private static void TimerCallback(Object o)
        {
            Console.WriteLine("");

            // Display the date/time when this method got called.
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine(DateTime.Now);
            Console.WriteLine("-----------------------------------------");

            SetFormTopMost(true);

            UpdateUI();

            // Force a garbage collection to occur for this demo.
            GC.Collect();
        }

        delegate void SetFormTopMostCallback(Boolean isTopMost);
        private static void SetFormTopMost(Boolean isTopMost)
        {
            if (form != null)
            {
                if (form.InvokeRequired)
                {
                    SetFormTopMostCallback callback = new SetFormTopMostCallback(SetFormTopMost);
                    form.Invoke(callback, isTopMost);
                }
                else
                {
                    form.TopLevel = true;
                    form.TopMost = true;

                    /*Pen myPen;
                    myPen = new Pen(Color.Red);
                    Graphics formGraphics = form.CreateGraphics();
                    formGraphics.DrawLine(myPen, 0, 0, 200, 200);
                    myPen.Dispose();
                    */
                    //formGraphics.Dispose();
                    //SetWindowPos(form.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
                }
            }
        }

        private static void SendKeyPress(ushort key)
        {
            SendKey(key, KeyEventF.KeyDown);

            Random random = new Random(DateTime.Now.Millisecond);

            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer((obj) =>
            {
                SendKey(key, KeyEventF.KeyUp);

                timer.Dispose();
            }, null, random.Next(120, 250), System.Threading.Timeout.Infinite);

            // Reset flag
            isKeyPending = false;
        }

        delegate void Runnable();
        private static void UpdateUI()
        {
            if (form != null)
            {
                if (form.InvokeRequired)
                {
                    /*
                    // Create runnable
                    Runnable r = delegate
                    {
                        UpdateUIInternal();
                    };

                    // Invoke runnalbe on form's thread
                    form.Invoke(r);
                    */
                    UpdateUIInternal();
                }
                else
                {
                    UpdateUIInternal();
                }
            }
        }

        private static void UpdateUIInternal()
        {
            Color c = GetCursorColor();

            Console.WriteLine("Color(RGBA) = {0}, {1}, {2}, {3}", c.R, c.G, c.B, c.A);

            btnSpace.IsOnScreen();

            icFishing.IsOnScreen();

            Runnable r = delegate
            {
                form.Invalidate(new Rectangle(767, 22, 878, 173), false);

                if (btnSpace.wasOnScreen)
                {
                    //TODO Could add top window/process check 

                    double currentTime = ToMillisecondsWith(DateTime.Now);

                    if (isKeyPending)
                    {
                        Console.WriteLine("★ Key is still pending now");
                    }
                    else if ((currentTime - lastKeyMessageTimeStamp) < KEY_MESSAGE_OFFSET)
                    {
                        //Console.WriteLine("★ Waiting... ({0} -> {1} = {2})", lastKeyMessageTimeStamp, currentTime, currentTime - lastKeyMessageTimeStamp);
                        Console.WriteLine("★ Waiting for {0}ms after last key message sent.", currentTime - lastKeyMessageTimeStamp);
                    }
                    else
                    {
                        isKeyPending = true;

                        Console.WriteLine("★ Prepare to send SPACE message");

                        Random random = new Random(DateTime.Now.Millisecond);
                        
                        System.Threading.Timer timer = null;
                        timer = new System.Threading.Timer((obj) =>
                        {
                            Console.WriteLine("★ Send SPACE message");

                            lastKeyMessageTimeStamp = ToMillisecondsWith(DateTime.Now);

                            SendKeyPress(DIK_SPACE);

                            timer.Dispose();
                        }, null, random.Next(3000, 7000), System.Threading.Timeout.Infinite);
                    }

                    //Process[] processes = Process.GetProcesses();
                    //Process[] processes = Process.GetProcessesByName("BlackDesert64");

                    //foreach (Process proc in processes)
                    //{
                    //    Console.WriteLine("★ Send SPACEBAR message to {0}", proc.ProcessName);
                    //    //PostMessage(proc.MainWindowHandle, WM_KEYDOWN, VK_SPACEBAR, 0);
                    //    //SendKeys.SendWait(" ");
                    
                    //}
                }
            };

            if (form.InvokeRequired)
            {
                form.Invoke(r);
            }
            else
            {
                r();
            }

            if (isBenchmarkMode)
            {
                Console.WriteLine("★ Get Color AVG time is {0}/{1} = {2}", getColorTimeSum, getColorTimeCount, getColorTimeSum / getColorTimeCount);
            }
        }
    }
}
