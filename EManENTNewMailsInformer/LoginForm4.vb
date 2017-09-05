Public Class LoginForm4

    ' TODO: insérez le code pour effectuer une authentification personnalisée à l'aide du nom d'utilisateur et du mot de passe fournis 
    ' (Voir http://go.microsoft.com/fwlink/?LinkId=35339).  
    ' L'objet Principal personnalisé peut ensuite être associé à l'objet Principal du thread actuel comme suit : 
    '     My.User.CurrentPrincipal = CustomPrincipal
    ' CustomPrincipal est l'implémentation IPrincipal utilisée pour effectuer l'authentification. 
    ' Par la suite, My.User retournera les informations d'identité encapsulées dans l'objet CustomPrincipal
    ' telles que le nom d'utilisateur, le nom complet, etc.

    Private Sub OK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK.Click
        Form1.password = PasswordTextBox.Text
        PasswordTextBox.Clear()
        DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel.Click
        DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub LoginForm4_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        PasswordTextBox.Clear()
    End Sub
End Class
