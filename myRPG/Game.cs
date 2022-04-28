using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Management;

namespace myRPG
{
    static class SetScreenColorsApp // Кастомные цвета в консоли
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            internal short X;
            internal short Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SMALL_RECT
        {
            internal short Left;
            internal short Top;
            internal short Right;
            internal short Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct COLORREF
        {
            internal uint ColorDWORD;

            internal COLORREF(Color color)
            {
                ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
            }

            internal COLORREF(uint r, uint g, uint b)
            {
                ColorDWORD = r + (g << 8) + (b << 16);
            }

            internal Color GetColor()
            {
                return Color.FromArgb((int)(0x000000FFU & ColorDWORD),
                                      (int)(0x0000FF00U & ColorDWORD) >> 8, (int)(0x00FF0000U & ColorDWORD) >> 16);
            }

            internal void SetColor(Color color)
            {
                ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CONSOLE_SCREEN_BUFFER_INFO_EX
        {
            internal int cbSize;
            internal COORD dwSize;
            internal COORD dwCursorPosition;
            internal ushort wAttributes;
            internal SMALL_RECT srWindow;
            internal COORD dwMaximumWindowSize;
            internal ushort wPopupAttributes;
            internal bool bFullscreenSupported;
            internal COLORREF black;
            internal COLORREF darkBlue;
            internal COLORREF darkGreen;
            internal COLORREF darkCyan;
            internal COLORREF darkRed;
            internal COLORREF darkMagenta;
            internal COLORREF darkYellow;
            internal COLORREF gray;
            internal COLORREF darkGray;
            internal COLORREF blue;
            internal COLORREF green;
            internal COLORREF cyan;
            internal COLORREF red;
            internal COLORREF magenta;
            internal COLORREF yellow;
            internal COLORREF white;
        }

        const int STD_OUTPUT_HANDLE = -11;                                        // per WinBase.h
        internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);    // per WinBase.h

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO_EX csbe);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO_EX csbe);

        // Set a specific console color to an RGB color
        // The default console colors used are gray (foreground) and black (background)
        public static int SetColor(ConsoleColor consoleColor, Color targetColor)
        {
            return SetColor(consoleColor, targetColor.R, targetColor.G, targetColor.B);
        }

        public static int SetColor(ConsoleColor color, uint r, uint g, uint b)
        {
            CONSOLE_SCREEN_BUFFER_INFO_EX csbe = new CONSOLE_SCREEN_BUFFER_INFO_EX();
            csbe.cbSize = (int)Marshal.SizeOf(csbe);                    // 96 = 0x60
            IntPtr hConsoleOutput = GetStdHandle(STD_OUTPUT_HANDLE);    // 7
            if (hConsoleOutput == INVALID_HANDLE_VALUE)
            {
                return Marshal.GetLastWin32Error();
            }
            bool brc = GetConsoleScreenBufferInfoEx(hConsoleOutput, ref csbe);
            if (!brc)
            {
                return Marshal.GetLastWin32Error();
            }

            switch (color)
            {
                case ConsoleColor.Black:
                    csbe.black = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkBlue:
                    csbe.darkBlue = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkGreen:
                    csbe.darkGreen = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkCyan:
                    csbe.darkCyan = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkRed:
                    csbe.darkRed = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkMagenta:
                    csbe.darkMagenta = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkYellow:
                    csbe.darkYellow = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Gray:
                    csbe.gray = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkGray:
                    csbe.darkGray = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Blue:
                    csbe.blue = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Green:
                    csbe.green = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Cyan:
                    csbe.cyan = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Red:
                    csbe.red = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Magenta:
                    csbe.magenta = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Yellow:
                    csbe.yellow = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.White:
                    csbe.white = new COLORREF(r, g, b);
                    break;
            }
            ++csbe.srWindow.Bottom;
            ++csbe.srWindow.Right;
            brc = SetConsoleScreenBufferInfoEx(hConsoleOutput, ref csbe);
            if (!brc)
            {
                return Marshal.GetLastWin32Error();
            }
            return 0;
        }

