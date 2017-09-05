Public Class DNBDisclaimer
    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        On Error Resume Next
        Process.Start("https://www.teamdev.com/dotnetbrowser")
    End Sub

    Private Sub LinkLabel2_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel2.LinkClicked
        On Error Resume Next
        Process.Start("https://www.teamdev.com/dotnetbrowser/licence-agreement")
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Close()
    End Sub

    Private Sub DNBDisclaimer_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        On Error Resume Next
        If CheckBox1.Checked Then
            IO.File.WriteAllText(Application.StartupPath + "\disclaimer.ini", "show:false")
        End If
    End Sub
End Class