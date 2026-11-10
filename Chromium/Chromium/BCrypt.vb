Imports System
Imports System.Runtime.InteropServices

Namespace ChromeRecovery
    Public Module BCrypt
        Public Const ERROR_SUCCESS As UInteger = &H0
        Public Const BCRYPT_PAD_PSS As UInteger = 8
        Public Const BCRYPT_PAD_OAEP As UInteger = 4

        Public ReadOnly BCRYPT_KEY_DATA_BLOB_MAGIC As Byte() = BitConverter.GetBytes(&H4D42444B)

        Public ReadOnly BCRYPT_OBJECT_LENGTH As String = "ObjectLength"
        Public ReadOnly BCRYPT_CHAIN_MODE_GCM As String = "ChainingModeGCM"
        Public ReadOnly BCRYPT_AUTH_TAG_LENGTH As String = "AuthTagLength"
        Public ReadOnly BCRYPT_CHAINING_MODE As String = "ChainingMode"
        Public ReadOnly BCRYPT_KEY_DATA_BLOB As String = "KeyDataBlob"
        Public ReadOnly BCRYPT_AES_ALGORITHM As String = "AES"

        Public ReadOnly MS_PRIMITIVE_PROVIDER As String = "Microsoft Primitive Provider"

        Public ReadOnly BCRYPT_AUTH_MODE_CHAIN_CALLS_FLAG As Integer = &H1
        Public ReadOnly BCRYPT_INIT_AUTH_MODE_INFO_VERSION As Integer = &H1

        Public ReadOnly STATUS_AUTH_TAG_MISMATCH As UInteger = &HC000A002UI

        <StructLayout(LayoutKind.Sequential)>
        Public Structure BCRYPT_PSS_PADDING_INFO
            Public Sub New(pszAlgId As String, cbSalt As Integer)
                pszAlgId = pszAlgId
                cbSalt = cbSalt
            End Sub

            <MarshalAs(UnmanagedType.LPWStr)>
            Public pszAlgId As String
            Public cbSalt As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential)>
        Public Structure BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO
            Implements IDisposable
            Public cbSize As Integer
            Public dwInfoVersion As Integer
            Public pbNonce As IntPtr
            Public cbNonce As Integer
            Public pbAuthData As IntPtr
            Public cbAuthData As Integer
            Public pbTag As IntPtr
            Public cbTag As Integer
            Public pbMacContext As IntPtr
            Public cbMacContext As Integer
            Public cbAAD As Integer
            Public cbData As Long
            Public dwFlags As Integer

            Public Sub New(iv As Byte(), aad As Byte(), tag As Byte())
                Me.New()
                dwInfoVersion = BCRYPT_INIT_AUTH_MODE_INFO_VERSION
                cbSize = Marshal.SizeOf(GetType(BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO))

                If iv IsNot Nothing Then
                    cbNonce = iv.Length
                    pbNonce = Marshal.AllocHGlobal(cbNonce)
                    Marshal.Copy(iv, 0, pbNonce, cbNonce)
                End If

                If aad IsNot Nothing Then
                    cbAuthData = aad.Length
                    pbAuthData = Marshal.AllocHGlobal(cbAuthData)
                    Marshal.Copy(aad, 0, pbAuthData, cbAuthData)
                End If

                If tag IsNot Nothing Then
                    cbTag = tag.Length
                    pbTag = Marshal.AllocHGlobal(cbTag)
                    Marshal.Copy(tag, 0, pbTag, cbTag)

                    cbMacContext = tag.Length
                    pbMacContext = Marshal.AllocHGlobal(cbMacContext)
                End If
            End Sub

            Public Sub Dispose() Implements IDisposable.Dispose
                If pbNonce <> IntPtr.Zero Then Marshal.FreeHGlobal(pbNonce)
                If pbTag <> IntPtr.Zero Then Marshal.FreeHGlobal(pbTag)
                If pbAuthData <> IntPtr.Zero Then Marshal.FreeHGlobal(pbAuthData)
                If pbMacContext <> IntPtr.Zero Then Marshal.FreeHGlobal(pbMacContext)
            End Sub
        End Structure

        <StructLayout(LayoutKind.Sequential)>
        Public Structure BCRYPT_KEY_LENGTHS_STRUCT
            Public dwMinLength As Integer
            Public dwMaxLength As Integer
            Public dwIncrement As Integer
        End Structure

        <StructLayout(LayoutKind.Sequential)>
        Public Structure BCRYPT_OAEP_PADDING_INFO
            Public Sub New(alg As String)
                pszAlgId = alg
                pbLabel = IntPtr.Zero
                cbLabel = 0
            End Sub

            <MarshalAs(UnmanagedType.LPWStr)>
            Public pszAlgId As String
            Public pbLabel As IntPtr
            Public cbLabel As Integer
        End Structure

        <DllImport("bcrypt.dll")>
        Public Function BCryptOpenAlgorithmProvider(<Out> ByRef phAlgorithm As IntPtr,
                                                              <MarshalAs(UnmanagedType.LPWStr)> pszAlgId As String,
                                                                                                                            <MarshalAs(UnmanagedType.LPWStr)> pszImplementation As String, dwFlags As UInteger) As UInteger
        End Function

        <DllImport("bcrypt.dll")>
        Public Function BCryptCloseAlgorithmProvider(hAlgorithm As IntPtr, flags As UInteger) As UInteger
        End Function

        <DllImport("bcrypt.dll", EntryPoint:="BCryptGetProperty")>
        Public Function BCryptGetProperty(hObject As IntPtr,
        <MarshalAs(UnmanagedType.LPWStr)> pszProperty As String, pbOutput As Byte(), cbOutput As Integer, ByRef pcbResult As Integer, flags As UInteger) As UInteger
        End Function

        <DllImport("bcrypt.dll", EntryPoint:="BCryptSetProperty")>
        Friend Function BCryptSetAlgorithmProperty(hObject As IntPtr,
        <MarshalAs(UnmanagedType.LPWStr)> pszProperty As String, pbInput As Byte(), cbInput As Integer, dwFlags As Integer) As UInteger
        End Function


        <DllImport("bcrypt.dll")>
        Public Function BCryptImportKey(hAlgorithm As IntPtr, hImportKey As IntPtr,
                                                                                                                                                                                                                                                                                                                                                                                                               <MarshalAs(UnmanagedType.LPWStr)> pszBlobType As String, <Out> ByRef phKey As IntPtr, pbKeyObject As IntPtr, cbKeyObject As Integer, pbInput As Byte(), cbInput As Integer, dwFlags As UInteger) As UInteger 'blob of type BCRYPT_KEY_DATA_BLOB + raw key data = (dwMagic (4 bytes) | uint dwVersion (4 bytes) | cbKeyData (4 bytes) | data)
        End Function

        <DllImport("bcrypt.dll")>
        Public Function BCryptDestroyKey(hKey As IntPtr) As UInteger
        End Function

        <DllImport("bcrypt.dll")>
        Public Function BCryptEncrypt(hKey As IntPtr, pbInput As Byte(), cbInput As Integer, ByRef pPaddingInfo As BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO, pbIV As Byte(), cbIV As Integer, pbOutput As Byte(), cbOutput As Integer, ByRef pcbResult As Integer, dwFlags As UInteger) As UInteger
        End Function

        <DllImport("bcrypt.dll")>
        Friend Function BCryptDecrypt(hKey As IntPtr, pbInput As Byte(), cbInput As Integer, ByRef pPaddingInfo As BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO, pbIV As Byte(), cbIV As Integer, pbOutput As Byte(), cbOutput As Integer, ByRef pcbResult As Integer, dwFlags As Integer) As UInteger
        End Function
    End Module

End Namespace
