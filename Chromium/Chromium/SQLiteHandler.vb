Imports System
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices

Namespace ChromeRecovery
    Public Class SQLiteHandler
        Private db_bytes As Byte()
        Private encoding As ULong
        Private field_names As String()
        Private master_table_entries As sqlite_master_entry()
        Private page_size As UShort
        Private SQLDataTypeSize As Byte() = New Byte() {0, 1, 2, 3, 4, 6, 8, 8, 0, 0}
        Private table_entries As table_entry()

        Public Sub New(baseName As String)
            If File.Exists(baseName) Then
                FileOpen(1, baseName, OpenMode.Binary, OpenAccess.Read, OpenShare.Shared, -1)
                Dim str = Space(LOF(1))
                FileGet(1, str, -1L, False)
                FileClose(New Integer() {1})
                db_bytes = Text.Encoding.Default.GetBytes(str)
                If Text.Encoding.Default.GetString(db_bytes, 0, 15).CompareTo("SQLite format 3") <> 0 Then
                    Throw New Exception("Not a valid SQLite 3 Database File")
                End If
                If db_bytes(&H34) <> 0 Then
                    Throw New Exception("Auto-vacuum capable database is not supported")
                End If
                page_size = CUShort(ConvertToInteger(&H10, 2))
                encoding = ConvertToInteger(&H38, 4)
                If Decimal.Compare(New Decimal(encoding), Decimal.Zero) = 0 Then
                    encoding = 1L
                End If
                ReadMasterTable(100L)
            End If
        End Sub

        Private Function ConvertToInteger(startIndex As Integer, Size As Integer) As ULong
            If Size > 8 Or Size = 0 Then
                Return 0L
            End If
            Dim num2 As ULong = 0L
            Dim num4 = Size - 1
            For i = 0 To num4
                num2 = num2 << 8 Or db_bytes(startIndex + i)
            Next
            Return num2
        End Function

        Private Function CVL(startIndex As Integer, endIndex As Integer) As Long
            endIndex += 1
            Dim buffer = New Byte(7) {}
            Dim num4 = endIndex - startIndex
            Dim flag = False
            If num4 = 0 Or num4 > 9 Then
                Return 0L
            End If
            If num4 = 1 Then
                buffer(0) = CByte(db_bytes(startIndex) And &H7F)
                Return BitConverter.ToInt64(buffer, 0)
            End If
            If num4 = 9 Then
                flag = True
            End If
            Dim num2 = 1
            Dim num3 = 7
            Dim index = 0
            If flag Then
                buffer(0) = db_bytes(endIndex - 1)
                endIndex -= 1
                index = 1
            End If
            Dim num7 = startIndex
            Dim i = endIndex - 1

            While i >= num7
                If i - 1 >= startIndex Then
                    buffer(index) = CByte(db_bytes(i) >> (num2 - 1 And 7) And &HFF >> num2 Or db_bytes(i - 1) << (num3 And 7))
                    num2 += 1
                    index += 1
                    num3 -= 1
                ElseIf Not flag Then
                    buffer(index) = CByte(db_bytes(i) >> (num2 - 1 And 7) And &HFF >> num2)
                End If

                i += -1
            End While
            Return BitConverter.ToInt64(buffer, 0)
        End Function

        Public Function GetRowCount() As Integer
            Return table_entries.Length
        End Function

        Public Function GetTableNames() As String()
            Dim strArray2 As String() = Nothing
            Dim index = 0
            Dim num3 = master_table_entries.Length - 1
            For i = 0 To num3
                If Equals(master_table_entries(i).item_type, "table") Then
                    strArray2 = CType(Utils.CopyArray(strArray2, New String(index + 1 - 1) {}), String())
                    strArray2(index) = master_table_entries(i).item_name
                    index += 1
                End If
            Next
            Return strArray2
        End Function

        Public Function GetValue(row_num As Integer, field As Integer) As String
            If row_num >= table_entries.Length Then
                Return Nothing
            End If
            If field >= table_entries(row_num).content.Length Then
                Return Nothing
            End If
            Return table_entries(row_num).content(field)
        End Function

        Public Function GetValue(row_num As Integer, field As String) As String
            Dim num = -1
            Dim length = field_names.Length - 1
            For i = 0 To length
                If field_names(i).ToLower().CompareTo(field.ToLower()) = 0 Then
                    num = i
                    Exit For
                End If
            Next
            If num = -1 Then
                Return Nothing
            End If
            Return GetValue(row_num, num)
        End Function

        Private Function GVL(startIndex As Integer) As Integer
            If startIndex > db_bytes.Length Then
                Return 0
            End If
            Dim num3 = startIndex + 8
            For i = startIndex To num3
                If i > db_bytes.Length - 1 Then
                    Return 0
                End If
                If (db_bytes(i) And &H80) <> &H80 Then
                    Return i
                End If
            Next
            Return startIndex + 8
        End Function

        Private Function IsOdd(value As Long) As Boolean
            Return (value And 1L) = 1L
        End Function

        Private Sub ReadMasterTable(Offset As ULong)
            If db_bytes(Offset) = 13 Then
                Dim num2 As UShort = Convert.ToUInt16(Decimal.Subtract(New Decimal(ConvertToInteger(Convert.ToInt32(Decimal.Add(New Decimal(Offset), 3D)), 2)), Decimal.One))
                Dim length = 0
                If master_table_entries IsNot Nothing Then
                    length = master_table_entries.Length
                    master_table_entries = CType(Utils.CopyArray(master_table_entries, New sqlite_master_entry(master_table_entries.Length + num2 + 1 - 1) {}), sqlite_master_entry())
                Else
                    master_table_entries = New sqlite_master_entry(num2 + 1 - 1) {}
                End If
                Dim num13 As Integer = num2
                For i = 0 To num13
                    Dim num As ULong = ConvertToInteger(Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(Offset), 8D), New Decimal(i * 2))), 2)
                    If Decimal.Compare(New Decimal(Offset), 100D) <> 0 Then
                        num += Offset
                    End If
                    Dim endIndex = GVL(num)
                    Dim num7 = CVL(num, endIndex)
                    Dim num6 As Integer = GVL(Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(num), Decimal.Subtract(New Decimal(endIndex), New Decimal(num))), Decimal.One)))
                    master_table_entries(length + i).row_id = CVL(Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(num), Decimal.Subtract(New Decimal(endIndex), New Decimal(num))), Decimal.One)), num6)
                    num = Convert.ToUInt64(Decimal.Add(Decimal.Add(New Decimal(num), Decimal.Subtract(New Decimal(num6), New Decimal(num))), Decimal.One))
                    endIndex = GVL(num)
                    num6 = endIndex
                    Dim num5 = CVL(num, endIndex)
                    Dim numArray = New Long(4) {}
                    Dim index = 0
                    Do
                        endIndex = num6 + 1
                        num6 = GVL(endIndex)
                        numArray(index) = CVL(endIndex, num6)
                        If numArray(index) > 9L Then
                            If IsOdd(numArray(index)) Then
                                numArray(index) = CLng(Math.Round((numArray(index) - 13L) / 2.0))
                            Else
                                numArray(index) = CLng(Math.Round((numArray(index) - 12L) / 2.0))
                            End If
                        Else
                            numArray(index) = SQLDataTypeSize(numArray(index))
                        End If
                        index += 1
                    Loop While index <= 4
                    If Decimal.Compare(New Decimal(encoding), Decimal.One) = 0 Then
                        master_table_entries(length + i).item_type = Text.Encoding.Default.GetString(db_bytes, Convert.ToInt32(Decimal.Add(New Decimal(num), New Decimal(num5))), numArray(0))
                    ElseIf Decimal.Compare(New Decimal(encoding), 2D) = 0 Then
                        master_table_entries(length + i).item_type = Text.Encoding.Unicode.GetString(db_bytes, Convert.ToInt32(Decimal.Add(New Decimal(num), New Decimal(num5))), numArray(0))
                    ElseIf Decimal.Compare(New Decimal(encoding), 3D) = 0 Then
                        master_table_entries(length + i).item_type = Text.Encoding.BigEndianUnicode.GetString(db_bytes, Convert.ToInt32(Decimal.Add(New Decimal(num), New Decimal(num5))), numArray(0))
                    End If
                    If Decimal.Compare(New Decimal(encoding), Decimal.One) = 0 Then
                        master_table_entries(length + i).item_name = Text.Encoding.Default.GetString(db_bytes, Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(num), New Decimal(num5)), New Decimal(numArray(0)))), numArray(1))
                    ElseIf Decimal.Compare(New Decimal(encoding), 2D) = 0 Then
                        master_table_entries(length + i).item_name = Text.Encoding.Unicode.GetString(db_bytes, Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(num), New Decimal(num5)), New Decimal(numArray(0)))), numArray(1))
                    ElseIf Decimal.Compare(New Decimal(encoding), 3D) = 0 Then
                        master_table_entries(length + i).item_name = Text.Encoding.BigEndianUnicode.GetString(db_bytes, Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(num), New Decimal(num5)), New Decimal(numArray(0)))), numArray(1))
                    End If
                    master_table_entries(length + i).root_num = CLng(ConvertToInteger(Convert.ToInt32(Decimal.Add(Decimal.Add(Decimal.Add(Decimal.Add(New Decimal(num), New Decimal(num5)), New Decimal(numArray(0))), New Decimal(numArray(1))), New Decimal(numArray(2)))), numArray(3)))
                    If Decimal.Compare(New Decimal(encoding), Decimal.One) = 0 Then
                        master_table_entries(length + i).sql_statement = Text.Encoding.Default.GetString(db_bytes, Convert.ToInt32(Decimal.Add(Decimal.Add(Decimal.Add(Decimal.Add(Decimal.Add(New Decimal(num), New Decimal(num5)), New Decimal(numArray(0))), New Decimal(numArray(1))), New Decimal(numArray(2))), New Decimal(numArray(3)))), numArray(4))
                    ElseIf Decimal.Compare(New Decimal(encoding), 2D) = 0 Then
                        master_table_entries(length + i).sql_statement = Text.Encoding.Unicode.GetString(db_bytes, Convert.ToInt32(Decimal.Add(Decimal.Add(Decimal.Add(Decimal.Add(Decimal.Add(New Decimal(num), New Decimal(num5)), New Decimal(numArray(0))), New Decimal(numArray(1))), New Decimal(numArray(2))), New Decimal(numArray(3)))), numArray(4))
                    ElseIf Decimal.Compare(New Decimal(encoding), 3D) = 0 Then
                        master_table_entries(length + i).sql_statement = Text.Encoding.BigEndianUnicode.GetString(db_bytes, Convert.ToInt32(Decimal.Add(Decimal.Add(Decimal.Add(Decimal.Add(Decimal.Add(New Decimal(num), New Decimal(num5)), New Decimal(numArray(0))), New Decimal(numArray(1))), New Decimal(numArray(2))), New Decimal(numArray(3)))), numArray(4))
                    End If
                Next
            ElseIf db_bytes(Offset) = 5 Then
                Dim num11 As UShort = Convert.ToUInt16(Decimal.Subtract(New Decimal(ConvertToInteger(Convert.ToInt32(Decimal.Add(New Decimal(Offset), 3D)), 2)), Decimal.One))
                Dim num14 As Integer = num11
                For j = 0 To num14
                    Dim startIndex As UShort = CUShort(ConvertToInteger(Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(Offset), 12D), New Decimal(j * 2))), 2))
                    If Decimal.Compare(New Decimal(Offset), 100D) = 0 Then
                        ReadMasterTable(Convert.ToUInt64(Decimal.Multiply(Decimal.Subtract(New Decimal(ConvertToInteger(startIndex, 4)), Decimal.One), New Decimal(page_size))))
                    Else
                        ReadMasterTable(Convert.ToUInt64(Decimal.Multiply(Decimal.Subtract(New Decimal(ConvertToInteger(Offset + startIndex, 4)), Decimal.One), New Decimal(page_size))))
                    End If
                Next
                ReadMasterTable(Convert.ToUInt64(Decimal.Multiply(Decimal.Subtract(New Decimal(ConvertToInteger(Convert.ToInt32(Decimal.Add(New Decimal(Offset), 8D)), 4)), Decimal.One), New Decimal(page_size))))
            End If
        End Sub

        Public Function ReadTable(TableName As String) As Boolean
            Dim index = -1
            Dim length = master_table_entries.Length - 1
            For i = 0 To length
                If master_table_entries(i).item_name.ToLower().CompareTo(TableName.ToLower()) = 0 Then
                    index = i
                    Exit For
                End If
            Next
            If index = -1 Then
                Return False
            End If
            Dim strArray = master_table_entries(index).sql_statement.Substring(master_table_entries(index).sql_statement.IndexOf("(") + 1).Split(New Char() {","c})
            Dim num6 = strArray.Length - 1
            For j = 0 To num6
                strArray(j) = strArray(j).TrimStart()
                Dim num4 = strArray(j).IndexOf(" ")
                If num4 > 0 Then
                    strArray(j) = strArray(j).Substring(0, num4)
                End If
                If strArray(j).IndexOf("UNIQUE") = 0 Then
                    Exit For
                End If
                field_names = CType(Utils.CopyArray(field_names, New String(j + 1 - 1) {}), String())
                field_names(j) = strArray(j)
            Next
            Return ReadTableFromOffset((master_table_entries(index).root_num - 1L) * page_size)
        End Function

        Private Function ReadTableFromOffset(Offset As ULong) As Boolean
            If db_bytes(Offset) = 13 Then
                Dim num2 As Integer = Convert.ToInt32(Decimal.Subtract(New Decimal(ConvertToInteger(Convert.ToInt32(Decimal.Add(New Decimal(Offset), 3D)), 2)), Decimal.One))
                Dim length = 0
                If table_entries IsNot Nothing Then
                    length = table_entries.Length
                    table_entries = CType(Utils.CopyArray(table_entries, New table_entry(table_entries.Length + num2 + 1 - 1) {}), table_entry())
                Else
                    table_entries = New table_entry(num2 + 1 - 1) {}
                End If
                Dim num16 = num2
                For i = 0 To num16
                    Dim _fieldArray As record_header_field() = Nothing
                    Dim num As ULong = ConvertToInteger(Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(Offset), 8D), New Decimal(i * 2))), 2)
                    If Decimal.Compare(New Decimal(Offset), 100D) <> 0 Then
                        num += Offset
                    End If
                    Dim endIndex = GVL(num)
                    Dim num9 = CVL(num, endIndex)
                    Dim num8 As Integer = GVL(Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(num), Decimal.Subtract(New Decimal(endIndex), New Decimal(num))), Decimal.One)))
                    table_entries(length + i).row_id = CVL(Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(num), Decimal.Subtract(New Decimal(endIndex), New Decimal(num))), Decimal.One)), num8)
                    num = Convert.ToUInt64(Decimal.Add(Decimal.Add(New Decimal(num), Decimal.Subtract(New Decimal(num8), New Decimal(num))), Decimal.One))
                    endIndex = GVL(num)
                    num8 = endIndex
                    Dim num7 = CVL(num, endIndex)
                    Dim num10 As Long = Convert.ToInt64(Decimal.Add(Decimal.Subtract(New Decimal(num), New Decimal(endIndex)), Decimal.One))
                    For j As Integer = 0 To num7 - 1
                        _fieldArray = CType(Utils.CopyArray(_fieldArray, New record_header_field(j + 1 - 1) {}), record_header_field())
                        endIndex = num8 + 1
                        num8 = GVL(endIndex)
                        _fieldArray(j).type = CVL(endIndex, num8)
                        If _fieldArray(j).type > 9L Then
                            If IsOdd(_fieldArray(j).type) Then
                                _fieldArray(j).size = CLng(Math.Round((_fieldArray(j).type - 13L) / 2.0))
                            Else
                                _fieldArray(j).size = CLng(Math.Round((_fieldArray(j).type - 12L) / 2.0))
                            End If
                        Else
                            _fieldArray(j).size = SQLDataTypeSize(_fieldArray(j).type)
                        End If
                        num10 = num10 + (num8 - endIndex) + 1L
                    Next
                    table_entries(length + i).content = New String(_fieldArray.Length - 1 + 1 - 1) {}
                    Dim num4 = 0
                    Dim num17 = _fieldArray.Length - 1
                    For k = 0 To num17
                        If _fieldArray(k).type > 9L Then
                            If Not IsOdd(_fieldArray(k).type) Then
                                If Decimal.Compare(New Decimal(encoding), Decimal.One) = 0 Then
                                    table_entries(length + i).content(k) = Text.Encoding.Default.GetString(db_bytes, Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(num), New Decimal(num7)), New Decimal(num4))), _fieldArray(k).size)
                                ElseIf Decimal.Compare(New Decimal(encoding), 2D) = 0 Then
                                    table_entries(length + i).content(k) = Text.Encoding.Unicode.GetString(db_bytes, Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(num), New Decimal(num7)), New Decimal(num4))), _fieldArray(k).size)
                                ElseIf Decimal.Compare(New Decimal(encoding), 3D) = 0 Then
                                    table_entries(length + i).content(k) = Text.Encoding.BigEndianUnicode.GetString(db_bytes, Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(num), New Decimal(num7)), New Decimal(num4))), _fieldArray(k).size)
                                End If
                            Else
                                table_entries(length + i).content(k) = Text.Encoding.Default.GetString(db_bytes, Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(num), New Decimal(num7)), New Decimal(num4))), _fieldArray(k).size)
                            End If
                        Else
                            table_entries(length + i).content(k) = Conversions.ToString(ConvertToInteger(Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(num), New Decimal(num7)), New Decimal(num4))), _fieldArray(k).size))
                        End If
                        num4 += CInt(_fieldArray(k).size)
                    Next
                Next
            ElseIf db_bytes(Offset) = 5 Then
                Dim num14 As UShort = Convert.ToUInt16(Decimal.Subtract(New Decimal(ConvertToInteger(Convert.ToInt32(Decimal.Add(New Decimal(Offset), 3D)), 2)), Decimal.One))
                Dim num18 As Integer = num14
                For m = 0 To num18
                    Dim num13 As UShort = CUShort(ConvertToInteger(Convert.ToInt32(Decimal.Add(Decimal.Add(New Decimal(Offset), 12D), New Decimal(m * 2))), 2))
                    ReadTableFromOffset(Convert.ToUInt64(Decimal.Multiply(Decimal.Subtract(New Decimal(ConvertToInteger(Offset + num13, 4)), Decimal.One), New Decimal(page_size))))
                Next
                ReadTableFromOffset(Convert.ToUInt64(Decimal.Multiply(Decimal.Subtract(New Decimal(ConvertToInteger(Convert.ToInt32(Decimal.Add(New Decimal(Offset), 8D)), 4)), Decimal.One), New Decimal(page_size))))
            End If
            Return True
        End Function

        <StructLayout(LayoutKind.Sequential)>
        Private Structure record_header_field
            Public size As Long
            Public type As Long
        End Structure

        <StructLayout(LayoutKind.Sequential)>
        Private Structure sqlite_master_entry
            Public row_id As Long
            Public item_type As String
            Public item_name As String
            Public astable_name As String
            Public root_num As Long
            Public sql_statement As String
        End Structure

        <StructLayout(LayoutKind.Sequential)>
        Private Structure table_entry
            Public row_id As Long
            Public content As String()
        End Structure
    End Class
End Namespace