        public static int SetScreenColors(Color foregroundColor, Color backgroundColor)
        {
            int irc;
            irc = SetColor(ConsoleColor.Gray, foregroundColor);
            if (irc != 0) return irc;
            irc = SetColor(ConsoleColor.Black, backgroundColor);
            if (irc != 0) return irc;

            return 0;
        }
    }
    static class DisableConsoleQuickEdit // отключение редактирования (выделения) ЛКМ
    {
        const uint ENABLE_QUICK_EDIT = 0x0040;

        const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        internal static bool Go()
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
                return false;

            consoleMode &= ~ENABLE_QUICK_EDIT;

            if (!SetConsoleMode(consoleHandle, consoleMode))
                return false;

            return true;
        }
    }
    public static class Game
    {
        #region Подключение библиотек
        //Доступ к буферу консоли
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteConsoleOutput(
          SafeFileHandle hConsoleOutput,
          CharInfo[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref SmallRect lpWriteRegion);

        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct CharUnion
        {
            [FieldOffset(0)] public char UnicodeChar;
            [FieldOffset(0)] public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct CharInfo
        {
            [FieldOffset(0)] public CharUnion Char;
            [FieldOffset(2)] public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        // Максимизация консоли
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        private static IntPtr ThisConsole = GetConsoleWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Выбор шрифтра консоли
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal unsafe struct CONSOLE_FONT_INFO_EX
        {
            internal uint cbSize;
            internal uint nFont;
            internal COORD dwFontSize;
            internal int FontFamily;
            internal int FontWeight;
            internal fixed char FaceName[LF_FACESIZE];
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            internal short X;
            internal short Y;
            internal COORD(short x, short y)
            {
                X = x;
                Y = y;
            }
        }
        private const int STD_OUTPUT_HANDLE = -11;
        private const int TMPF_TRUETYPE = 4;
        private const int LF_FACESIZE = 32;
        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetCurrentConsoleFontEx(IntPtr consoleOutput, bool maximumWindow, ref CONSOLE_FONT_INFO_EX consoleCurrentFontEx);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int dwType);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int SetConsoleFont(IntPtr hOut, uint dwFontNum);

        // Эмуляция нажатия клавиш
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte vk, byte scan, int flags, int extrainfo);

        // Получение состояния клавиши
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int key);
        #endregion

        #region Объявление глобальных переменных
        private static SafeFileHandle h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

        public static Random Rand = new Random();

        public static int ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
        public static int ScreenWidth = Screen.PrimaryScreen.Bounds.Width;

        public static int RenderScale = 3;
        public static bool TrueSight = false;

        private static int WindowHeight;
        private static int WindowWidth;

        private static int OutputLevelHeight;
        private static int OutputLevelWidth;
        private static int OutputLevelPositionX;
        private static int OutputLevelPositionY;

        private static WindowCell[,] OutputInterface;
        private static WindowCell[,] OutputLevel;

        private static int FPSRenderInterface = 0;
        private static int FPSRenderLevel = 0;
        private static int FPSRayCasting = 0;
        private static int FPSRefreshScreen = 0;

        public static double FpsLimit = 32;
        public static bool DisplayInfo = true;

        public static string[] MenuItems = { "CONTINUE", "NEW GAME", "CONTROLS", "OPTIONS", "QUIT GAME" };
        public static int ChosenItem = 0;
        public static bool ItemOpened = false;

        public static bool Pause = false;
        public static bool Restarting = false;

        private static double logSpace = 3;
        public static List<string> CombatLog;

        struct WindowCell
        {
            public byte ASCIIChar;
            public short Color;
        }

        public static Thread RestartingStateCheck;

        // Игровые объекты
        public static Map map;
        public static List<Monster> monsters;
        public static Player player;

        public static Thread refreshScreen;
        public static Thread renderInterface;
        public static Thread renderLevel;
        public static Thread rayCasting;
        #endregion

        static void Main()
        {
            Console.Title = "My RPG";
            //SetConsoleFont("Consolas", 16);
            SetConsoleColorGardient(20, 20, 20, 20, 20, 20);
            Console.BackgroundColor = (ConsoleColor)0;
            Console.ForegroundColor = (ConsoleColor)15;
            DisableConsoleQuickEdit.Go();
            ShowWindow(ThisConsole, 3);
            keybd_event(0x7A, 0, 0, 0);
            keybd_event(0x7A, 0, 0x2, 0);
            Thread.Sleep(300);

            WindowHeight = Console.WindowHeight;
            WindowWidth = Console.WindowWidth;

            Console.SetWindowPosition(0, 0);
            Console.SetWindowSize(WindowWidth, WindowHeight);
            Console.SetBufferSize(WindowWidth, WindowHeight);

            OutputInterface = new WindowCell[WindowHeight, WindowWidth];
            OutputLevel = new WindowCell[WindowHeight, WindowWidth];

            OutputLevelHeight = (int)(WindowHeight * 1);
            OutputLevelWidth = (int)(WindowWidth * 1);
            OutputLevelPositionX = WindowWidth / 2 - OutputLevelWidth / 2;
            OutputLevelPositionY = WindowHeight / 2 - OutputLevelHeight / 2;
            OutputLevel = new WindowCell[OutputLevelHeight, OutputLevelWidth];

            CombatLog = new List<string>();
            for (int n = 0; n < WindowHeight / 3d / logSpace; n++)
                CombatLog.Add("");

            map = new Map(256, 256, 5, 4, 47, 10);

            monsters = new List<Monster>();

            player = new Player("Mister Player", '@', 1, 100, 0.5, 5, 10, 0.25, 5, 32, 100, map, monsters);

            for (int m = 0; m < 100; m++)
                monsters.Add(new Monster("Rat", 'R', 5, 1, 24, 300, 0, 0, map, player, monsters));
            for (int m = 0; m < 10; m++)
                monsters.Add(new Monster("Snake", 'S', 10, 5, 48, 650, 0, 0, map, player, monsters));
            monsters.Add(new Monster("Cave Dragon", 'D', 150, 35, 128, 1000, 99, 4, map, player, monsters));

            refreshScreen = new Thread(RefreshScreen);
            renderInterface = new Thread(RenderInterface);
            renderLevel = new Thread(RenderLevel);
            rayCasting = new Thread(RayCasting);

            refreshScreen.Start();
            renderInterface.Start();
            renderLevel.Start();
            rayCasting.Start();

            RestartingStateCheck = new Thread(Restart);
            RestartingStateCheck.Start();
            RestartingStateCheck.Priority = ThreadPriority.Lowest;
        }
        static void Restart()
        {
            while (true)
            {
                if (!Restarting)
                {
                    Thread.Sleep(30);
                }
                else
                {
                    Restarting = false;

                    refreshScreen.Abort();
                    renderInterface.Abort();
                    renderLevel.Abort();
                    rayCasting.Abort();

                    CombatLog.Clear();
                    for (int n = 0; n < WindowHeight / 3d / logSpace; n++)
                        CombatLog.Add("");

                    // LOH? Надо убить монстров и игрока и создать их ещё раз

                    refreshScreen = new Thread(RefreshScreen);
                    renderInterface = new Thread(RenderInterface);
                    renderLevel = new Thread(RenderLevelBig);
                    rayCasting = new Thread(RayCasting);

                    refreshScreen.Start();
                    renderInterface.Start();
                    renderLevel.Start();
                    rayCasting.Start();
                }
            }
        }
        static void Reset()
        {
            WindowHeight = Console.WindowHeight;
            WindowWidth = Console.WindowWidth;

            Console.SetWindowPosition(0, 0);
            Console.SetWindowSize(WindowWidth, WindowHeight);
            Console.SetBufferSize(WindowWidth, WindowHeight);

            OutputInterface = new WindowCell[WindowHeight, WindowWidth];
            OutputLevel = new WindowCell[WindowHeight, WindowWidth];

            OutputLevelHeight = (int)(WindowHeight * 1);
            OutputLevelWidth = (int)(WindowWidth * 1);
            OutputLevelPositionX = WindowWidth / 2 - OutputLevelWidth / 2;
            OutputLevelPositionY = WindowHeight / 2 - OutputLevelHeight / 2;
            OutputLevel = new WindowCell[OutputLevelHeight, OutputLevelWidth];

            List<string> temp = new List<string>();
            for (int n = (int)(WindowHeight / 3d / logSpace); n > 0; n--)
            {
                if (n < CombatLog.Count)
                    temp.Add(CombatLog[n]);
                else
                    temp.Add("");
            }
            CombatLog = temp;
        }
        static void RefreshScreen()
        {
            DateTime fpsLimitStartingTime = DateTime.Now;
            int frameCounter = 0;

            while (true)
            {
                DateTime frameStartingTime = DateTime.Now;

                try
                {
                    if (WindowHeight != Console.WindowHeight || WindowWidth != Console.WindowWidth)
                        Reset();

                    Console.CursorVisible = false;
                    
                    WindowCell[,] bufferLevel = (WindowCell[,])OutputLevel.Clone();
                    WindowCell[,] bufferInterface = (WindowCell[,])OutputInterface.Clone();

                    if (!h.IsInvalid)
                    {
                        CharInfo[] buf = new CharInfo[WindowWidth * WindowHeight];
                        SmallRect rect = new SmallRect() { Left = 0, Top = 0, Right = (short)WindowWidth, Bottom = (short)WindowHeight };

                        for (int i = 0; i < WindowHeight; i++)
                            for (int j = 0; j < WindowWidth; j++)
                            {
                                if (bufferInterface[i, j].ASCIIChar != 0 && bufferInterface[i, j].Color != 0)
                                {
                                    buf[i * WindowWidth + j].Attributes = bufferInterface[i, j].Color;
                                    buf[i * WindowWidth + j].Char.AsciiChar = bufferInterface[i, j].ASCIIChar;
                                }
                                else if (i >= OutputLevelPositionY && i < OutputLevelPositionY + OutputLevelHeight && j >= OutputLevelPositionX && j < OutputLevelPositionX + OutputLevelWidth)
                                {
                                    buf[i * WindowWidth + j].Attributes = bufferLevel[i, j].Color;
                                    buf[i * WindowWidth + j].Char.AsciiChar = bufferLevel[i, j].ASCIIChar;
            }
                            }

                        WriteConsoleOutput(h, buf,
                            new Coord() { X = (short)WindowWidth, Y = (short)WindowHeight },
                            new Coord() { X = 0, Y = 0 },
                            ref rect);
                    }

                }
                catch (Exception ex) { try { File.AppendAllText("log.txt", DateTime.Now + "\nОшибка при обновлении экрана:\n" + ex.ToString() + "\n\n\n", Encoding.UTF8); } catch { } }

                // Рассчёт FPS
                if ((DateTime.Now - fpsLimitStartingTime).TotalSeconds >= 0.5)
                {
                    if (frameCounter > 0)
                        FPSRefreshScreen = frameCounter * 2;
                    else
                        FPSRefreshScreen = 0;
                    frameCounter = 0;
                    fpsLimitStartingTime = DateTime.Now;
                }
                else frameCounter++;

                // Ограничение FPS
                DateTime frameFinishingTime = DateTime.Now;
                if ((frameFinishingTime - frameStartingTime).TotalMilliseconds < 1000 / FpsLimit)
                    Thread.Sleep((int)(1000 / FpsLimit - (frameFinishingTime - frameStartingTime).TotalMilliseconds));
            }
        }
        static void RenderInterface()
        {
            DateTime fpsLimitStartingTime = DateTime.Now;
            int frameCounter = 0;

            while (true)
            {
                DateTime frameStartingTime = DateTime.Now;

                try
                {
                    WindowCell[,] buffer = new WindowCell[WindowHeight, WindowWidth];

                    if (Pause)
                    {
                        if (monsters.Count(m => !m.Alive) == monsters.Count())
                        {
                            string youWin = "!  YOU HAVE SLAIN ALL ENEMIES  !";
                            for (int i = 0; i < buffer.GetLength(0); i++)
                            {
                                for (int j = 0; j < buffer.GetLength(1); j++)
                                {
                                    buffer[i, j].ASCIIChar = 219;
                                    buffer[i, j].Color = 2 | 2 << 4;
                                }
                            }
                            for (int n = 0; n < youWin.Length; n++)
                            {
                                if (youWin[n] == ' ')
                                {
                                    buffer[WindowHeight / 2, n + WindowWidth / 2 - youWin.Length / 2].ASCIIChar = 219;
                                    buffer[WindowHeight / 2, n + WindowWidth / 2 - youWin.Length / 2].Color = 2 | 2 << 4;
                                }
                                else
                                {
                                    buffer[WindowHeight / 2, n + WindowWidth / 2 - youWin.Length / 2].ASCIIChar = (byte)youWin[n];
                                    buffer[WindowHeight / 2, n + WindowWidth / 2 - youWin.Length / 2].Color = 4 | 2 << 4;
                                }
                            }
                        }
                        else if (!player.Alive)
                        {
                            string youAreDead = "+  YOU HAVE BEEN SLAIN  +";
                            for (int i = 0; i < buffer.GetLength(0); i++)
                            {
                                for (int j = 0; j < buffer.GetLength(1); j++)
                                {
                                    buffer[i, j].ASCIIChar = 219;
                                    buffer[i, j].Color = 0 | 1 << 4;
                                }
                            }
                            for (int n = 0; n < youAreDead.Length; n++)
                            {
                                if (youAreDead[n] == '+')
                                {
                                    buffer[WindowHeight / 2, n + WindowWidth / 2 - youAreDead.Length / 2].ASCIIChar = 197;
                                    buffer[WindowHeight / 2, n + WindowWidth / 2 - youAreDead.Length / 2].Color = 3 | 0 << 4;
                                }
                                else if (youAreDead[n] == ' ')
                                {
                                    buffer[WindowHeight / 2, n + WindowWidth / 2 - youAreDead.Length / 2].ASCIIChar = 219;
                                    buffer[WindowHeight / 2, n + WindowWidth / 2 - youAreDead.Length / 2].Color = 0 | 1 << 4;
                                }
                                else
                                {
                                    buffer[WindowHeight / 2, n + WindowWidth / 2 - youAreDead.Length / 2].ASCIIChar = (byte)youAreDead[n];
                                    buffer[WindowHeight / 2, n + WindowWidth / 2 - youAreDead.Length / 2].Color = 3 | 0 << 4;
                                }
                            }
                        }

                        for (int i = 0; i < MenuItems.Length; i++)
                        {
                            if (i == ChosenItem && ItemOpened)
                            {
                                switch (i)
                                {
                                    /*case 0:
                                        {
                                            for (int n = 0; n < MenuItems[i].Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2].ASCIIChar = (byte)MenuItems[i][n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2].Color = 10;
                                            }

                                            string temp = "(continue)";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + MenuItems[i].Length + 3].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + MenuItems[i].Length + 3].Color = 11;
                                            }
                                            break;
                                        }*/
                                    case 1:
                                        {
                                            for (int n = 0; n < MenuItems[i].Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2].ASCIIChar = (byte)MenuItems[i][n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2].Color = 10;
                                            }
                                            int step = MenuItems[i].Length + 3;

                                            string temp = "Start a new game?";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 11;
                                            }
                                            step += temp.Length + 3;

                                            temp = "(Y)YES";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 13;
                                            }
                                            step += temp.Length + 3;

                                            temp = "(N)NO";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 12;
                                            }
                                            step += temp.Length + 3;

                                            temp = "<this function is not working>";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 7;
                                            }
                                            break;
                                        }
                                    case 2:
                                        {
                                            for (int n = 0; n < MenuItems[i].Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2].ASCIIChar = (byte)MenuItems[i][n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2].Color = 10;
                                            }
                                            int step = MenuItems[i].Length + 3;

                                            string temp = "Move / Attack:";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 11;
                                            }
                                            step += temp.Length + 1;

                                            temp = "W A S D";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 15;
                                            }
                                            step += temp.Length + 1;

                                            temp = "or";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 11;
                                            }
                                            step += temp.Length + 1;

                                            temp = "NumPad";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 15;
                                            }
                                            step += temp.Length + 1;

                                            temp = "arrows";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 11;
                                            }
                                            step = MenuItems[i].Length + 3;

                                            temp = "Dig a wall:";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + (i + 1) * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + (i + 1) * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 11;
                                            }
                                            step += temp.Length + 1;

                                            temp = "Ctrl";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + (i + 1) * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + (i + 1) * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 15;
                                            }
                                            step += temp.Length + 1;

                                            temp = "+ Move";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + (i + 1) * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + (i + 1) * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 11;
                                            }

                                            break;
                                        }
                                    case 3:
                                        {
                                            for (int n = 0; n < MenuItems[i].Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2].ASCIIChar = (byte)MenuItems[i][n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2].Color = 10;
                                            }
                                            int step = MenuItems[i].Length + 3;

                                            string temp = "FPS limit";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 11;
                                            }
                                            step += temp.Length + 3;

                                            temp = "(-)";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 10;
                                            }
                                            step += temp.Length + 1;

                                            temp = FpsLimit != -1 ? $"{FpsLimit}" : "INF";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 14;
                                            }
                                            step += temp.Length + 1;

                                            temp = "(+)";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 10;
                                            }
                                            step = MenuItems[i].Length + 3;

                                            temp = "Display system info";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + (i + 1) * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + (i + 1) * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 11;
                                            }
                                            step += temp.Length + 3;

                                            temp = "(Y)YES";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + (i + 1) * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + (i + 1) * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = (short)(DisplayInfo ? 13 : 10);
                                            }
                                            step += temp.Length + 3;

                                            temp = "(N)NO";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + (i + 1) * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + (i + 1) * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = (short)(!DisplayInfo ? 12 : 10);
                                            }
                                            break;
                                        }
                                    case 4:
                                        {
                                            for (int n = 0; n < MenuItems[i].Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2].ASCIIChar = (byte)MenuItems[i][n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2].Color = 10;
                                            }
                                            int step = MenuItems[i].Length + 3;

                                            string temp = "Are you sure?";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 11;
                                            }
                                            step += temp.Length + 3;

                                            temp = "(Y)YES";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 13;
                                            }
                                            step += temp.Length + 3;

                                            temp = "(N)NO";
                                            for (int n = 0; n < temp.Length; n++)
                                            {
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].ASCIIChar = (byte)temp[n];
                                                buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 2 + step].Color = 12;
                                            }
                                            break;
                                        }
                                }
                            }
                            else if (i == ChosenItem)
                                for (int n = 0; n < MenuItems[i].Length; n++)
                                {
                                    buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 1].ASCIIChar = (byte)MenuItems[i][n];
                                    buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n + 1].Color = (short)(i == ChosenItem ? 11 : 5);
                                }
                            else
                                for (int n = 0; n < MenuItems[i].Length; n++)
                                {
                                    buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n].ASCIIChar = (byte)MenuItems[i][n];
                                    buffer[(int)(WindowHeight / 4d) + i * 3, (int)(WindowHeight / 4d) + n].Color = (short)(i == ChosenItem ? 11 : 5);
                                }
                        }
                    }
                    else if (monsters.Count(m => !m.Alive) == monsters.Count())
                    {
                        string youWin = "!  YOU HAVE SLAIN ALL ENEMIES  !";
                        for (int i = 0; i < buffer.GetLength(0); i++)
                        {
                            for (int j = 0; j < buffer.GetLength(1); j++)
                            {
                                buffer[i, j].ASCIIChar = 197;
                                buffer[i, j].Color = 3 | 1 << 4;
                            }
                        }
                        for (int n = 0; n < youWin.Length; n++)
                        {
                            if (youWin[n] == ' ')
                            {
                                buffer[WindowHeight / 2, n + WindowWidth / 2 - youWin.Length / 2].ASCIIChar = 197;
                                buffer[WindowHeight / 2, n + WindowWidth / 2 - youWin.Length / 2].Color = 3 | 1 << 4;
                            }
                            else
                            {
                                buffer[WindowHeight / 2, n + WindowWidth / 2 - youWin.Length / 2].ASCIIChar = (byte)youWin[n];
                                buffer[WindowHeight / 2, n + WindowWidth / 2 - youWin.Length / 2].Color = 13 | 1 << 4;
                            }
                        }
                    }
                    else if (!player.Alive)
                    {
                        string youAreDead = "+  YOU HAVE BEEN SLAIN  +";
                        for (int i = 0; i < buffer.GetLength(0); i++)
                        {
                            for (int j = 0; j < buffer.GetLength(1); j++)
                            {
                                buffer[i, j].ASCIIChar = 197;
                                buffer[i, j].Color = 0 | 1 << 4;
                            }
                        }
                        for (int n = 0; n < youAreDead.Length; n++)
                        {
                            if (youAreDead[n] == '+')
                            {
                                buffer[WindowHeight / 2, n + WindowWidth / 2 - youAreDead.Length / 2].ASCIIChar = 197;
                                buffer[WindowHeight / 2, n + WindowWidth / 2 - youAreDead.Length / 2].Color = 12 | 1 << 4;
                            }
                            else if (youAreDead[n] == ' ')
                            {
                                buffer[WindowHeight / 2, n + WindowWidth / 2 - youAreDead.Length / 2].ASCIIChar = 197;
                                buffer[WindowHeight / 2, n + WindowWidth / 2 - youAreDead.Length / 2].Color = 0 | 1 << 4;
                            }
                            else
                            {
                                buffer[WindowHeight / 2, n + WindowWidth / 2 - youAreDead.Length / 2].ASCIIChar = (byte)youAreDead[n];
                                buffer[WindowHeight / 2, n + WindowWidth / 2 - youAreDead.Length / 2].Color = 12 | 1 << 4;
                            }
                        }
                    }
                    else
                    {
                        // Отображение журнала боя
                        List<string> tempCombatLog = new List<string>(CombatLog);
                        for (int i = tempCombatLog.Count - 1; i >= 0; i--)
                        {
                            try
                            {
                                for (int n = 0; n < tempCombatLog[i].Length; n++)
                                {
                                    if (tempCombatLog[i][n] == ' ')
                                        continue;

                                    buffer[((int)(WindowHeight * 0.6) + (int)(WindowHeight / 3d / logSpace)) - (int)(tempCombatLog.Count * logSpace - i * logSpace) + (int)logSpace, n + 3].ASCIIChar = (byte)tempCombatLog[i][n];
                                    buffer[((int)(WindowHeight * 0.6) + (int)(WindowHeight / 3d / logSpace)) - (int)(tempCombatLog.Count * logSpace - i * logSpace) + (int)logSpace, n + 3].Color = (short)(i > 10 ? 10 : i);
                                }
                            }
                            catch (Exception ex) { try { File.AppendAllText("log.txt", DateTime.Now + "\nОшибка при отрисовке интерфейса -> Журнал боя:\n" + ex.ToString() + "\n\n\n", Encoding.UTF8); } catch { } }
                        }

                        // Отображение системной информации
                        try
                        {
                            if (DisplayInfo)
                            {
                                string info = "Output FPS:" + $"{FPSRefreshScreen}".PadLeft(4).PadRight(9) + "Interface FPS:" + $"{FPSRenderInterface}".PadLeft(4).PadRight(9) + "Rendering FPS:" + $"{FPSRenderLevel}".PadLeft(4).PadRight(9) + "Ray-Casting FPS:" + $"{FPSRayCasting}".PadLeft(4).PadRight(9);
                                if (info.Length > buffer.GetLength(1))
                                    info = "Output FPS:" + $"{FPSRefreshScreen}".PadLeft(4);

                                for (int n = 0; n < info.Length; n++)
                                {
                                    if (info[n] == ' ')
                                        continue;

                                    buffer[1, n + 1].ASCIIChar = (byte)info[n];
                                    buffer[1, n + 1].Color = 3;
                                }
                            }
                        }
                        catch (Exception ex) { try { File.AppendAllText("log.txt", DateTime.Now + "\nОшибка при отрисовке интерфейса -> Системная информация:\n" + ex.ToString() + "\n\n\n", Encoding.UTF8); } catch { } }

                        // Отображение статуса (копать стены)
                        string diggingState = player.isDig ? "Dig" : "";

                        for (int n = 0; n < diggingState.Length; n++)
                        {
                            if (diggingState[n] == ' ')
                                continue;

                            buffer[3, n + 1].ASCIIChar = (byte)diggingState[n];
                            buffer[3, n + 1].Color = 11;
                        }

                        // Отображение радара
                        int monsterID = 0;
                        double distance = int.MaxValue;
                        foreach (Monster m in monsters)
                        {
                            if (!m.Alive)
                                continue;

                            double temp = Math.Sqrt(((player.X + 0.5) - (m.X + 0.5)) * ((player.X + 0.5) - (m.X + 0.5)) + ((player.Y + 0.5) - (m.Y + 0.5)) * ((player.Y + 0.5) - (m.Y + 0.5)));
                            if (temp < distance)
                            {
                                distance = temp;
                                monsterID = m.ID;
                            }
                            if (distance <= 1)
                                break;
                        }

                        double nearestMonsterAngle = 0;

                        if (monsters[monsterID].X == player.X)
                        {
                            if (monsters[monsterID].Y > player.Y)
                                nearestMonsterAngle = 0;
                            else
                                nearestMonsterAngle = Math.PI;
                        }
                        else if (monsters[monsterID].Y == player.Y)
                        {
                            if (monsters[monsterID].X < player.X)
                                nearestMonsterAngle = Math.PI * 0.5;
                            else
                                nearestMonsterAngle = Math.PI * 1.5;
                        }
                        else
                        {
                            if (monsters[monsterID].X < player.X)
                                nearestMonsterAngle = Math.PI * 0.5 + Math.Atan((monsters[monsterID].Y - player.Y) / (double)(monsters[monsterID].X - player.X));
                            else
                                nearestMonsterAngle = Math.PI * 1.5 + Math.Atan((monsters[monsterID].Y - player.Y) / (double)(monsters[monsterID].X - player.X));
                        }

                        nearestMonsterAngle = nearestMonsterAngle - Math.PI >= 0 ? nearestMonsterAngle - Math.PI : nearestMonsterAngle + Math.PI;

                        char arrow = ' ';
                        if (nearestMonsterAngle >= (Math.PI * 2) * 1 / 8 && nearestMonsterAngle < (Math.PI * 2) * 3 / 8)
                            arrow = '>';
                        else if (nearestMonsterAngle >= (Math.PI * 2) * 3 / 8 && nearestMonsterAngle < (Math.PI * 2) * 5 / 8)
                            arrow = 'v';
                        else if (nearestMonsterAngle >= (Math.PI * 2) * 5 / 8 && nearestMonsterAngle < (Math.PI * 2) * 7 / 8)
                            arrow = '<'; 
                        else if (nearestMonsterAngle >= (Math.PI * 2) * 7 / 8 || nearestMonsterAngle < (Math.PI * 2) * 1 / 8)
                            arrow = '^';

                        char[,] radar =
                            {{' ', ' ', ' ', ' ', ' ', '.', '-', '-', '-', '-', '-', '.', ' ', ' ', ' ', ' ', ' '},
                             {' ', ' ', ' ', ' ', '/', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '\\', ' ', ' ', ' ', ' '},
                             {' ', ' ', ' ', '/', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '\\', ' ', ' ', ' '},
                             {' ', ' ', '/', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '\\', ' ', ' '},
                             {' ', '/', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '\\', ' '},
                             {'.', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '.'},
                             {'|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|'},
                             {'|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|'},
                             {'|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', player.Icon, ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|'},
                             {'|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|'},
                             {'|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|'},
                             {'.', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '.'},
                             {' ', '\\', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '/', ' '},
                             {' ', ' ', '\\', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '/', ' ', ' '},
                             {' ', ' ', ' ', '\\', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '/', ' ', ' ', ' '},
                             {' ', ' ', ' ', ' ', '\\', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '/', ' ', ' ', ' ', ' '},
                             {' ', ' ', ' ', ' ', ' ', '.', '-', '-', '-', '-', '-', '.', ' ', ' ', ' ', ' ', ' '}};

                        distance = distance != 1 ? distance : 0;

                        for (int n = 5; n >= 0; n--)
                            radar[(int)Math.Round(8 - Math.Cos(-nearestMonsterAngle) * (1 + (distance / 2d < n ? distance / 2d : n))), (int)Math.Round(8 - Math.Sin(-nearestMonsterAngle) * (1 + (distance / 2d < n ? distance / 2d : n)))] = '.';

                        if (distance / 2d <= 6)
                            radar[(int)Math.Round(8 - Math.Cos(-nearestMonsterAngle) * (1 + (distance / 2d < 6 ? distance / 2d : 6))), (int)Math.Round(8 - Math.Sin(-nearestMonsterAngle) * (1 + (distance / 2d < 6 ? distance / 2d : 6)))] = monsters[monsterID].Icon;
                        else
                            radar[(int)Math.Round(8 - Math.Cos(-nearestMonsterAngle) * (1 + (distance / 2d < 6 ? distance / 2d : 6))), (int)Math.Round(8 - Math.Sin(-nearestMonsterAngle) * (1 + (distance / 2d < 6 ? distance / 2d : 6)))] = arrow;

                        for (int i = 0; i < radar.GetLength(0); i++)
                        {
                            for (int j = 0; j < radar.GetLength(1); j++)
                            {
                                if (radar[i, j] == '|')
                                {
                                    buffer[WindowHeight - radar.GetLength(0) + i - 1, j + 1].ASCIIChar = 179;
                                    buffer[WindowHeight - radar.GetLength(0) + i - 1, j + 1].Color = 11;
                                }
                                else if(radar[i, j] == '-')
                                {
                                    buffer[WindowHeight - radar.GetLength(0) + i - 1, j + 1].ASCIIChar = 196;
                                    buffer[WindowHeight - radar.GetLength(0) + i - 1, j + 1].Color = 11;
                                }
                                else if (radar[i, j] == '.')
                                {
                                    buffer[WindowHeight - radar.GetLength(0) + i - 1, j + 1].ASCIIChar = 250;
                                    buffer[WindowHeight - radar.GetLength(0) + i - 1, j + 1].Color = 11;
                                }
                                else if (radar[i, j] != ' ')
                                {
                                    buffer[WindowHeight - radar.GetLength(0) + i - 1, j + 1].ASCIIChar = (byte)radar[i, j];
                                    buffer[WindowHeight - radar.GetLength(0) + i - 1, j + 1].Color = 11;
                                }
                            }
                        }

                        // Отображение прогресса уровня
                        int cells = 0;
                        int checkedCells = 0;
                        for (int i = 0; i < map.MapHeight; i++)
                            for (int j = 0; j < map.MapWidth; j++)
                            {
                                if (map.LevelMap[i, j].Type == 0)
                                {
                                    cells++;
                                    if (map.LevelMap[i, j].Seen)
                                        checkedCells++;
                                }
                            }

                        int levelResearchBarLength = WindowWidth / 3;
                        string levelResearchTemp = $"{(checkedCells / (double)cells * 100):f2}%".Replace(',', '.').PadLeft(7);

                        for (int n = -1; n <= levelResearchBarLength; n++)
                        {
                            if (n == -1)
                            {
                                buffer[3, n + WindowWidth / 2 - levelResearchBarLength / 2].ASCIIChar = (byte)'[';
                                buffer[3, n + WindowWidth / 2 - levelResearchBarLength / 2].Color = 11;
                            }
                            else if (n == levelResearchBarLength)
                            {
                                buffer[3, n + WindowWidth / 2 - levelResearchBarLength / 2].ASCIIChar = (byte)']';
                                buffer[3, n + WindowWidth / 2 - levelResearchBarLength / 2].Color = 11;
                            }
                            else
                            {
                                if (n >= levelResearchBarLength / 2 - levelResearchTemp.Length / 2 && n <= levelResearchBarLength / 2 + levelResearchTemp.Length / 2)
                                    buffer[3, n + WindowWidth / 2 - levelResearchBarLength / 2].ASCIIChar = (byte)levelResearchTemp[n - (levelResearchBarLength / 2 - levelResearchTemp.Length / 2)];
                                else
                                    buffer[3, n + WindowWidth / 2 - levelResearchBarLength / 2].ASCIIChar = 32;

                                if (n / (double)(WindowWidth / 3) * 100 < checkedCells / (double)cells * 100)
                                    buffer[3, n + WindowWidth / 2 - levelResearchBarLength / 2].Color = 0 | 15 << 4;
                                else
                                    buffer[3, n + WindowWidth / 2 - levelResearchBarLength / 2].Color = 0 | 5 << 4;
                            }
                        }

                        string levelResearchLabel = "Level research:";

                        for (int n = 0; n < levelResearchLabel.Length; n++)
                        {
                            if (levelResearchLabel[n] == ' ')
                                continue;

                            buffer[3, n + WindowWidth / 2 - levelResearchBarLength / 2 - levelResearchLabel.Length - 4].ASCIIChar = (byte)levelResearchLabel[n];
                            buffer[3, n + WindowWidth / 2 - levelResearchBarLength / 2 - levelResearchLabel.Length - 4].Color = 11;
                        }

                        // Отображение прогресса в убийстве монстров
                        string monstersSlain = "Monsters slain:" + $"{monsters.Count(m => !m.Alive)}".PadLeft($"{monsters.Count()}".Length + 1) + " / " + monsters.Count();

                        for (int n = 0; n < monstersSlain.Length; n++)
                        {
                            if (monstersSlain[n] == ' ')
                                continue;

                            buffer[5, n + WindowWidth / 2 - monstersSlain.Length / 2 - 8].ASCIIChar = (byte)monstersSlain[n];

                            if (n < 15)
                                buffer[5, n + WindowWidth / 2 - monstersSlain.Length / 2 - 8].Color = 11;
                            else
                                buffer[5, n + WindowWidth / 2 - monstersSlain.Length / 2 - 8].Color = 15;

                        }

                        // Отображение Имени и Уровня игрока
                        string playerInfo = $"   {player.Name}";

                        for (int n = 0; n < playerInfo.Length; n++)
                        {
                            if (playerInfo[n] == ' ')
                                continue;

                            buffer[WindowHeight - 8, WindowWidth / 6 + n].ASCIIChar = (byte)playerInfo[n];
                            buffer[WindowHeight - 8, WindowWidth / 6 + n].Color = 11;
                        }

                        playerInfo = $"lvl <{player.Lvl}>   exp " + $"{player.Exp:f2}%".Replace(',', '.').PadLeft(7);

                        for (int n = 0; n < playerInfo.Length; n++)
                        {
                            if (playerInfo[n] == ' ')
                                continue;

                            buffer[WindowHeight - 3, WindowWidth / 6 + n].ASCIIChar = (byte)playerInfo[n];
                            buffer[WindowHeight - 3, WindowWidth / 6 + n].Color = 11;
                        }

                        // Отображение щита
                        char[,] shieldPoint =
                            { {'/',  '#',  '\\' },
                              {'#',  '#',  '#'  },
                              {'\\', '#',  '/' }};

                        for (int n = 0; n < player.MaxShieldPoints; n++)
                        {
                            for (int i = 0; i < shieldPoint.GetLength(0); i++)
                            {
                                for (int j = 0; j < shieldPoint.GetLength(1); j++)
                                {
                                    if (shieldPoint[i, j] == '#' && n < player.ShieldPoints)
                                    {
                                        buffer[WindowHeight - 9 + i, n * 6 + WindowWidth / 2 - (player.MaxShieldPoints * 6) / 2 + j + 2].ASCIIChar = 32;
                                        buffer[WindowHeight - 9 + i, n * 6 + WindowWidth / 2 - (player.MaxShieldPoints * 6) / 2 + j + 2].Color = 0 | 14 << 4;
                                    }
                                    else if (shieldPoint[i, j] == '#')
                                    {
                                        buffer[WindowHeight - 9 + i, n * 6 + WindowWidth / 2 - (player.MaxShieldPoints * 6) / 2 + j + 2].ASCIIChar = 32;
                                        buffer[WindowHeight - 9 + i, n * 6 + WindowWidth / 2 - (player.MaxShieldPoints * 6) / 2 + j + 2].Color = 0 | 5 << 4;
                                    }
                                    else
                                    {
                                        buffer[WindowHeight - 9 + i, n * 6 + WindowWidth / 2 - (player.MaxShieldPoints * 6) / 2 + j + 2].ASCIIChar = (byte)shieldPoint[i, j];
                                        buffer[WindowHeight - 9 + i, n * 6 + WindowWidth / 2 - (player.MaxShieldPoints * 6) / 2 + j + 2].Color = 11;
                                    }
                                }
                            }
                        }

                        string shieldRegenTemp = $"+{player.ShieldRegen:f2}/s".Replace(',', '.').PadLeft(5);

                        for (int n = 0; n < shieldRegenTemp.Length; n++)
                        {
                            if (shieldRegenTemp[n] == ' ')
                                continue;

                            buffer[WindowHeight - 8, n + player.MaxShieldPoints * 6 + WindowWidth / 2 - (player.MaxShieldPoints * 6) / 2 + 2].ASCIIChar = (byte)shieldRegenTemp[n];
                            buffer[WindowHeight - 8, n + player.MaxShieldPoints * 6 + WindowWidth / 2 - (player.MaxShieldPoints * 6) / 2 + 2].Color = 14;
                        }

                        string shieldLabel = "Shield:";

                        for (int n = 0; n < shieldLabel.Length; n++)
                        {
                            if (shieldLabel[n] == ' ')
                                continue;

                            buffer[WindowHeight - 8, n + WindowWidth / 2 - player.MaxShieldPoints / 2 * 6 - shieldLabel.Length - 4].ASCIIChar = (byte)shieldLabel[n];
                            buffer[WindowHeight - 8, n + WindowWidth / 2 - player.MaxShieldPoints / 2 * 6 - shieldLabel.Length - 4].Color = 11;
                        }

                        // отображение здоровья
                        int hpBarLength = WindowWidth / 3;
                        string hpTemp = $"{player.HealthPoints:f2} / {player.MaxHealthPoints:f2}".Replace(',', '.').PadLeft($"{player.MaxHealthPoints:f2} / {player.MaxHealthPoints:f2}".Length);
                        string hpRegenTemp = $"+{player.MaxHealthPoints * player.HealthRegen / 100d:f2}/s".Replace(',', '.').PadLeft(5);

                        for (int n = -1; n <= hpBarLength; n++)
                        {
                            if (n == -1)
                            {
                                buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = (byte)'<';
                                buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].Color = 11;
                            }
                            else if (n == 0)
                            {
                                buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = (byte)'/';
                                buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].Color = 11;

                                if (n / (double)(WindowWidth / 3) * 100 < player.HealthPoints / player.MaxHealthPoints * 100 || player.HealthPoints != 0)
                                {
                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 13 << 4;
                                }
                                else
                                {
                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 5 << 4;
                                }

                                buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = (byte)'\\';
                                buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].Color = 11;
                            }
                            else if (n == hpBarLength - 1)
                            {
                                buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = (byte)'\\';
                                buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].Color = 11;

                                if (n / (double)(WindowWidth / 3) * 100 < player.HealthPoints / player.MaxHealthPoints * 100 && player.HealthPoints == player.MaxHealthPoints)
                                {
                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 13 << 4;
                                }
                                else
                                {
                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 5 << 4;
                                }

                                buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = (byte)'/';
                                buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].Color = 11;
                            }
                            else if (n == hpBarLength)
                            {
                                buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = (byte)'>';
                                buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].Color = 11;
                            }
                            else if (n >= hpBarLength / 2 - hpTemp.Length / 2 && n <= hpBarLength / 2 + hpTemp.Length / 2)
                            {
                                if (n / (double)(WindowWidth / 3) * 100 < player.HealthPoints / player.MaxHealthPoints * 100)
                                {
                                    buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                    buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 13 << 4;

                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = (byte)hpTemp[n - (hpBarLength / 2 - hpTemp.Length / 2)];
                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 13 << 4;

                                    buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                    buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 13 << 4;
                                }
                                else
                                {
                                    buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                    buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 5 << 4;

                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = (byte)hpTemp[n - (hpBarLength / 2 - hpTemp.Length / 2)];
                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 5 << 4;

                                    buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                    buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 5 << 4;
                                }
                            }
                            else if (n >= (hpBarLength - hpRegenTemp.Length) - 2 && n < hpBarLength - 2)
                            {
                                if (n / (double)(WindowWidth / 3) * 100 < player.HealthPoints / player.MaxHealthPoints * 100)
                                {
                                    buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                    buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 13 << 4;

                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = (byte)hpRegenTemp[n - (hpBarLength - hpRegenTemp.Length) + 2];
                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 13 << 4;

                                    buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                    buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 13 << 4;
                                }
                                else
                                {
                                    buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                    buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 5 << 4;

                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = (byte)hpRegenTemp[n - (hpBarLength - hpRegenTemp.Length) + 2];
                                    buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 5 << 4;

                                    buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                    buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 5 << 4;
                                }
                            }
                            else if (n / (double)(WindowWidth / 3) * 100 < player.HealthPoints / player.MaxHealthPoints * 100)
                            {
                                buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 13 << 4;
                                buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 13 << 4;
                                buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 13 << 4;
                            }
                            else
                            {
                                buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                buffer[WindowHeight - 4, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 5 << 4;
                                buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 5 << 4;
                                buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].ASCIIChar = 32;
                                buffer[WindowHeight - 2, n + WindowWidth / 2 - hpBarLength / 2].Color = 0 | 5 << 4;
                            }
                        }

                        string healthLabel = "Health:";

                        for (int n = 0; n < healthLabel.Length; n++)
                        {
                            if (healthLabel[n] == ' ')
                                continue;

                            buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2 - healthLabel.Length - 4].ASCIIChar = (byte)healthLabel[n];
                            buffer[WindowHeight - 3, n + WindowWidth / 2 - hpBarLength / 2 - healthLabel.Length - 4].Color = 11;
                        }
                    }
                    OutputInterface = (WindowCell[,])buffer.Clone();
                }
                catch (Exception ex) { try { File.AppendAllText("log.txt", DateTime.Now + "\nОшибка при отрисовке интерфейса:\n" + ex.ToString() + "\n\n\n", Encoding.UTF8); } catch { } }

                // Рассчёт FPS
                if ((DateTime.Now - fpsLimitStartingTime).TotalSeconds >= 0.5)
                {
                    if (frameCounter > 0)
                        FPSRenderInterface = frameCounter * 2;
                    else
                        FPSRenderInterface = 0;
                    frameCounter = 0;
                    fpsLimitStartingTime = DateTime.Now;
                }
                else frameCounter++;

                // Ограничение FPS
                DateTime frameFinishingTime = DateTime.Now;
                if ((frameFinishingTime - frameStartingTime).TotalMilliseconds < 1000 / FpsLimit)
                    Thread.Sleep((int)(1000 / FpsLimit - (frameFinishingTime - frameStartingTime).TotalMilliseconds));
            }
        }
        static void RenderLevel()
        {
            DateTime fpsLimitStartingTime = DateTime.Now;
            int frameCounter = 0;

            while (true)
            {
                DateTime frameStartingTime = DateTime.Now;

                // Запуск метода отрисовки в зависимости от выбранного масштаба
                switch (RenderScale)
                {
                    case 1:
                        {
                            RenderLevelSmall();
                            break;
                        }
                    case 3:
                        {
                            RenderLevelBig();
                            break;
                        }
                    default:
                        {
                            RenderLevelBig();
                            break;
                        }
                }

                // Рассчёт FPS
                if ((DateTime.Now - fpsLimitStartingTime).TotalSeconds >= 0.5)
                {
                    if (frameCounter > 0)
                        FPSRenderLevel = frameCounter * 2;
                    else
                        FPSRenderLevel = 0;
                    frameCounter = 0;
                    fpsLimitStartingTime = DateTime.Now;
                }
                else frameCounter++;

                // Ограничение FPS
                DateTime frameFinishingTime = DateTime.Now;
                if ((frameFinishingTime - frameStartingTime).TotalMilliseconds < 1000 / FpsLimit)
                    Thread.Sleep((int)(1000 / FpsLimit - (frameFinishingTime - frameStartingTime).TotalMilliseconds));
            }
        }
        static void RenderLevelSmall()
        {
            int tempPlayerX = 0;
            int tempPlayerY = 0;

            try
            {
                if (tempPlayerX != player.X || tempPlayerY != player.Y)
                {
                    tempPlayerX = player.X;
                    tempPlayerY = player.Y;
                }

                WindowCell[,] buffer = new WindowCell[OutputLevelHeight, OutputLevelWidth];

                WindowCell shade;
                WindowCell floorShade;

                int halfFH = OutputLevelHeight / 2;
                int halfFW = OutputLevelWidth / 2;
                double halfFHd = OutputLevelHeight / 2d;
                double halfFWd = OutputLevelWidth / 2d;

                int pauseDarken = Pause ? 3 : 0;

                for (int i = 0; i < OutputLevelHeight; i++)
                    for (int j = 0; j < OutputLevelWidth; j++)
                    {
                        if (i + tempPlayerY - halfFH < 0 || i + tempPlayerY - halfFH >= map.MapHeight || j + tempPlayerX - halfFW < 0 || j + tempPlayerX - halfFW >= map.MapWidth) // За краем карты
                        {
                            shade.ASCIIChar = 197; // ┼
                            floorShade.ASCIIChar = 32;

                            shade.Color = 0;
                        }
                        else
                        {
                            if (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Seen)
                            {
                                if (map.InSightMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].InSight) // В поле зрения
                                {
                                    shade.ASCIIChar = 43; // +
                                    floorShade.ASCIIChar = 250; // ·

                                    shade.Color = (byte)(10 - (map.InSightMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Distance / (double)player.ViewDistance * 7));
                                }
                                else // Вне поля зрения, но было исследовано
                                {
                                    shade.ASCIIChar = 43; // +
                                    floorShade.ASCIIChar = 250; // ·

                                    if ((((j - halfFWd) * (j - halfFWd)) / (halfFWd * halfFWd)) + (((i - halfFHd) * (i - halfFHd)) / (halfFHd * halfFHd)) > 0.9)
                                        shade.Color = 0;
                                    else if ((((j - halfFWd) * (j - halfFWd)) / (halfFWd * halfFWd)) + (((i - halfFHd) * (i - halfFHd)) / (halfFHd * halfFHd)) > 0.8)
                                        shade.Color = 1;
                                    else if ((((j - halfFWd) * (j - halfFWd)) / (halfFWd * halfFWd)) + (((i - halfFHd) * (i - halfFHd)) / (halfFHd * halfFHd)) > 0.7)
                                        shade.Color = 2;
                                    else
                                        shade.Color = 3;
                                }
                            }
                            else // Не было исследовано
                            {
                                shade.ASCIIChar = 43; // +
                                floorShade.ASCIIChar = 32;

                                shade.Color = TrueSight ? (short)1 : (short)0;
                            }

                            if (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1) // Поиск соседей и сглаживание блоков
                            {
                                int neighbors = 0;
                                bool[] n = new bool[8];

                                if (i - 1 + tempPlayerY - halfFH < 0 || i - 1 + tempPlayerY - halfFH >= map.MapHeight || j - 1 + tempPlayerX - halfFW < 0 || j - 1 + tempPlayerX - halfFW >= map.MapWidth) { n[0] = true; neighbors++; }
                                else if (map.LevelMap[i - 1 + tempPlayerY - halfFH, j - 1 + tempPlayerX - halfFW].Type == 1) { n[0] = true; neighbors++; }

                                if (i - 1 + tempPlayerY - halfFH < 0 || i - 1 + tempPlayerY - halfFH >= map.MapHeight || j + tempPlayerX - halfFW < 0 || j + tempPlayerX - halfFW >= map.MapWidth) { n[1] = true; neighbors++; }
                                else if (map.LevelMap[i - 1 + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1) { n[1] = true; neighbors++; }

                                if (i - 1 + tempPlayerY - halfFH < 0 || i - 1 + tempPlayerY - halfFH >= map.MapHeight || j + 1 + tempPlayerX - halfFW < 0 || j + 1 + tempPlayerX - halfFW >= map.MapWidth) { n[2] = true; neighbors++; }
                                else if (map.LevelMap[i - 1 + tempPlayerY - halfFH, j + 1 + tempPlayerX - halfFW].Type == 1) { n[2] = true; neighbors++; }

                                if (i + tempPlayerY - halfFH < 0 || i + tempPlayerY - halfFH >= map.MapHeight || j + 1 + tempPlayerX - halfFW < 0 || j + 1 + tempPlayerX - halfFW >= map.MapWidth) { n[3] = true; neighbors++; }
                                else if (map.LevelMap[i + tempPlayerY - halfFH, j + 1 + tempPlayerX - halfFW].Type == 1) { n[3] = true; neighbors++; }

                                if (i + 1 + tempPlayerY - halfFH < 0 || i + 1 + tempPlayerY - halfFH >= map.MapHeight || j + 1 + tempPlayerX - halfFW < 0 || j + 1 + tempPlayerX - halfFW >= map.MapWidth) { n[4] = true; neighbors++; }
                                else if (map.LevelMap[i + 1 + tempPlayerY - halfFH, j + 1 + tempPlayerX - halfFW].Type == 1) { n[4] = true; neighbors++; }

                                if (i + 1 + tempPlayerY - halfFH < 0 || i + 1 + tempPlayerY - halfFH >= map.MapHeight || j + tempPlayerX - halfFW < 0 || j + tempPlayerX - halfFW >= map.MapWidth) { n[5] = true; neighbors++; }
                                else if (map.LevelMap[i + 1 + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1) { n[5] = true; neighbors++; }

                                if (i + 1 + tempPlayerY - halfFH < 0 || i + 1 + tempPlayerY - halfFH >= map.MapHeight || j - 1 + tempPlayerX - halfFW < 0 || j - 1 + tempPlayerX - halfFW >= map.MapWidth) { n[6] = true; neighbors++; }
                                else if (map.LevelMap[i + 1 + tempPlayerY - halfFH, j - 1 + tempPlayerX - halfFW].Type == 1) { n[6] = true; neighbors++; }

                                if (i + tempPlayerY - halfFH < 0 || i + tempPlayerY - halfFH >= map.MapHeight || j - 1 + tempPlayerX - halfFW < 0 || j - 1 + tempPlayerX - halfFW >= map.MapWidth) { n[7] = true; neighbors++; }
                                else if (map.LevelMap[i + tempPlayerY - halfFH, j - 1 + tempPlayerX - halfFW].Type == 1) { n[7] = true; neighbors++; }

                                shade.ASCIIChar = RenderCell(neighbors, n);
                            }
                        }

                        if (i + tempPlayerY - halfFH < 0 || i + tempPlayerY - halfFH >= map.MapHeight || j + tempPlayerX - halfFW < 0 || j + tempPlayerX - halfFW >= map.MapWidth) // За краем карты
                        {
                            buffer[i, j].ASCIIChar = shade.ASCIIChar;
                            buffer[i, j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                        }
                        else // Не за краем карты
                        {
                            if (i == halfFH && j == halfFW)
                            {
                                buffer[i, j].ASCIIChar = (byte)player.Icon;
                                buffer[i, j].Color = (short)(11 - pauseDarken >= 0 ? 11 - pauseDarken : 0);
                            }
                            else if (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].MonsterID != -1 && map.InSightMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].InSight)
                            {
                                buffer[i, j].ASCIIChar = (byte)monsters[map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].MonsterID].Icon;
                                buffer[i, j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                            }
                            else if (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].DeadMonsterID != -1 && map.InSightMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].InSight)
                            {
                                buffer[i, j].ASCIIChar = (byte)monsters[map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].DeadMonsterID].Icon;
                                buffer[i, j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                            }
                            else
                            {
                                buffer[i, j].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shade.ASCIIChar : floorShade.ASCIIChar);
                                buffer[i, j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                            }
                        }
                    }

                OutputLevel = (WindowCell[,])buffer.Clone();
            }
            catch (Exception ex) { try { File.AppendAllText("log.txt", DateTime.Now + "\nОшибка при отрисовке уровня:\n" + ex.ToString() + "\n\n\n", Encoding.UTF8); } catch { } }
        }
        static void RenderLevelBig()
        {
            int tempPlayerX = 0;
            int tempPlayerY = 0;

            try
            {
                if (tempPlayerX != player.X || tempPlayerY != player.Y)
                {
                    tempPlayerX = player.X;
                    tempPlayerY = player.Y;
                }

                WindowCell[,] buffer = new WindowCell[OutputLevelHeight, OutputLevelWidth];

                WindowCell shade;
                WindowCell floorShade;

                WindowCell[] shadeBig = new WindowCell[9];
                WindowCell[] floorShadeBig = new WindowCell[9];

                int outputHeightScaled = OutputLevelHeight / 3;
                int outputWidthScaled = OutputLevelWidth / 3;

                int halfFH = outputHeightScaled / 2;
                int halfFW = outputWidthScaled / 2;
                double halfFHd = outputHeightScaled / 2d;
                double halfFWd = outputWidthScaled / 2d;

                int pauseDarken = Pause ? 3 : 0;

                for (int i = 0; i < outputHeightScaled; i++)
                    for (int j = 0; j < outputWidthScaled; j++)
                    {
                        if (i + tempPlayerY - halfFH < 0 || i + tempPlayerY - halfFH >= map.MapHeight || j + tempPlayerX - halfFW < 0 || j + tempPlayerX - halfFW >= map.MapWidth) // За краем карты
                        {
                            shade.ASCIIChar = 197; // ┼
                            floorShade.ASCIIChar = 32;

                            shadeBig[0].ASCIIChar = 197; // ┼
                            shadeBig[1].ASCIIChar = 197;
                            shadeBig[2].ASCIIChar = 197;
                            shadeBig[3].ASCIIChar = 197;
                            shadeBig[4].ASCIIChar = 197;
                            shadeBig[5].ASCIIChar = 197;
                            shadeBig[6].ASCIIChar = 197;
                            shadeBig[7].ASCIIChar = 197;
                            shadeBig[8].ASCIIChar = 197;
                            floorShadeBig[0].ASCIIChar = 32;
                            floorShadeBig[1].ASCIIChar = 32;
                            floorShadeBig[2].ASCIIChar = 32;
                            floorShadeBig[3].ASCIIChar = 32;
                            floorShadeBig[4].ASCIIChar = 32;
                            floorShadeBig[5].ASCIIChar = 32;
                            floorShadeBig[6].ASCIIChar = 32;
                            floorShadeBig[7].ASCIIChar = 32;
                            floorShadeBig[8].ASCIIChar = 32;

                            shade.Color = 0;
                        }
                        else
                        {
                            if (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Seen)
                            {
                                if (map.InSightMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].InSight) // В поле зрения
                                {
                                    shade.ASCIIChar = 43; // +
                                    floorShade.ASCIIChar = 250; // ·

                                    shadeBig[0].ASCIIChar = 43; // +
                                    shadeBig[1].ASCIIChar = 43;
                                    shadeBig[2].ASCIIChar = 43;
                                    shadeBig[3].ASCIIChar = 43;
                                    shadeBig[4].ASCIIChar = 43;
                                    shadeBig[5].ASCIIChar = 43;
                                    shadeBig[6].ASCIIChar = 43;
                                    shadeBig[7].ASCIIChar = 43;
                                    shadeBig[8].ASCIIChar = 43;
                                    floorShadeBig[0].ASCIIChar = 250; // ·
                                    floorShadeBig[1].ASCIIChar = 250;
                                    floorShadeBig[2].ASCIIChar = 250;
                                    floorShadeBig[3].ASCIIChar = 250;
                                    floorShadeBig[4].ASCIIChar = 250;
                                    floorShadeBig[5].ASCIIChar = 250;
                                    floorShadeBig[6].ASCIIChar = 250;
                                    floorShadeBig[7].ASCIIChar = 250;
                                    floorShadeBig[8].ASCIIChar = 250;

                                    shade.Color = (byte)(10 - (map.InSightMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Distance / (double)player.ViewDistance * 7));
                                }
                                else // Вне поля зрения, но было исследовано
                                {
                                    shade.ASCIIChar = 43; // +
                                    floorShade.ASCIIChar = 250; // ·

                                    shadeBig[0].ASCIIChar = 43; // +
                                    shadeBig[1].ASCIIChar = 43;
                                    shadeBig[2].ASCIIChar = 43;
                                    shadeBig[3].ASCIIChar = 43;
                                    shadeBig[4].ASCIIChar = 43;
                                    shadeBig[5].ASCIIChar = 43;
                                    shadeBig[6].ASCIIChar = 43;
                                    shadeBig[7].ASCIIChar = 43;
                                    shadeBig[8].ASCIIChar = 43;
                                    floorShadeBig[0].ASCIIChar = 250; // ·
                                    floorShadeBig[1].ASCIIChar = 250;
                                    floorShadeBig[2].ASCIIChar = 250;
                                    floorShadeBig[3].ASCIIChar = 250;
                                    floorShadeBig[4].ASCIIChar = 250;
                                    floorShadeBig[5].ASCIIChar = 250;
                                    floorShadeBig[6].ASCIIChar = 250;
                                    floorShadeBig[7].ASCIIChar = 250;
                                    floorShadeBig[8].ASCIIChar = 250;

                                    if ((((j - halfFWd) * (j - halfFWd)) / (halfFWd * halfFWd)) + (((i - halfFHd) * (i - halfFHd)) / (halfFHd * halfFHd)) > 0.9)
                                        shade.Color = 0;
                                    else if ((((j - halfFWd) * (j - halfFWd)) / (halfFWd * halfFWd)) + (((i - halfFHd) * (i - halfFHd)) / (halfFHd * halfFHd)) > 0.8)
                                        shade.Color = 1;
                                    else if ((((j - halfFWd) * (j - halfFWd)) / (halfFWd * halfFWd)) + (((i - halfFHd) * (i - halfFHd)) / (halfFHd * halfFHd)) > 0.7)
                                        shade.Color = 2;
                                    else
                                        shade.Color = 3;
                                }
                            }
                            else // Не было исследовано
                            {
                                shade.ASCIIChar = 43; // +
                                floorShade.ASCIIChar = 32;

                                shadeBig[0].ASCIIChar = 43; // +
                                shadeBig[1].ASCIIChar = 43;
                                shadeBig[2].ASCIIChar = 43;
                                shadeBig[3].ASCIIChar = 43;
                                shadeBig[4].ASCIIChar = 43;
                                shadeBig[5].ASCIIChar = 43;
                                shadeBig[6].ASCIIChar = 43;
                                shadeBig[7].ASCIIChar = 43;
                                shadeBig[8].ASCIIChar = 43;
                                floorShadeBig[0].ASCIIChar = 32;
                                floorShadeBig[1].ASCIIChar = 32;
                                floorShadeBig[2].ASCIIChar = 32;
                                floorShadeBig[3].ASCIIChar = 32;
                                floorShadeBig[4].ASCIIChar = 32;
                                floorShadeBig[5].ASCIIChar = 32;
                                floorShadeBig[6].ASCIIChar = 32;
                                floorShadeBig[7].ASCIIChar = 32;
                                floorShadeBig[8].ASCIIChar = 32;

                                shade.Color = TrueSight ? (short)1 : (short)0;
                            }

                            if (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1) // Поиск соседей и сглаживание блоков
                            {
                                int neighbors = 0;
                                bool[] n = new bool[8];

                                if (i - 1 + tempPlayerY - halfFH < 0 || i - 1 + tempPlayerY - halfFH >= map.MapHeight || j - 1 + tempPlayerX - halfFW < 0 || j - 1 + tempPlayerX - halfFW >= map.MapWidth) { n[0] = true; neighbors++; }
                                else if (map.LevelMap[i - 1 + tempPlayerY - halfFH, j - 1 + tempPlayerX - halfFW].Type == 1) { n[0] = true; neighbors++; }

                                if (i - 1 + tempPlayerY - halfFH < 0 || i - 1 + tempPlayerY - halfFH >= map.MapHeight || j + tempPlayerX - halfFW < 0 || j + tempPlayerX - halfFW >= map.MapWidth) { n[1] = true; neighbors++; }
                                else if (map.LevelMap[i - 1 + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1) { n[1] = true; neighbors++; }

                                if (i - 1 + tempPlayerY - halfFH < 0 || i - 1 + tempPlayerY - halfFH >= map.MapHeight || j + 1 + tempPlayerX - halfFW < 0 || j + 1 + tempPlayerX - halfFW >= map.MapWidth) { n[2] = true; neighbors++; }
                                else if (map.LevelMap[i - 1 + tempPlayerY - halfFH, j + 1 + tempPlayerX - halfFW].Type == 1) { n[2] = true; neighbors++; }

                                if (i + tempPlayerY - halfFH < 0 || i + tempPlayerY - halfFH >= map.MapHeight || j + 1 + tempPlayerX - halfFW < 0 || j + 1 + tempPlayerX - halfFW >= map.MapWidth) { n[3] = true; neighbors++; }
                                else if (map.LevelMap[i + tempPlayerY - halfFH, j + 1 + tempPlayerX - halfFW].Type == 1) { n[3] = true; neighbors++; }

                                if (i + 1 + tempPlayerY - halfFH < 0 || i + 1 + tempPlayerY - halfFH >= map.MapHeight || j + 1 + tempPlayerX - halfFW < 0 || j + 1 + tempPlayerX - halfFW >= map.MapWidth) { n[4] = true; neighbors++; }
                                else if (map.LevelMap[i + 1 + tempPlayerY - halfFH, j + 1 + tempPlayerX - halfFW].Type == 1) { n[4] = true; neighbors++; }

                                if (i + 1 + tempPlayerY - halfFH < 0 || i + 1 + tempPlayerY - halfFH >= map.MapHeight || j + tempPlayerX - halfFW < 0 || j + tempPlayerX - halfFW >= map.MapWidth) { n[5] = true; neighbors++; }
                                else if (map.LevelMap[i + 1 + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1) { n[5] = true; neighbors++; }

                                if (i + 1 + tempPlayerY - halfFH < 0 || i + 1 + tempPlayerY - halfFH >= map.MapHeight || j - 1 + tempPlayerX - halfFW < 0 || j - 1 + tempPlayerX - halfFW >= map.MapWidth) { n[6] = true; neighbors++; }
                                else if (map.LevelMap[i + 1 + tempPlayerY - halfFH, j - 1 + tempPlayerX - halfFW].Type == 1) { n[6] = true; neighbors++; }

                                if (i + tempPlayerY - halfFH < 0 || i + tempPlayerY - halfFH >= map.MapHeight || j - 1 + tempPlayerX - halfFW < 0 || j - 1 + tempPlayerX - halfFW >= map.MapWidth) { n[7] = true; neighbors++; }
                                else if (map.LevelMap[i + tempPlayerY - halfFH, j - 1 + tempPlayerX - halfFW].Type == 1) { n[7] = true; neighbors++; }

                                shade.ASCIIChar = RenderCell(neighbors, n);

                                byte[] tempShadeBig = RenderCellBig(neighbors, n);

                                shadeBig[0].ASCIIChar = tempShadeBig[0];
                                shadeBig[1].ASCIIChar = tempShadeBig[1];
                                shadeBig[2].ASCIIChar = tempShadeBig[2];
                                shadeBig[3].ASCIIChar = tempShadeBig[3];
                                shadeBig[4].ASCIIChar = tempShadeBig[4];
                                shadeBig[5].ASCIIChar = tempShadeBig[5];
                                shadeBig[6].ASCIIChar = tempShadeBig[6];
                                shadeBig[7].ASCIIChar = tempShadeBig[7];
                                shadeBig[8].ASCIIChar = tempShadeBig[8];
                            }
                        }

                        if (i + tempPlayerY - halfFH < 0 || i + tempPlayerY - halfFH >= map.MapHeight || j + tempPlayerX - halfFW < 0 || j + tempPlayerX - halfFW >= map.MapWidth) // За краем карты
                        {
                            buffer[i, j].ASCIIChar = shade.ASCIIChar;
                            buffer[i, j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                        }
                        else // Не за краем карты
                        {
                            if (i == halfFH && j == halfFW)
                            { // Иконка игрока
                                buffer[3 * i, 3 * j].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[0].ASCIIChar : floorShadeBig[0].ASCIIChar);
                                buffer[3 * i, 3 * j + 1].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[1].ASCIIChar : floorShadeBig[1].ASCIIChar);
                                buffer[3 * i, 3 * j + 2].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[2].ASCIIChar : floorShadeBig[2].ASCIIChar);
                                buffer[3 * i + 1, 3 * j].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[3].ASCIIChar : floorShadeBig[3].ASCIIChar);
                                buffer[3 * i + 1, 3 * j + 1].ASCIIChar = (byte)player.Icon;
                                buffer[3 * i + 1, 3 * j + 2].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[5].ASCIIChar : floorShadeBig[5].ASCIIChar);
                                buffer[3 * i + 2, 3 * j].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[6].ASCIIChar : floorShadeBig[6].ASCIIChar);
                                buffer[3 * i + 2, 3 * j + 1].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[7].ASCIIChar : floorShadeBig[7].ASCIIChar);
                                buffer[3 * i + 2, 3 * j + 2].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[8].ASCIIChar : floorShadeBig[8].ASCIIChar);
                                buffer[3 * i, 3 * j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i, 3 * j + 1].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i, 3 * j + 2].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 1, 3 * j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 1, 3 * j + 1].Color = (short)(11 - pauseDarken >= 0 ? 11 - pauseDarken : 0);
                                buffer[3 * i + 1, 3 * j + 2].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 2, 3 * j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 2, 3 * j + 1].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 2, 3 * j + 2].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                            }
                            else if (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].MonsterID != -1 && map.InSightMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].InSight)
                            { // Монстры
                                buffer[3 * i, 3 * j].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[0].ASCIIChar : floorShadeBig[0].ASCIIChar);
                                buffer[3 * i, 3 * j + 1].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[1].ASCIIChar : floorShadeBig[1].ASCIIChar);
                                buffer[3 * i, 3 * j + 2].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[2].ASCIIChar : floorShadeBig[2].ASCIIChar);
                                buffer[3 * i + 1, 3 * j].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[3].ASCIIChar : floorShadeBig[3].ASCIIChar);
                                buffer[3 * i + 1, 3 * j + 1].ASCIIChar = (byte)monsters[map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].MonsterID].Icon;
                                buffer[3 * i + 1, 3 * j + 2].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[5].ASCIIChar : floorShadeBig[5].ASCIIChar);
                                buffer[3 * i + 2, 3 * j].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[6].ASCIIChar : floorShadeBig[6].ASCIIChar);
                                buffer[3 * i + 2, 3 * j + 1].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[7].ASCIIChar : floorShadeBig[7].ASCIIChar);
                                buffer[3 * i + 2, 3 * j + 2].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[8].ASCIIChar : floorShadeBig[8].ASCIIChar);
                                buffer[3 * i, 3 * j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i, 3 * j + 1].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i, 3 * j + 2].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 1, 3 * j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 1, 3 * j + 1].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 1, 3 * j + 2].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 2, 3 * j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 2, 3 * j + 1].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 2, 3 * j + 2].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                            }
                            else if (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].DeadMonsterID != -1 && map.InSightMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].InSight)
                            { // Мёртвые монстры
                                buffer[3 * i, 3 * j].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[0].ASCIIChar : floorShadeBig[0].ASCIIChar);
                                buffer[3 * i, 3 * j + 1].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[1].ASCIIChar : floorShadeBig[1].ASCIIChar);
                                buffer[3 * i, 3 * j + 2].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[2].ASCIIChar : floorShadeBig[2].ASCIIChar);
                                buffer[3 * i + 1, 3 * j].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[3].ASCIIChar : floorShadeBig[3].ASCIIChar);
                                buffer[3 * i + 1, 3 * j + 1].ASCIIChar = (byte)monsters[map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].DeadMonsterID].Icon;
                                buffer[3 * i + 1, 3 * j + 2].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[5].ASCIIChar : floorShadeBig[5].ASCIIChar);
                                buffer[3 * i + 2, 3 * j].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[6].ASCIIChar : floorShadeBig[6].ASCIIChar);
                                buffer[3 * i + 2, 3 * j + 1].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[7].ASCIIChar : floorShadeBig[7].ASCIIChar);
                                buffer[3 * i + 2, 3 * j + 2].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[8].ASCIIChar : floorShadeBig[8].ASCIIChar);
                                buffer[3 * i, 3 * j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i, 3 * j + 1].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i, 3 * j + 2].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 1, 3 * j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 1, 3 * j + 1].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 1, 3 * j + 2].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 2, 3 * j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 2, 3 * j + 1].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 2, 3 * j + 2].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                            }
                            else
                            { // Карта
                                for (int k = 0; k < 9; k++) // Добавить пол под стены
                                    if (shadeBig[k].ASCIIChar == 32)
                                        shadeBig[k].ASCIIChar = floorShadeBig[k].ASCIIChar;

                                buffer[3 * i, 3 * j].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[0].ASCIIChar : floorShadeBig[0].ASCIIChar);
                                buffer[3 * i, 3 * j + 1].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[1].ASCIIChar : floorShadeBig[1].ASCIIChar);
                                buffer[3 * i, 3 * j + 2].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[2].ASCIIChar : floorShadeBig[2].ASCIIChar);
                                buffer[3 * i + 1, 3 * j].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[3].ASCIIChar : floorShadeBig[3].ASCIIChar);
                                buffer[3 * i + 1, 3 * j + 1].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[4].ASCIIChar : floorShadeBig[4].ASCIIChar);
                                buffer[3 * i + 1, 3 * j + 2].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[5].ASCIIChar : floorShadeBig[5].ASCIIChar);
                                buffer[3 * i + 2, 3 * j].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[6].ASCIIChar : floorShadeBig[6].ASCIIChar);
                                buffer[3 * i + 2, 3 * j + 1].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[7].ASCIIChar : floorShadeBig[7].ASCIIChar);
                                buffer[3 * i + 2, 3 * j + 2].ASCIIChar = (map.LevelMap[i + tempPlayerY - halfFH, j + tempPlayerX - halfFW].Type == 1 ? shadeBig[8].ASCIIChar : floorShadeBig[8].ASCIIChar);
                                buffer[3 * i, 3 * j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i, 3 * j + 1].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i, 3 * j + 2].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 1, 3 * j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 1, 3 * j + 1].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 1, 3 * j + 2].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 2, 3 * j].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 2, 3 * j + 1].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                                buffer[3 * i + 2, 3 * j + 2].Color = (short)(shade.Color - pauseDarken >= 0 ? shade.Color - pauseDarken : 0);
                            }
                        }
                    }

                OutputLevel = (WindowCell[,])buffer.Clone();
            }
            catch (Exception ex) { try { File.AppendAllText("log.txt", DateTime.Now + "\nОшибка при отрисовке уровня:\n" + ex.ToString() + "\n\n\n", Encoding.UTF8); } catch { } }
        }
        static byte RenderCell(int neighbors, bool[] n)
        {
            switch (neighbors)
            {
                case 0:
                    {
                        return 43; // +
                    }
                case 1:
                    {
                        if (n[0])
                            return 92; // \
                        else if (n[1])
                            return 179; // |
                        else if (n[2])
                            return 47; // /
                        else if (n[3])
                            return 196; // ─
                        else if (n[4])
                            return 92; // \
                        else if (n[5])
                            return 179; // |
                        else if (n[6])
                            return 47; // /
                        else if (n[7])
                            return 196; // ─
                        break;
                    }
                case 2:
                    {
                        if (n[0])
                        {
                            if (n[1])
                                return 118; // v
                            else if (n[2])
                                return 118; // v
                            else if (n[3])
                                return 92; // \
                            else if (n[4])
                                return 92; // \
                            else if (n[5])
                                return 92; // \
                            else if (n[6])
                                return 62; // >
                            else if (n[7])
                                return 62; // >
                        }
                        else if (n[1])
                        {
                            if (n[2])
                                return 118; // v
                            else if (n[3])
                                return 92; // \
                            else if (n[4])
                                return 92; // \
                            else if (n[5])
                                return 179; // |
                            else if (n[6])
                                return 47; // /
                            else if (n[7])
                                return 47; // /
                        }
                        else if (n[2])
                        {
                            if (n[3])
                                return 60; // <
                            else if (n[4])
                                return 60; // <
                            else if (n[5])
                                return 47; // /
                            else if (n[6])
                                return 47; // /
                            else if (n[7])
                                return 47; // /
                        }
                        else if (n[3])
                        {
                            if (n[4])
                                return 60; // <
                            else if (n[5])
                                return 47; // /
                            else if (n[6])
                                return 47; // /
                            else if (n[7])
                                return 196; // ─
                        }
                        else if (n[4])
                        {
                            if (n[5])
                                return 94; // ^
                            else if (n[6])
                                return 94; // ^
                            else if (n[7])
                                return 92; // \
                        }
                        else if (n[5])
                        {
                            if (n[6])
                                return 94; // ^
                            else if (n[7])
                                return 92; // \
                        }
                        else if (n[6])
                        {
                            if (n[7])
                                return 62; // >
                        }
                        break;
                    }
                case 3:
                    {
                        if (n[0])
                        {
                            if (n[1])
                            {
                                if (n[2])
                                    return 118; // v
                                else if (n[3])
                                    return 92; // \
                                else if (n[4])
                                    return 92; // \
                                else if (n[5])
                                    return 179; // |
                                else if (n[6])
                                    return 47; // /
                                else if (n[7])
                                    return 47; // /
                            }
                            else if (n[2])
                            {
                                if (n[3])
                                    return 92; // \
                                else if (n[4])
                                    return 92; // \
                                else if (n[5])
                                    return 194; // ┬
                                else if (n[6])
                                    return 47; // /
                                else if (n[7])
                                    return 47; // /
                            }
                            else if (n[3])
                            {
                                if (n[4])
                                    return 92; // \
                                else if (n[5])
                                    return 47; // /
                                else if (n[6])
                                    return 195; // ├
                                else if (n[7])
                                    return 196; // ─
                            }
                            else if (n[4])
                            {
                                if (n[5])
                                    return 92; // \
                                else if (n[6])
                                    return 92; // \
                                else if (n[7])
                                    return 92; // \
                            }
                            else if (n[5])
                            {
                                if (n[6])
                                    return 92; // \
                                else if (n[7])
                                    return 92; // \
                            }
                            else if (n[6])
                            {
                                if (n[7])
                                    return 62; // >
                            }
                        }
                        else if (n[1])
                        {
                            if (n[2])
                            {
                                if (n[3])
                                    return 92; // \
                                else if (n[4])
                                    return 92; // \
                                else if (n[5])
                                    return 179; // |
                                else if (n[6])
                                    return 47; // /
                                else if (n[7])
                                    return 47; // /
                            }
                            else if (n[3])
                            {
                                if (n[4])
                                    return 92; // \
                                else if (n[5])
                                    return 179; // |
                                else if (n[6])
                                    return 92; // \
                                else if (n[7])
                                    return 196; // ─
                            }
                            else if (n[4])
                            {
                                if (n[5])
                                    return 179; // |
                                else if (n[6])
                                    return 193; // ┴
                                else if (n[7])
                                    return 47; // /
                            }
                            else if (n[5])
                            {
                                if (n[6])
                                    return 179; // |
                                else if (n[7])
                                    return 179; // |
                            }
                            else if (n[6])
                            {
                                if (n[7])
                                    return 47; // /
                            }
                        }
                        else if (n[2])
                        {
                            if (n[3])
                            {
                                if (n[4])
                                    return 60; // <
                                else if (n[5])
                                    return 47; // /
                                else if (n[6])
                                    return 47; // /
                                else if (n[7])
                                    return 196; // ─
                            }
                            if (n[4])
                            {
                                if (n[5])
                                    return 47; // /
                                else if (n[6])
                                    return 47; // /
                                else if (n[7])
                                    return 180; // ┤
                            }
                            if (n[5])
                            {
                                if (n[6])
                                    return 47; // /
                                else if (n[7])
                                    return 92; // \
                            }
                            if (n[6])
                            {
                                if (n[7])
                                    return 47; // /
                            }
                        }
                        else if (n[3])
                        {
                            if (n[4])
                            {
                                if (n[5])
                                    return 47; // /
                                if (n[6])
                                    return 47; // /
                                else if (n[7])
                                    return 194; // ┬
                            }
                            if (n[5])
                            {
                                if (n[6])
                                    return 47; // /
                                else if (n[7])
                                    return 196; // ─
                            }
                            if (n[6])
                            {
                                if (n[7])
                                    return 47; // /
                            }
                        }
                        else if (n[4])
                        {
                            if (n[5])
                            {
                                if (n[6])
                                    return 94; // ^
                                else if (n[7])
                                    return 47; // /
                            }
                            if (n[6])
                            {
                                if (n[7])
                                    return 92; // \
                            }
                        }
                        else if (n[5])
                        {
                            if (n[6])
                            {
                                if (n[7])
                                    return 92; // \
                            }
                        }
                        break;
                    }
                case 4:
                    {
                        if (n[0])
                        {
                            if (n[1])
                            {
                                if (n[2])
                                {
                                    if (n[3])
                                        return 92; // \
                                    else if (n[4])
                                        return 118; // v
                                    else if (n[5])
                                        return 179; // │
                                    else if (n[6])
                                        return 118; // v
                                    else if (n[7])
                                        return 47; // /
                                }
                                else if (n[3])
                                {
                                    if (n[4])
                                        return 92; // \
                                    else if (n[5])
                                        return 179; // │
                                    else if (n[6])
                                        return 92; // \
                                    else if (n[7])
                                        return 196; // ─
                                }
                                else if (n[4])
                                {
                                    if (n[5])
                                        return 179; // │
                                    else if (n[6])
                                        return 196; // ─
                                    else if (n[7])
                                        return 47; // /
                                }
                                else if (n[5])
                                {
                                    if (n[6])
                                        return 179; // │
                                    else if (n[7])
                                        return 179; // │
                                }
                                else if (n[6])
                                {
                                    if (n[7])
                                        return 47; // /
                                }
                            }
                            else if (n[2])
                            {
                                if (n[3])
                                {
                                    if (n[4])
                                        return 60; // <
                                    else if (n[5])
                                        return 47; // /
                                    else if (n[6])
                                        return 179; // │
                                    else if (n[7])
                                        return 196; // ─
                                }
                                else if (n[4])
                                {
                                    if (n[5])
                                        return 196; // ─
                                    else if (n[6])
                                        return 88; // X
                                    else if (n[7])
                                        return 62; // >
                                }
                                else if (n[5])
                                {
                                    if (n[6])
                                        return 196; // ─
                                    else if (n[7])
                                        return 92; // \
                                }
                                else if (n[6])
                                {
                                    if (n[7])
                                        return 62; // >
                                }
                            }
                            else if (n[3])
                            {
                                if (n[4])
                                {
                                    if (n[5])
                                        return 47; // /
                                    else if (n[6])
                                        return 179; // │
                                    else if (n[7])
                                        return 196; // ─
                                }
                                else if (n[5])
                                {
                                    if (n[6])
                                        return 47; // /
                                    else if (n[7])
                                        return 196; // ─
                                }
                                else if (n[6])
                                {
                                    if (n[7])
                                        return 196; // ─
                                }
                            }
                            else if (n[4])
                            {
                                if (n[5])
                                {
                                    if (n[6])
                                        return 94; // ^
                                    else if (n[7])
                                        return 92; // \
                                }
                                else if (n[6])
                                {
                                    if (n[7])
                                        return 62; // >
                                }
                            }
                            else if (n[5])
                            {
                                if (n[6])
                                { 
                                    if (n[7])
                                        return 92; // \
                                }
                            }
                        }
                        else if (n[1])
                        {
                            if (n[2])
                            {
                                if (n[3])
                                {
                                    if (n[4])
                                        return 92; // \
                                    else if (n[5])
                                        return 179; // │
                                    else if (n[6])
                                        return 92; // \
                                    else if (n[7])
                                        return 196; // ─
                                }
                                else if (n[4])
                                {
                                    if (n[5])
                                        return 179; // │
                                    else if (n[6])
                                        return 196; // ─
                                    else if (n[7])
                                        return 47; // /
                                }
                                else if (n[5])
                                {
                                    if (n[6])
                                        return 179; // │
                                    else if (n[7])
                                        return 179; // │
                                }
                                else if (n[6])
                                {
                                    if (n[7])
                                        return 47; // /
                                }
                            }
                            else if (n[3])
                            {
                                if (n[4])
                                {
                                    if (n[5])
                                        return 179; // │
                                    else if (n[6])
                                        return 92; // \
                                    else if (n[7])
                                        return 92; // \
                                }
                                if (n[5])
                                {
                                    if (n[6])
                                        return 92; // \
                                    else if (n[7])
                                        return 197; // ┼
                                }
                                if (n[6])
                                {
                                    if (n[7])
                                        return 196; // ─
                                }
                            }
                            else if (n[4])
                            {
                                if (n[5])
                                {
                                    if (n[6])
                                        return 179; // │
                                    else if (n[7])
                                        return 92; // \
                                }
                                else if (n[6])
                                {
                                    if (n[7])
                                        return 47; // /
                                }
                            }
                            else if (n[5])
                            {
                                if (n[6])
                                { 
                                    if (n[7])
                                        return 179; // │
                                }
                            }
                        }
                        else if (n[2])
                        {
                            if (n[3])
                            {
                                if (n[4])
                                {
                                    if (n[5])
                                        return 47; // /
                                    else if (n[6])
                                        return 118; // v
                                    else if (n[7])
                                        return 196; // ─
                                }
                                else if (n[5])
                                {
                                    if (n[6])
                                        return 47; // /
                                    else if (n[7])
                                        return 196; // ─
                                }
                                else if (n[6])
                                {
                                    if (n[7])
                                        return 196; // ─
                                }
                            }
                            else if (n[4])
                            {
                                if (n[5])
                                {
                                    if (n[6])
                                        return 94; // ^
                                    else if (n[7])
                                        return 92; // \
                                }
                                else if (n[6])
                                {
                                    if (n[7])
                                        return 62; // >
                                }
                            }
                            else if (n[5])
                            {
                                if (n[6])
                                { 
                                    if (n[7])
                                        return 92; // \
                                }
                            }
                        }
                        else if (n[3])
                        {
                            if (n[4])
                            {
                                if (n[5])
                                {
                                    if (n[6])
                                        return 47; // /
                                    else if (n[7])
                                        return 196; // ─
                                }
                                else if (n[6])
                                {
                                    if (n[7])
                                        return 196; // ─
                                }
                            }
                            else if (n[5])
                            {
                                if (n[6])
                                {
                                    if (n[7])
                                        return 196; // ─
                                }
                            }
                        }
                        else if (n[4])
                        {
                            if (n[5])
                            {
                                if (n[6])
                                { 
                                    if (n[7])
                                        return 92; // \
                                }
                            }
                        }
                        break;
                    }
                case 5:
                    {
                        if (!n[0])
                        {
                            if (!n[1])
                            {
                                if (!n[2])
                                    return 196; // ─
                                else if (!n[3])
                                    return 92; // \
                                else if (!n[4])
                                    return 92; // \
                                else if (!n[5])
                                    return 196; // ─
                                else if (!n[6])
                                    return 47; // /
                                else if (!n[7])
                                    return 47; // /
                            }
                            else if (!n[2])
                            {
                                if (!n[3])
                                    return 92; // \
                                else if (!n[4])
                                    return 197; // ┼
                                else if (!n[5])
                                    return 94; // ^
                                else if (!n[6])
                                    return 197; // ┼
                                else if (!n[7])
                                    return 47; // /
                            }
                            else if (!n[3])
                            {
                                if (!n[4])
                                    return 47; // /
                                else if (!n[5])
                                    return 47; // /
                                else if (!n[6])
                                    return 60; // <
                                else if (!n[7])
                                    return 179; // |
                            }
                            else if (!n[4])
                            {
                                if (!n[5])
                                    return 47; // /
                                else if (!n[6])
                                    return 197; // ┼
                                else if (!n[7])
                                    return 47; // /
                            }
                            else if (!n[5])
                            {
                                if (!n[6])
                                    return 196; // ─
                                else if (!n[7])
                                    return 47; // /
                            }
                            else if (!n[6])
                            {
                                if (!n[7])
                                    return 179; // |
                            }
                        }
                        else if (!n[1])
                        {
                            if (!n[2])
                            {
                                if (!n[3])
                                    return 92; // \
                                else if (!n[4])
                                    return 196; // ─
                                else if (!n[5])
                                    return 196; // ─
                                else if (!n[6])
                                    return 92; // \
                                else if (!n[7])
                                    return 47; // /
                            }
                            else if (!n[3])
                            {
                                if (!n[4])
                                    return 92; // \
                                else if (!n[5])
                                    return 179; // |
                                else if (!n[6])
                                    return 92; // \
                                else if (!n[7])
                                    return 196; // ─
                            }
                            else if (!n[4])
                            {
                                if (!n[5])
                                    return 196; // ─
                                else if (!n[6])
                                    return 196; // ─
                                else if (!n[7])
                                    return 47; // /
                            }
                            else if (!n[5])
                            {
                                if (!n[6])
                                    return 196; // ─
                                else if (!n[7])
                                    return 179; // |
                            }
                            else if (!n[6])
                            {
                                if (!n[7])
                                    return 179; // |
                            }
                        }
                        else if (!n[2])
                        {
                            if (!n[3])
                            {
                                if (!n[4])
                                    return 179; // |
                                else if (!n[5])
                                    return 47; // /
                                else if (!n[6])
                                    return 92; // \
                                else if (!n[7])
                                    return 179; // |
                            }
                            if (!n[4])
                            {
                                if (!n[5])
                                    return 196; // ─
                                else if (!n[6])
                                    return 197; // ┼
                                else if (!n[7])
                                    return 179; // |
                            }
                            if (!n[5])
                            {
                                if (!n[6])
                                    return 92; // \
                                else if (!n[7])
                                    return 92; // \
                            }
                            if (!n[6])
                            {
                                if (!n[7])
                                    return 92; // \
                            }
                        }
                        else if (!n[3])
                        {
                            if (!n[4])
                            {
                                if (!n[5])
                                    return 47; // /
                                if (!n[6])
                                    return 47; // /
                                else if (!n[7])
                                    return 179; // |
                            }
                            if (!n[5])
                            {
                                if (!n[6])
                                    return 47; // /
                                else if (!n[7])
                                    return 196; // ─
                            }
                            if (!n[6])
                            {
                                if (!n[7])
                                    return 179; // |
                            }
                        }
                        else if (!n[4])
                        {
                            if (!n[5])
                            {
                                if (!n[6])
                                    return 196; // ─
                                else if (!n[7])
                                    return 92; // \
                            }
                            if (!n[6])
                            {
                                if (!n[7])
                                    return 179; // |
                            }
                        }
                        else if (!n[5])
                        {
                            if (!n[6])
                            {
                                if (!n[7])
                                    return 92; // \
                            }
                        }
                        break;
                    }
                case 6:
                    {
                        if (!n[0])
                        {
                            if (!n[1])
                                return 47; // /
                            else if (!n[2])
                                return 197; // ┼
                            else if (!n[3])
                                return 60; // <
                            else if (!n[4])
                                return 197; // ┼
                            else if (!n[5])
                                return 94; // ^
                            else if (!n[6])
                                return 197; // ┼
                            else if (!n[7])
                                return 47; // /
                        }
                        else if (!n[1])
                        {
                            if (!n[2])
                                return 92; // \
                            else if (!n[3])
                                return 92; // \
                            else if (!n[4])
                                return 47; // /
                            else if (!n[5])
                                return 196; // ─
                            else if (!n[6])
                                return 92; // \
                            else if (!n[7])
                                return 47; // /
                        }
                        else if (!n[2])
                        {
                            if (!n[3])
                                return 92; // \
                            else if (!n[4])
                                return 197; // ┼
                            else if (!n[5])
                                return 94; // ^
                            else if (!n[6])
                                return 197; // ┼
                            else if (!n[7])
                                return 62; // >
                        }
                        else if (!n[3])
                        {
                            if (!n[4])
                                return 47; // /
                            else if (!n[5])
                                return 47; // /
                            else if (!n[6])
                                return 60; // <
                            else if (!n[7])
                                return 179; // |
                        }
                        else if (!n[4])
                        {
                            if (!n[5])
                                return 47; // /
                            else if (!n[6])
                                return 197; // ┼
                            else if (!n[7])
                                return 62; // >
                        }
                        else if (!n[5])
                        {
                            if (!n[6])
                                return 92; // \
                            else if (!n[7])
                                return 92; // \
                        }
                        else if (!n[6])
                        {
                            if (!n[7])
                                return 92; // \
                        }
                        break;
                    }
                case 7:
                    {
                        if (!n[0])
                            return 47; // /
                        else if (!n[1])
                            return 118; // v
                        else if (!n[2])
                            return 92; // \
                        else if (!n[3])
                            return 60; // <
                        else if (!n[4])
                            return 47; // /
                        else if (!n[5])
                            return 94; // ^
                        else if (!n[6])
                            return 92; // \
                        else if (!n[7])
                            return 62; // >
                        break;
                    }
                case 8:
                    {
                        return 197; // ┼
                    }
            }

            return (byte)'?';
        }
        static byte[] RenderCellBig(int neighbors, bool[] n)
        {
            byte[] cellBig = new byte[9];

            switch (neighbors)
            {
                case 0:
                    {
                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;

                        return cellBig;
                    }
                case 1:
                    {
                        if (n[0])
                        {
                            cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                            cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                            cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                        }
                        else if (n[1])
                        {
                            cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                            cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                            cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                        }
                        else if (n[2])
                        {
                            cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                            cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                            cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                        }
                        else if (n[3])
                        {
                            cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                            cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                            cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                        }
                        else if (n[4])
                        {
                            cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                            cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                            cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                        }
                        else if (n[5])
                        {
                            cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                            cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                            cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                        }
                        else if (n[6])
                        {
                            cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                            cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                            cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                        }
                        else if (n[7])
                        {
                            cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                            cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                            cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                        }

                        return cellBig;
                    }
                case 2:
                    {
                        if (n[0])
                        {
                            if (n[1])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                            }
                            else if (n[2])
                            {
                                cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                            }
                            else if (n[3])
                            {
                                cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                            }
                            else if (n[4])
                            {
                                cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                            }
                            else if (n[5])
                            {
                                cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                            }
                            else if (n[6])
                            {
                                cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                            }
                            else if (n[7])

                            {
                                cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                            }
                        }
                        else if (n[1])
                        {
                            if (n[2])
                            {
                                cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                            }
                            else if (n[3])
                            {
                                cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                            }
                            else if (n[4])
                            {
                                cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                            }
                            else if (n[5])
                            {
                                cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                            }
                            else if (n[6])
                            {
                                cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                            }
                            else if (n[7])
                            {
                                cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                            }
                        }
                        else if (n[2])
                        {
                            if (n[3])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                            }
                            else if (n[4])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                            }
                            else if (n[5])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                            }
                            else if (n[6])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                            }
                            else if (n[7])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                            }
                        }
                        else if (n[3])
                        {
                            if (n[4])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                            }
                            else if (n[5])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                            }
                            else if (n[6])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                            }
                            else if (n[7])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                            }
                        }
                        else if (n[4])
                        {
                            if (n[5])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                            }
                            else if (n[6])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                            }
                            else if (n[7])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                            }
                        }
                        else if (n[5])
                        {
                            if (n[6])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                            }
                            else if (n[7])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                            }
                        }
                        else if (n[6])
                        {
                            if (n[7])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                            }
                        }

                        return cellBig;
                    }
                case 3:
                    {
                        if (n[0])
                        {
                            if (n[1])
                            {
                                if (n[2])
                                {
                                    cellBig[0] = 92; cellBig[1] =                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                }
                                else if (n[3])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                }
                                else if (n[4])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (n[5])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (n[6])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                }
                                else if (n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                }
                            }
                            else if (n[2])
                            {
                                if (n[3])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                }
                                else if (n[4])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (n[5])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (n[6])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                }
                                else if (n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                }
                            }
                            else if (n[3])
                            {
                                if (n[4])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (n[5])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (n[6])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                }
                                else if (n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                }
                            }
                            else if (n[4])
                            {
                                if (n[5])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                }
                                else if (n[6])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                }
                            }
                            else if (n[5])
                            {
                                if (n[6])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                }
                            }
                            else if (n[6])
                            {
                                if (n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                }
                            }
                        }
                        else if (n[1])
                        {
                            if (n[2])
                            {
                                if (n[3])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                }
                                else if (n[4])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (n[5])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (n[6])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                }
                                else if (n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                }
                            }
                            else if (n[3])
                            {
                                if (n[4])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (n[5])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (n[6])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                }
                                else if (n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                }
                            }
                            else if (n[4])
                            {
                                if (n[5])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                }
                                else if (n[6])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                }
                            }
                            else if (n[5])
                            {
                                if (n[6])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                }
                            }
                            else if (n[6])
                            {
                                if (n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                }
                            }
                        }
                        else if (n[2])
                        {
                            if (n[3])
                            {
                                if (n[4])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (n[5])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (n[6])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                }
                                else if (n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                }
                            }
                            if (n[4])
                            {
                                if (n[5])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                }
                                else if (n[6])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                }
                            }
                            if (n[5])
                            {
                                if (n[6])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                }
                            }
                            if (n[6])
                            {
                                if (n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                }
                            }
                        }
                        else if (n[3])
                        {
                            if (n[4])
                            {
                                if (n[5])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                }
                                if (n[6])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                }
                            }
                            if (n[5])
                            {
                                if (n[6])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                }
                            }
                            if (n[6])
                            {
                                if (n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                }
                            }
                        }
                        else if (n[4])
                        {
                            if (n[5])
                            {
                                if (n[6])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                }
                                else if (n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                }
                            }
                            if (n[6])
                            {
                                if (n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                }
                            }
                        }
                        else if (n[5])
                        {
                            if (n[6])
                            {
                                if (n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                }
                            }
                        }

                        return cellBig;
                    }
                case 4:
                    {
                        if (!n[0])
                        {
                            if (!n[1])
                            {
                                if (!n[2])
                                {
                                    if (!n[3])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                    }
                                    else if (!n[4])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                    else if (!n[5])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                    else if (!n[6])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                    }
                                }
                                else if (!n[3])
                                {
                                    if (!n[4])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                    else if (!n[5])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                    else if (!n[6])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                    }
                                }
                                else if (!n[4])
                                {
                                    if (!n[5])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                    }
                                    else if (!n[6])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                }
                                else if (!n[5])
                                {
                                    if (!n[6])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                }
                                else if (!n[6])
                                {
                                    if (!n[7])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                    }
                                }
                            }
                            else if (!n[2])
                            {
                                if (!n[3])
                                {
                                    if (!n[4])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                    else if (!n[5])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                    else if (!n[6])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                    }
                                }
                                else if (!n[4])
                                {
                                    if (!n[5])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                    }
                                    else if (!n[6])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                }
                                else if (!n[5])
                                {
                                    if (!n[6])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                }
                                else if (!n[6])
                                {
                                    if (!n[7])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                    }
                                }
                            }
                            else if (!n[3])
                            {
                                if (!n[4])
                                {
                                    if (!n[5])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                    }
                                    else if (!n[6])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                }
                                else if (!n[5])
                                {
                                    if (!n[6])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                }
                                else if (!n[6])
                                {
                                    if (!n[7])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                    }
                                }
                            }
                            else if (!n[4])
                            {
                                if (!n[5])
                                {
                                    if (!n[6])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                    }
                                }
                                else if (!n[6])
                                {
                                    if (!n[7])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                }
                            }
                            else if (!n[5])
                            {
                                if (!n[6])
                                {
                                    if (!n[7])
                                    {
                                        cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                }
                            }
                        }
                        else if (!n[1])
                        {
                            if (!n[2])
                            {
                                if (!n[3])
                                {
                                    if (!n[4])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                    else if (!n[5])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                    else if (!n[6])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                    }
                                }
                                else if (!n[4])
                                {
                                    if (!n[5])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                    }
                                    else if (!n[6])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                }
                                else if (!n[5])
                                {
                                    if (!n[6])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                }
                                else if (!n[6])
                                {
                                    if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                    }
                                }
                            }
                            else if (!n[3])
                            {
                                if (!n[4])
                                {
                                    if (!n[5])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                    }
                                    else if (!n[6])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                }
                                if (!n[5])
                                {
                                    if (!n[6])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                }
                                if (!n[6])
                                {
                                    if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                    }
                                }
                            }
                            else if (!n[4])
                            {
                                if (!n[5])
                                {
                                    if (!n[6])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                    }
                                }
                                else if (!n[6])
                                {
                                    if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                }
                            }
                            else if (!n[5])
                            {
                                if (!n[6])
                                {
                                    if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                }
                            }
                        }
                        else if (!n[2])
                        {
                            if (!n[3])
                            {
                                if (!n[4])
                                {
                                    if (!n[5])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                    }
                                    else if (!n[6])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                }
                                else if (!n[5])
                                {
                                    if (!n[6])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                }
                                else if (!n[6])
                                {
                                    if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                    }
                                }
                            }
                            else if (!n[4])
                            {
                                if (!n[5])
                                {
                                    if (!n[6])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                    }
                                }
                                else if (!n[6])
                                {
                                    if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                }
                            }
                            else if (!n[5])
                            {
                                if (!n[6])
                                {
                                    if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                }
                            }
                        }
                        else if (!n[3])
                        {
                            if (!n[4])
                            {
                                if (!n[5])
                                {
                                    if (!n[6])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                    }
                                    else if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                    }
                                }
                                else if (!n[6])
                                {
                                    if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                    }
                                }
                            }
                            else if (!n[5])
                            {
                                if (!n[6])
                                {
                                    if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                    }
                                }
                            }
                        }
                        else if (!n[4])
                        {
                            if (!n[5])
                            {
                                if (!n[6])
                                {
                                    if (!n[7])
                                    {
                                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                        cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                        cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                    }
                                }
                            }
                        }

                        return cellBig;
                    }
                case 5:
                    {
                        if (!n[0])
                        {
                            if (!n[1])
                            {
                                if (!n[2])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                }
                                else if (!n[3])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                }
                                else if (!n[4])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (!n[5])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (!n[6])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                }
                                else if (!n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                }
                            }
                            else if (!n[2])
                            {
                                if (!n[3])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                }
                                else if (!n[4])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (!n[5])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (!n[6])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                }
                                else if (!n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                }
                            }
                            else if (!n[3])
                            {
                                if (!n[4])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (!n[5])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (!n[6])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                }
                                else if (!n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                }
                            }
                            else if (!n[4])
                            {
                                if (!n[5])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                }
                                else if (!n[6])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (!n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                }
                            }
                            else if (!n[5])
                            {
                                if (!n[6])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (!n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                }
                            }
                            else if (!n[6])
                            {
                                if (!n[7])
                                {
                                    cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                }
                            }
                        }
                        else if (!n[1])
                        {
                            if (!n[2])
                            {
                                if (!n[3])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                }
                                else if (!n[4])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (!n[5])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (!n[6])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                }
                                else if (!n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                }
                            }
                            else if (!n[3])
                            {
                                if (!n[4])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (!n[5])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (!n[6])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                }
                                else if (!n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                }
                            }
                            else if (!n[4])
                            {
                                if (!n[5])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                }
                                else if (!n[6])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (!n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                }
                            }
                            else if (!n[5])
                            {
                                if (!n[6])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (!n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                }
                            }
                            else if (!n[6])
                            {
                                if (!n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                }
                            }
                        }
                        else if (!n[2])
                        {
                            if (!n[3])
                            {
                                if (!n[4])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (!n[5])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (!n[6])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                }
                                else if (!n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                                }
                            }
                            if (!n[4])
                            {
                                if (!n[5])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                }
                                else if (!n[6])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (!n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                }
                            }
                            if (!n[5])
                            {
                                if (!n[6])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (!n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                }
                            }
                            if (!n[6])
                            {
                                if (!n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                }
                            }
                        }
                        else if (!n[3])
                        {
                            if (!n[4])
                            {
                                if (!n[5])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                }
                                if (!n[6])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                }
                                else if (!n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                                }
                            }
                            if (!n[5])
                            {
                                if (!n[6])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                }
                                else if (!n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                                }
                            }
                            if (!n[6])
                            {
                                if (!n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                                }
                            }
                        }
                        else if (!n[4])
                        {
                            if (!n[5])
                            {
                                if (!n[6])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 32;
                                }
                                else if (!n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                                }
                            }
                            if (!n[6])
                            {
                                if (!n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                                }
                            }
                        }
                        else if (!n[5])
                        {
                            if (!n[6])
                            {
                                if (!n[7])
                                {
                                    cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                    cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                    cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                                }
                            }
                        }

                        return cellBig;
                    }
                case 6:
                    {
                        if (!n[0])
                        {
                            if (!n[1])
                            {
                                cellBig[0] = 32; cellBig[1] = 32; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                            }
                            else if (!n[2])
                            {
                                cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 32;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                            }
                            else if (!n[3])
                            {
                                cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                            }
                            else if (!n[4])
                            {
                                cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                            }
                            else if (!n[5])
                            {
                                cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                            }
                            else if (!n[6])
                            {
                                cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                            }
                            else if (!n[7])
                            {
                                cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                            }
                        }
                        else if (!n[1])
                        {
                            if (!n[2])
                            {
                                cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 32;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                            }
                            else if (!n[3])
                            {
                                cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                            }
                            else if (!n[4])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                            }
                            else if (!n[5])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                            }
                            else if (!n[6])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                            }
                            else if (!n[7])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                            }
                        }
                        else if (!n[2])
                        {
                            if (!n[3])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                            }
                            else if (!n[4])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                            }
                            else if (!n[5])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                            }
                            else if (!n[6])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                            }
                            else if (!n[7])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                            }
                        }
                        else if (!n[3])
                        {
                            if (!n[4])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                            }
                            else if (!n[5])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                            }
                            else if (!n[6])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                            }
                            else if (!n[7])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 32;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                            }
                        }
                        else if (!n[4])
                        {
                            if (!n[5])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 32;
                            }
                            else if (!n[6])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 32;
                            }
                            else if (!n[7])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                            }
                        }
                        else if (!n[5])
                        {
                            if (!n[6])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 32; cellBig[7] = 32; cellBig[8] = 92;
                            }
                            else if (!n[7])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                            }
                        }
                        else if (!n[6])
                        {
                            if (!n[7])
                            {
                                cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                                cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                                cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                            }
                        }

                        return cellBig;
                    }
                case 7:
                    {
                        if (!n[0])
                        {
                            cellBig[0] = 32; cellBig[1] = 179; cellBig[2] = 47;
                            cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                            cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                        }
                        else if (!n[1])
                        {
                            cellBig[0] = 92; cellBig[1] = 32; cellBig[2] = 47;
                            cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                            cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                        }
                        else if (!n[2])
                        {
                            cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 32;
                            cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                            cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                        }
                        else if (!n[3])
                        {
                            cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                            cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 32;
                            cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                        }
                        else if (!n[4])
                        {
                            cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                            cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                            cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 32;
                        }
                        else if (!n[5])
                        {
                            cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                            cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                            cellBig[6] = 47; cellBig[7] = 32; cellBig[8] = 92;
                        }
                        else if (!n[6])
                        {
                            cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                            cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                            cellBig[6] = 32; cellBig[7] = 179; cellBig[8] = 92;
                        }
                        else if (!n[7])
                        {
                            cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                            cellBig[3] = 32; cellBig[4] = 197; cellBig[5] = 196;
                            cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;
                        }

                        return cellBig;
                    }
                case 8:
                    {
                        cellBig[0] = 92; cellBig[1] = 179; cellBig[2] = 47;
                        cellBig[3] = 196; cellBig[4] = 197; cellBig[5] = 196;
                        cellBig[6] = 47; cellBig[7] = 179; cellBig[8] = 92;

                        return cellBig;
                    }
                default:
                    {
                        cellBig[0] = 63; cellBig[1] = 63; cellBig[2] = 63;
                        cellBig[3] = 63; cellBig[4] = 63; cellBig[5] = 63;
                        cellBig[6] = 63; cellBig[7] = 63; cellBig[8] = 63;

                        return cellBig;
                    }
            }
        }
        static async void RayCasting()
        {
            DateTime fpsLimitStartingTime = DateTime.Now;
            int frameCounter = 0;

            int tempPlayerX = 0;
            int tempPlayerY = 0;

            while (true)
            {
                DateTime frameStartingTime = DateTime.Now;

                try
                {
                    if (tempPlayerX != player.X || tempPlayerY != player.Y)
                    {
                        tempPlayerX = player.X;
                        tempPlayerY = player.Y;
                    }
                    else
                    {
                        Thread.Sleep(1);
                        goto SkipRayCasting; // Если игрок стоит на месте, не нужно заново рассчитывать зону видимости
                    }

                    if (Pause)
                    {
                        Thread.Sleep((int)(1000 / FpsLimit));
                        continue;
                    }

                    map.InSightMapBuffer = new Map.SightMapCell[map.MapHeight, map.MapWidth];

                    var rayCastingTasks = new List<Task<bool>>();

                    for (double a = 0; a < 360; a++)
                    {
                        double angle = a * Math.PI / 180d;
                        rayCastingTasks.Add(Task.Run(() => CastRay(angle)));
                    }

                    await Task.WhenAll(rayCastingTasks); // как подругому ожидать выполнения всех Тасков я не знаю, по этому я добавил их в список

                    map.InSightMap = (Map.SightMapCell[,])map.InSightMapBuffer.Clone();
                }
                catch (Exception ex) { try { File.AppendAllText("log.txt", DateTime.Now + "\nОшибка при запуске рассчёта лучей:\n" + ex.ToString() + "\n\n\n", Encoding.UTF8); } catch { } }

                SkipRayCasting:

                // Рассчёт FPS
                if ((DateTime.Now - fpsLimitStartingTime).TotalSeconds >= 0.5)
                {
                    if (frameCounter > 0)
                        FPSRayCasting = frameCounter * 2;
                    else
                        FPSRayCasting = 0;
                    frameCounter = 0;
                    fpsLimitStartingTime = DateTime.Now;
                }
                else frameCounter++;

                // Ограничение FPS
                DateTime frameFinishingTime = DateTime.Now;
                if ((frameFinishingTime - frameStartingTime).TotalMilliseconds < 1000 / FpsLimit)
                    Thread.Sleep((int)(1000 / FpsLimit - (frameFinishingTime - frameStartingTime).TotalMilliseconds));
            }
        }
        static bool CastRay(double rayAngle)
        {
            try
            {
                double rayX = Math.Sin(rayAngle);
                double rayY = Math.Cos(rayAngle);

                double distanceToWall = 0;
                bool hitWall = false;

                while (!hitWall && distanceToWall < player.ViewDistance)
                {
                    distanceToWall += 0.1;

                    int testX = (int)(player.X + 0.5 + rayX * distanceToWall);
                    int testY = (int)(player.Y + 0.5 + rayY * distanceToWall);

                    if (testX < 0 || testX >= player.ViewDistance + player.X || testY < 0 || testY >= player.ViewDistance + player.Y)
                    {
                        hitWall = true;
                        distanceToWall = player.ViewDistance;
                    }
                    else
                    {
                        if (map.LevelMap[testY, testX].Type == 1)
                            hitWall = true;
                        /*else // нельзя видеть сквозь монстров
                            foreach (Monster m in monsters)
                                if (testX == m.X && testY == m.Y)
                                    hitWall = true;*/

                        map.LevelMap[testY, testX].Seen = true;
                        map.InSightMapBuffer[testY, testX].InSight = true;
                        map.InSightMapBuffer[testY, testX].Distance = (int)distanceToWall;
                    }
                }

                // нельзя видеть сквозь углы стен
                int neighbors = 0;

                if ((int)(player.Y + 0.5 + rayY * distanceToWall) - 1 < 0) neighbors++;
                else if (map.LevelMap[(int)(player.Y + 0.5 + rayY * distanceToWall) - 1, (int)(player.X + 0.5 + rayX * distanceToWall)].Type == 1) neighbors++;

                if ((int)(player.X + 0.5 + rayX * distanceToWall) - 1 < 0) neighbors++;
                else if (map.LevelMap[(int)(player.Y + 0.5 + rayY * distanceToWall), (int)(player.X + 0.5 + rayX * distanceToWall) - 1].Type == 1) neighbors++;

                if ((int)(player.X + 0.5 + rayX * distanceToWall) + 1 >= map.MapWidth) neighbors++;
                else if (map.LevelMap[(int)(player.Y + 0.5 + rayY * distanceToWall), (int)(player.X + 0.5 + rayX * distanceToWall) + 1].Type == 1) neighbors++;

                if ((int)(player.Y + 0.5 + rayY * distanceToWall) + 1 >= map.MapHeight) neighbors++;
                else if (map.LevelMap[(int)(player.Y + 0.5 + rayY * distanceToWall) + 1, (int)(player.X + 0.5 + rayX * distanceToWall)].Type == 1) neighbors++;

                if (neighbors >= 4)
                {
                    map.LevelMap[(int)(player.Y + 0.5 + rayY * distanceToWall), (int)(player.X + 0.5 + rayX * distanceToWall)].Seen = false;
                    map.InSightMapBuffer[(int)(player.Y + 0.5 + rayY * distanceToWall), (int)(player.X + 0.5 + rayX * distanceToWall)].InSight = false;
                }
            }
            catch (Exception ex) { try { File.AppendAllText("log.txt", DateTime.Now + "\nОшибка при рассчёте луча:\n" + ex.ToString() + "\n\n\n", Encoding.UTF8); } catch { } }

            return true;
        }
        static void SetConsoleFont(string fontName, short fontSizeY)
        {
            unsafe
            {
                IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
                if (hnd != INVALID_HANDLE_VALUE)
                {
                    CONSOLE_FONT_INFO_EX info = new CONSOLE_FONT_INFO_EX();
                    info.cbSize = (uint)Marshal.SizeOf(info);

                    CONSOLE_FONT_INFO_EX newInfo = new CONSOLE_FONT_INFO_EX();
                    newInfo.cbSize = (uint)Marshal.SizeOf(newInfo);
                    newInfo.FontFamily = TMPF_TRUETYPE;
                    IntPtr ptr = new IntPtr(newInfo.FaceName);
                    Marshal.Copy(fontName.ToCharArray(), 0, ptr, fontName.Length);

                    newInfo.dwFontSize = new COORD(info.dwFontSize.X, fontSizeY);
                    newInfo.FontWeight = info.FontWeight;
                    SetCurrentConsoleFontEx(hnd, false, ref newInfo);
                }
            }
        }
        static void SetConsoleColorGardient(int r, int g, int b, int stepR, int stepG, int stepB)
        {
            uint R = (uint)r;
            uint G = (uint)g;
            uint B = (uint)b;

            // Чёрный
            SetScreenColorsApp.SetColor((ConsoleColor)0, 0, 0, 0);

            // Гардиент от тёмного к светлому для основной грдации цвета
            SetScreenColorsApp.SetColor((ConsoleColor)1, R, G, B);
            SetScreenColorsApp.SetColor((ConsoleColor)2, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)3, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)4, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)5, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)6, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)7, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)8, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)9, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)10, R, G, B);

            // Белый
            SetScreenColorsApp.SetColor((ConsoleColor)11, 255, 255, 255);

            // Цвета интерфейса
            SetScreenColorsApp.SetColor((ConsoleColor)12, 125, 0, 0);
            SetScreenColorsApp.SetColor((ConsoleColor)13, 0, 200, 85);
            SetScreenColorsApp.SetColor((ConsoleColor)14, 0, 150, 255);
            SetScreenColorsApp.SetColor((ConsoleColor)15, 255, 225, 0);
        }
    }
    public class Player
    {
        public bool Alive = true;

        public int Lvl;
        public double Exp;

        public int X;
        public int Y;

        public int ViewDistance;
        public int ActionSpeed;

        public string Name;
        public char Icon;

        public double HealthPoints;
        public double MaxHealthPoints;
        public double HealthRegen;

        public int ShieldPoints;
        public int MaxShieldPoints;
        public double ShieldCapacity;
        public double ShieldRegen;

        private double tempShieldPoints = 0;

        public double Damage;

        public bool isDig = false;

        private static System.Timers.Timer Regeneration;

        //private Thread Mouse_0;
        private Thread Keyboard_0;
        private Thread Keyboard_1;
        private Thread Keyboard_8;
        private Thread Keyboard_9;
        public Player(string Name, char Icon, int Lvl, double HealthPoints, double HealthRegen, int ShieldPoints, double ShieldCapacity, double ShieldRegen, double Damage, int ViewDistance, int ActionSpeed, Map map, List<Monster> monsters)
        {
            this.Lvl = Lvl;
            Exp = 0;

            this.ViewDistance = ViewDistance;
            this.ActionSpeed = ActionSpeed;

            this.Name = Name;
            this.Icon = Icon;

            this.HealthPoints = HealthPoints;
            MaxHealthPoints = HealthPoints;
            this.HealthRegen = HealthRegen;


            this.ShieldPoints = ShieldPoints;
            MaxShieldPoints = ShieldPoints;
            this.ShieldCapacity = ShieldCapacity;
            this.ShieldRegen = ShieldRegen;

            this.Damage = Damage;

            Spawn(map);
            //Mouse_0 = new Thread(Mouse0);
            Keyboard_0 = new Thread(() => Keyboard0(map, monsters));
            Keyboard_1 = new Thread(Keyboard1);
            Keyboard_8 = new Thread(Keyboard8);
            Keyboard_9 = new Thread(Keyboard9);

            //Mouse_0.Start();
            Keyboard_0.Start();
            Keyboard_1.Start();
            Keyboard_8.Start();
            Keyboard_9.Start();


            Regeneration = new System.Timers.Timer(200);
            Regeneration.Elapsed += Regenerate;
            Regeneration.Start();
        }
        public void ToDie()
        {
            Alive = false;
            Keyboard_0.Abort();
            Keyboard_1.Abort();
            Keyboard_9.Abort();
        }
        public void ReceiveDamage(double damage)
        {
            int shieldPointsUsed = 0;
            double tempDamage = 0;
            double tempDamageShiedAbsorbed = 0;

            if (ShieldPoints > 0)
            {
                if (damage > ShieldCapacity)
                {
                    if (ShieldPoints >= (int)(damage % ShieldCapacity != 0 ? damage / ShieldCapacity + 1 : damage / ShieldCapacity))
                    {
                        ShieldPoints -= (int)(damage % ShieldCapacity != 0 ? damage / ShieldCapacity + 1 : damage / ShieldCapacity);
                        shieldPointsUsed = (int)(damage % ShieldCapacity != 0 ? damage / ShieldCapacity + 1 : damage / ShieldCapacity);
                        tempDamageShiedAbsorbed = damage;
                    }
                    else
                    {
                        tempDamage = damage - ShieldPoints * ShieldCapacity;
                        HealthPoints -= tempDamage;
                        shieldPointsUsed = ShieldPoints;
                        tempDamageShiedAbsorbed = ShieldPoints * ShieldCapacity;
                        ShieldPoints = 0;
                    }
                }
                else
                {
                    ShieldPoints--;
                    shieldPointsUsed = 1;
                    tempDamageShiedAbsorbed = damage;
                }
            }
            else
            {
                HealthPoints -= damage;
                tempDamage = damage;
            }

            if (HealthPoints > 0)
            {
                if (shieldPointsUsed == 0)
                    WriteLog($"{Name} takes {tempDamage:f2} damage", true);
                else if (shieldPointsUsed == 1)
                {
                    if (tempDamage == 0)
                        WriteLog($"{Name} blocks {tempDamageShiedAbsorbed:f2} damage by {shieldPointsUsed} shield charge", true);
                    else
                        WriteLog($"{Name} blocks {tempDamageShiedAbsorbed:f2} damage by {shieldPointsUsed} shield charge and takes {tempDamage:f2} damage", true);
                }
                else
                {
                    if (tempDamage == 0)
                        WriteLog($"{Name} blocks {tempDamageShiedAbsorbed:f2} damage by {shieldPointsUsed} shield charges", true);
                    else
                        WriteLog($"{Name} blocks {tempDamageShiedAbsorbed:f2} damage by {shieldPointsUsed} shield charges and takes {tempDamage:f2} damage", true);

                }
            }
            else
            {
                Alive = false;

                if (shieldPointsUsed == 0)
                    WriteLog($"{Name} takes {tempDamage:f2} damage and dies", true);
                else if (shieldPointsUsed == 1)
                    WriteLog($"{Name} blocks {tempDamageShiedAbsorbed:f2} damage by {shieldPointsUsed} shield charge and takes {tempDamage:f2} damage and dies", true);
                else
                    WriteLog($"{Name} blocks {tempDamageShiedAbsorbed:f2} damage by {shieldPointsUsed} shield charges and takes {tempDamage:f2} damage and dies", true);
            }
        }
        private void DealDamage(Map map, List<Monster> monsters, int Y, int X)
        {
            if (map.LevelMap[Y, X].MonsterID != -1)
            {
                WriteLog($"{Name} attacks {monsters[map.LevelMap[Y, X].MonsterID].Name}", false);
                monsters[map.LevelMap[Y, X].MonsterID].ReceiveDamage(Damage * Game.Rand.Next(50, 200) / 100d, map);
                if (Exp + 5 >= 100)
                {
                    Lvl++;
                    MaxHealthPoints *= 1.1;
                    Exp = Exp - 100 + 5;
                }
                else
                    Exp += 5;
            }
            else
                WriteLog($"{Name} fails to attack", false);
        }
        private void WriteLog(string Log, bool AddStep)
        {
            if (Log != "")
            {
                List<string> tempCombatLog = new List<string>(Game.CombatLog);

                if (AddStep)
                {
                    tempCombatLog.RemoveAt(0);
                    tempCombatLog.Add(" > " + Log.Replace(',', '.'));
                }
                else
                {
                    tempCombatLog.RemoveAt(0);
                    tempCombatLog.Add(Log.Replace(',', '.'));
                }

                Game.CombatLog = new List<string>(tempCombatLog);
            }
        }
        private void Spawn(Map map)
        {
            int x;
            int y;

            while (true)
            {
                x = Game.Rand.Next(map.LevelMap.GetLength(1));
                y = Game.Rand.Next(map.LevelMap.GetLength(0));
                if (map.LevelMap[y, x].Type != 1)
                    break;
            }

            X = x;
            Y = y;
        }
        private void Regenerate(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Game.Pause)
                return;

            double tempRegen = MaxHealthPoints * HealthRegen / 100d / 5d;

            if (HealthPoints > 0)
            {
                if (HealthPoints + tempRegen > MaxHealthPoints)
                    HealthPoints = MaxHealthPoints;
                else
                    HealthPoints += tempRegen;

                tempShieldPoints += ShieldRegen / 5d;

                if (tempShieldPoints >= 1)
                {
                    if (ShieldPoints + 1 <= MaxShieldPoints)
                    {
                        ShieldPoints++;
                        tempShieldPoints--;
                    }
                    else
                        tempShieldPoints = 0;
                }
            }
        }
        private void Keyboard0(Map map, List<Monster> monsters)
        {
            while (true)
            {
                if (Game.Pause)
                {
                    Thread.Sleep(ActionSpeed);
                    continue;
                }

                DateTime startingTime = DateTime.Now;

                bool attackTry = false;

                // Нампад или WASD
                if (Game.GetAsyncKeyState(104) != 0 || Game.GetAsyncKeyState(87) != 0) // Вверх
                {
                    if (map.LevelMap[Y - 1, X].Type != 1)
                    {
                        if (map.LevelMap[Y - 1, X].MonsterID == -1)
                            Y--;
                        else
                        {
                            attackTry = true;

                            Thread.Sleep(ActionSpeed / 2);

                            if (Alive)
                                DealDamage(map, monsters, Y - 1, X);
                        }
                    }
                    else if (isDig && Y - 1 > 1)
                    {
                        map.LevelMap[Y - 1, X].Type = 0;
                        map.LevelMap[Y - 1, X].Seen = true;
                    }
                }
                if (Game.GetAsyncKeyState(98) != 0 || Game.GetAsyncKeyState(83) != 0) // Вниз
                {
                    if (map.LevelMap[Y + 1, X].Type != 1)
                    {
                        if (map.LevelMap[Y + 1, X].MonsterID == -1)
                            Y++;
                        else
                        {
                            attackTry = true;

                            Thread.Sleep(ActionSpeed / 2);

                            if (Alive)
                                DealDamage(map, monsters, Y + 1, X);
                        }
                    }
                    else if (isDig && Y + 1 < map.MapHeight - 2)
                    {
                        map.LevelMap[Y + 1, X].Type = 0;
                        map.LevelMap[Y + 1, X].Seen = true;
                    }
                }

                if (Game.GetAsyncKeyState(100) != 0 || Game.GetAsyncKeyState(65) != 0) // Влево
                {
                    if (map.LevelMap[Y, X - 1].Type != 1)
                    {
                        if (map.LevelMap[Y, X - 1].MonsterID == -1)
                            X--;
                        else
                        {
                            attackTry = true;

                            Thread.Sleep(ActionSpeed / 2);

                            if (Alive)
                                DealDamage(map, monsters, Y, X - 1);
                        }
                    }
                    else if (isDig && X - 1 > 1)
                    {
                        map.LevelMap[Y, X - 1].Type = 0;
                        map.LevelMap[Y, X - 1].Seen = true;
                    }
                }
                if (Game.GetAsyncKeyState(102) != 0 || Game.GetAsyncKeyState(68) != 0) // Вправо
                {
                    if (map.LevelMap[Y, X + 1].Type != 1)
                    {
                        if (map.LevelMap[Y, X + 1].MonsterID == -1)
                            X++;
                        else
                        {
                            attackTry = true;

                            Thread.Sleep(ActionSpeed / 2);

                            if (Alive)
                                DealDamage(map, monsters, Y, X + 1);
                        }
                    }
                    else if (isDig && X + 1 < map.MapWidth - 2)
                    {
                        map.LevelMap[Y, X + 1].Type = 0;
                        map.LevelMap[Y, X + 1].Seen = true;
                    }
                }

                if (Game.GetAsyncKeyState(103) != 0) // Вверх влево
                {
                    if (map.LevelMap[Y - 1, X - 1].Type != 1)
                    {
                        if (map.LevelMap[Y - 1, X - 1].MonsterID == -1)
                        {
                            Y--;
                            X--;
                        }
                        else
                        {
                            attackTry = true;

                            Thread.Sleep(ActionSpeed / 2);

                            if (Alive)
                                DealDamage(map, monsters, Y - 1, X - 1);
                        }
                    }
                    else if (isDig && Y - 1 > 1 && X - 1 > 1)
                    {
                        map.LevelMap[Y - 1, X - 1].Type = 0;
                        map.LevelMap[Y - 1, X - 1].Seen = true;
                    }
                }
                if (Game.GetAsyncKeyState(97) != 0) // Вниз влево
                {
                    if (map.LevelMap[Y + 1, X - 1].Type != 1)
                    {
                        if (map.LevelMap[Y + 1, X - 1].MonsterID == -1)
                        {
                            Y++;
                            X--;
                        }
                        else
                        {
                            attackTry = true;

                            Thread.Sleep(ActionSpeed / 2);

                            if (Alive)
                                DealDamage(map, monsters, Y + 1, X - 1);
                        }
                    }
                    else if (isDig && Y + 1 < map.MapHeight - 2 && X - 1 > 1)
                    {
                        map.LevelMap[Y + 1, X - 1].Type = 0;
                        map.LevelMap[Y + 1, X - 1].Seen = true;
                    }
                }
                if (Game.GetAsyncKeyState(105) != 0) // Вверх вправо
                {
                    if (map.LevelMap[Y - 1, X + 1].Type != 1)
                    {
                        if (map.LevelMap[Y - 1, X + 1].MonsterID == -1)
                        {
                            Y--;
                            X++;
                        }
                        else
                        {
                            attackTry = true;

                            Thread.Sleep(ActionSpeed / 2);

                            if (Alive)
                                DealDamage(map, monsters, Y - 1, X + 1);
                        }
                    }
                    else if (isDig && Y - 1 > 1 && X + 1 < map.MapWidth - 2)
                    {
                        map.LevelMap[Y - 1, X + 1].Type = 0;
                        map.LevelMap[Y - 1, X + 1].Seen = true;
                    }
                }
                if (Game.GetAsyncKeyState(99) != 0) // Вниз вправо
                {
                    if (map.LevelMap[Y + 1, X + 1].Type != 1)
                    {
                        if (map.LevelMap[Y + 1, X + 1].MonsterID == -1)
                        {
                            Y++;
                            X++;
                        }
                        else
                        {
                            attackTry = true;

                            Thread.Sleep(ActionSpeed / 2);

                            if (Alive)
                                DealDamage(map, monsters, Y + 1, X + 1);
                        }
                    }
                    else if (isDig && Y + 1 < map.MapHeight - 2 && X + 1 < map.MapWidth - 2)
                    {
                        map.LevelMap[Y + 1, X + 1].Type = 0;
                        map.LevelMap[Y + 1, X + 1].Seen = true;
                    }
                }

                if (attackTry)
                {
                    DateTime finishingTime = DateTime.Now;
                    if ((finishingTime - startingTime).TotalMilliseconds < ActionSpeed / 2)
                        Thread.Sleep((int)(ActionSpeed / 2 - (finishingTime - startingTime).TotalMilliseconds));
                }
                else
                {
                    DateTime finishingTime = DateTime.Now;
                    if ((finishingTime - startingTime).TotalMilliseconds < ActionSpeed)
                        Thread.Sleep((int)(ActionSpeed - (finishingTime - startingTime).TotalMilliseconds));
                }
            }
        }
        private void Keyboard1()
        {
            while (true)
            {
                if (Game.Pause)
                {
                    Thread.Sleep(30);
                    continue;
                }

                if (Game.GetAsyncKeyState(17) != 0)
                    isDig = true;
                else
                    isDig = false;

                Thread.Sleep(30);
            }
        }
        private void Keyboard8()
        {
            while (true)
            {
                if (Game.Pause)
                {
                    Thread.Sleep(30);
                    continue;
                }

                if (Game.GetAsyncKeyState(187) != 0 || Game.GetAsyncKeyState(107) != 0) // +
                {
                    //Game.RenderScale = Game.RenderScale < 3 ? Game.RenderScale + 1 : 3;
                    Game.RenderScale = 3;

                    Thread.Sleep(300);
                }
                if (Game.GetAsyncKeyState(189) != 0 || Game.GetAsyncKeyState(108) != 0) // -
                {
                    //Game.RenderScale = Game.RenderScale > 1 ? Game.RenderScale - 1 : 1;
                    Game.RenderScale = 1;

                    Thread.Sleep(300);
                }

                if (Game.GetAsyncKeyState(112) != 0) // F1
                {
                    Game.TrueSight = !Game.TrueSight;

                    Thread.Sleep(300);
                }

                Thread.Sleep(30);
            }
        }
        private void Keyboard9()
        {
            while (true)
            {
                if (Game.GetAsyncKeyState(27) != 0) // Esc
                {
                    //Environment.Exit(0);

                    Game.Pause = !Game.Pause;
                    if (!Game.Pause)
                    {
                        Game.ChosenItem = 0;
                        Game.ItemOpened = false;
                    }
                    Thread.Sleep(300);
                }
                if (Game.Pause)
                {
                    // Нампад или WASD
                    if (Game.GetAsyncKeyState(104) != 0 || Game.GetAsyncKeyState(87) != 0) // Вверх
                    {
                        Game.ItemOpened = false;
                        Game.ChosenItem = Game.ChosenItem - 1 == -1 ? Game.MenuItems.Length - 1 : Game.ChosenItem - 1;
                        Thread.Sleep(150);
                    }
                    if (Game.GetAsyncKeyState(98) != 0 || Game.GetAsyncKeyState(83) != 0) // Вниз
                    {
                        Game.ItemOpened = false;
                        Game.ChosenItem = Game.ChosenItem + 1 == Game.MenuItems.Length ? 0 : Game.ChosenItem + 1;
                        Thread.Sleep(150);
                    }
                    if (Game.GetAsyncKeyState(100) != 0 || Game.GetAsyncKeyState(65) != 0) // Влево
                    {
                        Game.ItemOpened = false;
                        Thread.Sleep(150);
                    }
                    if (Game.GetAsyncKeyState(102) != 0 || Game.GetAsyncKeyState(68) != 0) // Вправо
                    {
                        if (Game.ChosenItem != 0)
                            Game.ItemOpened = true;

                        Thread.Sleep(150);
                    }
                    if (Game.GetAsyncKeyState(13) != 0 || Game.GetAsyncKeyState(32) != 0) // Enter, Space
                    {
                        if (Game.ChosenItem == 0)
                        {
                            Game.Pause = false;
                            Game.ChosenItem = 0;
                            Game.ItemOpened = false;
                        }
                        else if(Game.ChosenItem == 1 && Game.ItemOpened)
                        {
                            Game.Pause = false;
                            Game.ChosenItem = 0;
                            Game.ItemOpened = false;
                            //Game.Restarting = true;
                        }
                        else if (Game.ChosenItem == 3 && Game.ItemOpened)
                            Game.DisplayInfo = !Game.DisplayInfo;
                        else if (Game.ChosenItem == 4 && Game.ItemOpened)
                            Environment.Exit(0);
                        else
                            Game.ItemOpened = true;

                        Thread.Sleep(150);
                    }
                    if (Game.GetAsyncKeyState(89) != 0) // Y
                    {
                        if (Game.ChosenItem == 1 && Game.ItemOpened)
                        {
                            Game.Pause = false;
                            Game.ChosenItem = 0;
                            Game.ItemOpened = false;
                            //Game.Restarting = true;
                        }
                        else if (Game.ChosenItem == 3 && Game.ItemOpened)
                            Game.DisplayInfo = true;
                        else if (Game.ChosenItem == 4 && Game.ItemOpened)
                            Environment.Exit(0);

                        Thread.Sleep(150);
                    }
                    if (Game.GetAsyncKeyState(78) != 0) // N
                    {
                        if (Game.ChosenItem == 1 && Game.ItemOpened)
                            Game.ItemOpened = false;
                        else if (Game.ChosenItem == 3 && Game.ItemOpened)
                            Game.DisplayInfo = false;
                        else if (Game.ChosenItem == 4 && Game.ItemOpened)
                            Game.ItemOpened = false;

                        Thread.Sleep(150);
                    }
                    if ((Game.GetAsyncKeyState(187) != 0 || Game.GetAsyncKeyState(107) != 0) && Game.ChosenItem == 3 && Game.ItemOpened) // +
                    {
                        if (Game.FpsLimit == -1)
                            Game.FpsLimit = 10;
                        else
                            Game.FpsLimit = Game.FpsLimit + 1 <= 240 ? Game.FpsLimit + 1 : 240;

                        Thread.Sleep(100);
                    }
                    if ((Game.GetAsyncKeyState(189) != 0 || Game.GetAsyncKeyState(108) != 0) && Game.ChosenItem == 3 && Game.ItemOpened) // -
                    {
                        if (Game.FpsLimit == 10 || Game.FpsLimit == -1)
                            Game.FpsLimit = -1;
                        else
                            Game.FpsLimit = Game.FpsLimit - 1 >= 10 ? Game.FpsLimit - 1 : 10;

                        Thread.Sleep(100);
                    }
                }

                Thread.Sleep(30);
            }
        }
        private void Mouse0()
        {
            
        }
    }
    public class Monster
    {
        public int ID;
        public bool Alive = true;
        public int X;
        public int Y;
        public int ViewDistance;
        public string Name;
        public char Icon;
        public double HealthPoints;
        public double Damage;
        public int ActionSpeed;

        public int Ability;
        public double AbilityCooldown;

        private bool PreviousStepX = Convert.ToBoolean(Game.Rand.Next(1));
        private bool PreviousStepY = Convert.ToBoolean(Game.Rand.Next(1));
        private bool PreviousMoveX = false;
        private bool PreviousMoveY = false;

        private Thread Action;
        public Monster(string Name, char Icon, double HealthPoints, double Damage, int ViewDistance, int ActionSpeed, int Ability, double AbilityCooldown, Map map, Player player, List<Monster> monsters)
        {
            this.Name = Name;
            this.Icon = Icon;
            this.HealthPoints = HealthPoints;
            this.Damage = Damage;
            this.ViewDistance = ViewDistance;
            this.ActionSpeed = ActionSpeed;
            this.Ability = Ability;
            this.AbilityCooldown = AbilityCooldown;
            Spawn(map, player, monsters);

            Action = new Thread(() => Live(map, player, monsters));
            Action.Start();
        }
        public void ReceiveDamage(double damage, Map map)
        {
            if (HealthPoints - damage > 0)
            {
                HealthPoints -= damage;
                WriteLog($"{Name} takes {damage:f2} damage", true);
            }
            else
            {
                ToDie(map);
                WriteLog($"{Name} takes {damage:f2} damage and dies", true);
            }
        }
        public void DealDamage(Map map, List<Monster> monsters, Player player, int Y, int X)
        {
            if (player.X == X && player.Y == Y)
            {
                WriteLog($"{Name} attacks {player.Name}", false);
                player.ReceiveDamage(Game.Rand.Next(50, 200) * Damage / 100);
            }
            else if (map.LevelMap[Y, X].MonsterID != -1)
            {
                WriteLog($"{Name} tries to attack {player.Name} and hits {monsters[map.LevelMap[Y, X].MonsterID].Name}", false);
                monsters[map.LevelMap[Y, X].MonsterID].ReceiveDamage(Damage * Game.Rand.Next(50, 200) / 100d, map);
            }
            else
                WriteLog($"{Name} fails to attack", false);
        }
        private void WriteLog(string Log, bool AddStep)
        {
            if (Log != "")
            {
                List<string> tempCombatLog = new List<string>(Game.CombatLog);

                if (AddStep)
                {
                    tempCombatLog.RemoveAt(0);
                    tempCombatLog.Add(" > " + Log.Replace(',', '.'));
                }
                else
                {
                    tempCombatLog.RemoveAt(0);
                    tempCombatLog.Add(Log.Replace(',', '.'));
                }

                Game.CombatLog = new List<string>(tempCombatLog);
            }
        }
        public void ToDie(Map map)
        {
            Icon = 'x';
            map.LevelMap[Y, X].MonsterID = -1;
            map.LevelMap[Y, X].DeadMonsterID = ID;
            Alive = false;
            Action.Abort();
        }
        public void UseAbility(Map map)
        { 
            // Использование способности (и засыпание потока на время её использования?)
        }
        private void Live(Map map, Player player, List<Monster> monsters)
        {
            Thread.Sleep(1000);

            while (Alive)
            {
                if (Game.Pause)
                {
                    Thread.Sleep(ActionSpeed);
                    continue;
                }

                DateTime startingTime = DateTime.Now;

                if (HealthPoints <= 0)
                {
                    ToDie(map);
                    break;
                }

                bool attackTry = false;

                if (Math.Sqrt(((player.X + 0.5) - (X + 0.5)) * ((player.X + 0.5) - (X + 0.5)) + ((player.Y + 0.5) - (Y + 0.5)) * ((player.Y + 0.5) - (Y + 0.5))) <= ViewDistance)
                {
                    //Icon = '!'; // Чует игрока

                    int tempPlayerX = player.X;
                    int tempPlayerY = player.Y;

                    bool attackX = false;
                    bool attackY = false;

                    if (map.LevelMap[Y, X + 1].Type == 0 && map.LevelMap[Y, X + 1].MonsterID == -1)
                    {
                        if (Math.Abs(X - tempPlayerX) <= 1)
                        {
                            attackX = true;
                            PreviousMoveX = false;
                        }
                        else if (X < tempPlayerX && !attackX)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            X++;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepX = true;
                            PreviousMoveX = true;
                        }
                    }

                    if (map.LevelMap[Y, X - 1].Type == 0 && map.LevelMap[Y, X - 1].MonsterID == -1)
                    {
                        if (Math.Abs(X - tempPlayerX) <= 1)
                        {
                            attackX = true;
                            PreviousMoveX = false;
                        }
                        else if (X > tempPlayerX && !attackX)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            X--;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepX = false;
                            PreviousMoveX = true;
                        }
                    }

                    if (map.LevelMap[Y + 1, X].Type == 0 && map.LevelMap[Y + 1, X].MonsterID == -1)
                    {
                        if (Math.Abs(Y - tempPlayerY) <= 1)
                        {
                            attackY = true;
                            PreviousMoveY = false;
                        }
                        else if (Y < tempPlayerY && !attackY)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            Y++;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepY = true;
                            PreviousMoveY = true;
                        }
                    }

                    if (map.LevelMap[Y - 1, X].Type == 0 && map.LevelMap[Y - 1, X].MonsterID == -1)
                    {
                        if (Math.Abs(Y - tempPlayerY) <= 1)
                        {
                            attackY = true;
                            PreviousMoveY = false;
                        }
                        else if (Y > tempPlayerY && !attackY)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            Y--;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepY = false;
                            PreviousMoveY = true;
                        }
                    }

                    int tempRandMove = Game.Rand.Next(100);

                    if (tempRandMove < 25 && !PreviousMoveX && PreviousMoveY)
                    {
                        if (map.LevelMap[Y, X + 1].Type == 0 && map.LevelMap[Y, X + 1].MonsterID == -1)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            X++;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepX = true;
                            PreviousMoveX = true;
                        }
                    }
                    else if (tempRandMove > 75 && !PreviousMoveX && PreviousMoveY)
                    {
                        if (map.LevelMap[Y, X - 1].Type == 0 && map.LevelMap[Y, X - 1].MonsterID == -1)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            X--;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepX = false;
                            PreviousMoveX = true;
                        }
                    }

                    if (tempRandMove < 25 && !PreviousMoveY && PreviousMoveX)
                    {
                        if (map.LevelMap[Y + 1, X].Type == 0 && map.LevelMap[Y + 1, X].MonsterID == -1)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            Y++;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepY = true;
                            PreviousMoveY = true;
                        }
                    }
                    else if (tempRandMove > 75 && !PreviousMoveY && PreviousMoveX)
                    {
                        if (map.LevelMap[Y - 1, X].Type == 0 && map.LevelMap[Y - 1, X].MonsterID == -1)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            Y--;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepY = false;
                            PreviousMoveY = true;
                        }
                    }

                    // Атаковать, если расстояние по обеим осям не превышает 1.5
                    if (attackX && attackY)
                    {
                        attackTry = true;

                        Thread.Sleep(ActionSpeed / 2);

                        if (Alive)
                            DealDamage(map, monsters, player, tempPlayerY, tempPlayerX);
                    }

                }
                else
                {
                    //Icon = '?'; // Не чует игрока

                    if (PreviousStepX)
                    {
                        if (Game.Rand.Next(100) < 95 && map.LevelMap[Y, X + 1].Type == 0 && map.LevelMap[Y, X + 1].MonsterID == -1)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            X++;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepX = true;
                        }
                        else if (map.LevelMap[X - 1, Y].Type == 0)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            X--;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepX = false;
                        }
                    }
                    else
                    {
                        if (Game.Rand.Next(100) < 95 && map.LevelMap[Y, X - 1].Type == 0 && map.LevelMap[Y, X - 1].MonsterID == -1)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            X--;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepX = false;
                        }
                        else if (map.LevelMap[X + 1, Y].Type == 0)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            X++;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepX = true;
                        }
                    }

                    if (PreviousStepY)
                    {
                        if (Game.Rand.Next(100) < 95 && map.LevelMap[Y + 1, X].Type == 0 && map.LevelMap[Y + 1, X].MonsterID == -1)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            Y++;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepY = true;
                        }
                        else if (map.LevelMap[X, Y - 1].Type == 0)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            Y--;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepY = false;
                        }
                    }
                    else
                    {
                        if (Game.Rand.Next(100) < 95 && map.LevelMap[Y - 1, X].Type == 0 && map.LevelMap[Y - 1, X].MonsterID == -1)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            Y--;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepY = false;
                        }
                        else if (map.LevelMap[X, Y + 1].Type == 0)
                        {
                            map.LevelMap[Y, X].MonsterID = -1;
                            Y++;
                            map.LevelMap[Y, X].MonsterID = ID;

                            PreviousStepY = true;
                        }
                    }
                }
                if (attackTry)
                {
                    DateTime finishingTime = DateTime.Now;
                    if ((finishingTime - startingTime).TotalMilliseconds < ActionSpeed / 2)
                        Thread.Sleep((int)(ActionSpeed / 2 - (finishingTime - startingTime).TotalMilliseconds));
                }
                else
                {
                    DateTime finishingTime = DateTime.Now;
                    if ((finishingTime - startingTime).TotalMilliseconds < ActionSpeed)
                        Thread.Sleep((int)(ActionSpeed - (finishingTime - startingTime).TotalMilliseconds));
                }
            }
        }
        private void Spawn(Map map, Player player, List<Monster> monsters)
        {
            int x;
            int y;

            while (true)
            {
                x = Game.Rand.Next(map.LevelMap.GetLength(1));
                y = Game.Rand.Next(map.LevelMap.GetLength(0));
                if (map.LevelMap[y, x].Type != 1 && (player.X != x && player.Y != y) && map.LevelMap[y, x].MonsterID == -1)
                    break;
            }

            X = x;
            Y = y;

            ID = monsters.Count();
            map.LevelMap[Y, X].MonsterID = ID;
        }
    }
    public class Map
    {
        public int MapHeight;
        public int MapWidth;

        public struct MapCell
        {
            public int Type;
            public bool Seen;
            public int MonsterID;
            public int DeadMonsterID;
        }
        public struct SightMapCell
        {
            public bool InSight;
            public int Distance;
        }

        public MapCell[,] LevelMap;
        public SightMapCell[,] InSightMap;
        public SightMapCell[,] InSightMapBuffer;
        public Map(int MapHeight, int MapWidth, int Born, int Stay, int Fill, int Iterations)
        {
            this.MapHeight = MapHeight;
            this.MapWidth = MapWidth;
            GenerateMap(Born, Stay, Fill, Iterations);

            InSightMap = new SightMapCell[MapHeight, MapWidth];
            InSightMapBuffer = new SightMapCell[MapHeight, MapWidth];
        }
        private void GenerateMap(int Born, int Stay, int Fill, int Iterations)
        {
            LevelMap = new MapCell[MapHeight, MapWidth];

            for (int i = 0; i < MapHeight; i++)
                for (int j = 0; j < MapWidth; j++)
                {
                    if (Game.Rand.Next(100) < Fill)
                        LevelMap[i, j].Type = 1;
                    else
                        LevelMap[i, j].Type = 0;
                }

            MapCell[,] levelMapTemp;

            for (int k = 0; k < Iterations; k++)
            {
                levelMapTemp = new MapCell[MapHeight, MapWidth];

                for (int i = 0; i < MapHeight; i++)
                    for (int j = 0; j < MapWidth; j++)
                    {
                        int neighbors = 0;

                        if (i - 1 < 0 || j - 1 < 0) neighbors++;
                        else if (LevelMap[i - 1, j - 1].Type == 1) neighbors++;

                        if (i - 1 < 0) neighbors++;
                        else if(LevelMap[i - 1, j].Type == 1) neighbors++;

                        if (i - 1 < 0 || j + 1 >= MapWidth) neighbors++;
                        else if(LevelMap[i - 1, j + 1].Type == 1) neighbors++;

                        if (j - 1 < 0) neighbors++;
                        else if(LevelMap[i, j - 1].Type == 1) neighbors++;

                        if (j + 1 >= MapWidth) neighbors++;
                        else if (LevelMap[i, j + 1].Type == 1) neighbors++;

                        if (i + 1 >= MapHeight || j - 1 < 0) neighbors++;
                        else if (LevelMap[i + 1, j - 1].Type == 1) neighbors++;

                        if (i + 1 >= MapHeight) neighbors++;
                        else if (LevelMap[i + 1, j].Type == 1) neighbors++;

                        if (i + 1 >= MapHeight || j + 1 >= MapWidth) neighbors++;
                        else if (LevelMap[i + 1, j + 1].Type == 1) neighbors++;

                        if (LevelMap[i, j].Type == 0)
                        {
                            if (neighbors >= Born)
                                levelMapTemp[i, j].Type = 1;
                            else
                                levelMapTemp[i, j].Type = 0;
                        }
                        else
                        {
                            if (neighbors < Stay)
                                levelMapTemp[i, j].Type = 0;
                            else
                                levelMapTemp[i, j].Type = 1;
                        }
                    }

                LevelMap = (MapCell[,])levelMapTemp.Clone();
            }

            for (int i = 0; i < MapHeight; i++)
                for (int j = 0; j < MapWidth; j++)
                {
                    LevelMap[i, j].Seen = false;
                    LevelMap[i, j].MonsterID = -1;
                    LevelMap[i, j].DeadMonsterID = -1;

                    if (i == 0 || i == MapHeight - 1 || j == 0 || j == MapWidth - 1 || i == 1 || i == MapHeight - 2 || j == 1 || j == MapWidth - 2)
                        LevelMap[i, j].Type = 1;
                }
        }
    }
}
