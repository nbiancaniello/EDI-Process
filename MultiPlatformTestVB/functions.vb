Public Class functions
    Shared Function getLeft(ByVal msg As String) As String
        Return Left(msg, 9)
    End Function

    Shared Function getRight(ByVal msg As String) As String
        Return Right(msg, 3)
    End Function

    Shared Function getMid(ByVal msg As String) As String
        Return Mid(msg, 5, 3)
    End Function

    Shared Function getInstr(ByVal msg As String) As String
        Return InStr(11, msg, "str")
    End Function

    Shared Function getReplace(ByVal msg As String) As String
        Return Replace(msg, "a", "*b", 1, 2)
    End Function
End Class
