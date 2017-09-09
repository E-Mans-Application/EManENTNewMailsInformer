
Public Enum Retour
    CONNECTION_FAILED
    BAD_CREDENTIALS
    UNEXPECTED_ERROR
    SERVICE_UNAVAILABLE
    CONNECTED
    INTERRUPTED
    END_OF_THREAD
    NEW_MAILS
    INITIALIZATION
End Enum

Public Class Compte

    Private user As String = Nothing
    Private pass As String = Nothing
    Private plate As Plateforme = Nothing
    Private et As Retour = Retour.INITIALIZATION

    Public Enum Plateforme
        Agora06
        Atrium
        Enteduc
    End Enum

    Public Sub New()
    End Sub

    Public Event NewMails(ByVal sender As Compte, ByVal mailCount As UShort)
    Public Event StateChanged(ByVal sender As Compte, ByVal newState As Retour)

    Public Sub ReceiveMails(count As UShort)
        RaiseEvent NewMails(Me, count)
    End Sub

    Public Sub New(UserName As String, Password As String, Plateform As Plateforme)
        Me.user = UserName
        Me.pass = Password
        Me.plate = Plateform
    End Sub

    Public Property UserName As String
        Get
            Return user
        End Get
        Set(value As String)
            user = value
        End Set
    End Property

    Public Property Password As String
        Get
            Return pass
        End Get
        Set(value As String)
            pass = value
        End Set
    End Property

    Public Property Plateform As Plateforme
        Get
            Return plate
        End Get
        Set(value As Plateforme)
            plate = value
        End Set
    End Property

    Public Overrides Function ToString() As String
        Return "Compte{" & Me.UserName & ";" & Me.Password & ";" & Me.Plateform & "}"
    End Function

    Public Property Etat As Retour
        Get
            Return et
        End Get
        Set(value As Retour)
            et = value
            RaiseEvent StateChanged(Me, value)
        End Set
    End Property

    Public Shared Function Join(CompteString As String) As Compte
        If Not CompteString.StartsWith("Compte{") Or Not CompteString.EndsWith("}") Then
            Throw New ArgumentException
            Return Nothing
        End If
        Dim a As String
        a = Mid(CompteString, 8)
        a = Mid(a, 1, a.Length - 1)
        Dim b As String() = a.Split(";"c)
        If b.Count <> 3 Then
            Throw New ArgumentException
            Return Nothing
        End If
        Try
            Return New Compte(b(0), b(1), CType(b(2), Plateforme))
        Catch ex As Exception
            Throw New ArgumentException
            Return Nothing
        End Try
    End Function

End Class
