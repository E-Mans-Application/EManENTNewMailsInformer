Public Class LoginForm2

    ' TODO: insérez le code pour effectuer une authentification personnalisée à l'aide du nom d'utilisateur et du mot de passe fournis 
    ' (Voir http://go.microsoft.com/fwlink/?LinkId=35339).  
    ' L'objet Principal personnalisé peut ensuite être associé à l'objet Principal du thread actuel comme suit : 
    '     My.User.CurrentPrincipal = CustomPrincipal
    ' CustomPrincipal est l'implémentation IPrincipal utilisée pour effectuer l'authentification. 
    ' Par la suite, My.User retournera les informations d'identité encapsulées dans l'objet CustomPrincipal
    ' telles que le nom d'utilisateur, le nom complet, etc.

    Friend user As Compte
    Friend editedUser As Compte

    Private Sub OK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK.Click
        On Error Resume Next
        If UsernameTextBox.Text = "" Or ComboBox1.SelectedItem Is "" Then
            MsgBox("Veuillez remplir toutes les informations demandées.", MsgBoxStyle.Critical, "Erreur")
            Exit Sub
        End If
        If Form1.utilisateurs.Exists(Function(x) x.UserName = UsernameTextBox.Text And CInt(x.Plateform) = ComboBox1.SelectedIndex) And Not (UsernameTextBox.Text = user.UserName And ComboBox1.SelectedIndex = CInt(user.Plateform)) Then
            MsgBox("Cet utilisateur a déjà été ajouté.", MsgBoxStyle.Critical, "Erreur")
            Exit Sub
        End If
        Form1.utilisateurs(Form1.utilisateurs.IndexOf(user)).UserName = UsernameTextBox.Text
        If Not PasswordTextBox.Text = "" Then
            Form1.utilisateurs(Form1.utilisateurs.IndexOf(user)).Password = Form1.AES_Encrypt(PasswordTextBox.Text, "R)=@V74$9(HpY9u?b%D#~FJc6jy85{*3J8R6kd^a.Uk25!.JP[y-i=8" & UsernameTextBox.Text & "nUb66=&S%Tv4YBnYa34_Jx3;a4L896itJg=wwW7?YUE4y")
        End If
        Form1.utilisateurs(Form1.utilisateurs.IndexOf(user)).Plateform = CType(ComboBox1.SelectedIndex, Compte.Plateforme)
        Form1.RefreshUsers(False)
        editedUser = Form1.utilisateurs(Form1.utilisateurs.IndexOf(user))
        DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel.Click
        On Error Resume Next
        Me.Close()
    End Sub

    Private Sub UsernameTextBox_TextChanged(sender As Object, e As EventArgs) Handles UsernameTextBox.TextChanged
        Dim forbiddenChars As Char() = "<>:""/\|?*".ToCharArray()
        Dim invalidInput As Boolean = False
        For Each ch In forbiddenChars
            If UsernameTextBox.Text.Contains(ch) Then
                UsernameTextBox.Text = UsernameTextBox.Text.Replace(ch, String.Empty)
                invalidInput = True
            End If
        Next
        If invalidInput Then
            MsgBox("Vous avez tapé un caractère incorrect. Le nom d'utilisateur ne peut pas contenir les caractères <>:""/\|?*", MsgBoxStyle.Critical, "Erreur")
        End If
    End Sub
End Class
