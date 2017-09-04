Imports System.Windows.Forms

Public Class Dialog4


    Private Sub Dialog3_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        On Error Resume Next
        My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Hand)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        On Error Resume Next
        Me.Close()
    End Sub
End Class
