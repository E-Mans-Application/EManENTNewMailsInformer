Imports System.Windows.Forms

Public Class Dialog5


    Private Sub Dialog3_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        On Error Resume Next
        Location = New Point(Screen.PrimaryScreen.WorkingArea.Width - Me.Width, Screen.PrimaryScreen.WorkingArea.Height - Me.Height)
        My.Computer.Audio.Play(My.Resources.newemail, AudioPlayMode.Background)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        On Error Resume Next
        Me.Close()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Me.Opacity -= 0.01
        If Me.Opacity = 0 Then
            Close()
        End If
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        Timer2.Stop()
        Timer1.Start()
    End Sub
End Class
