Imports Microsoft.VisualBasic.ApplicationServices

Namespace My
    ' Les événements suivants sont disponibles pour MyApplication :
    ' Startup : déclenché au démarrage de l'application avant la création du formulaire de démarrage.
    ' Shutdown : déclenché après la fermeture de tous les formulaires de l'application. Cet événement n'est pas déclenché si l'application se termine de façon anormale.
    ' UnhandledException : déclenché si l'application rencontre une exception non gérée.
    ' StartupNextInstance : déclenché lors du lancement d'une application à instance unique et si cette application est déjà active. 
    ' NetworkAvailabilityChanged : déclenché lorsque la connexion réseau est connectée ou déconnectée.
    Partial Friend Class MyApplication


        Friend QuietStart As Boolean = False

        Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
            If Not IO.File.Exists(System.Windows.Forms.Application.StartupPath & "\DotNetBrowser.dll") Then
                MsgBox("La dépendance DotNetBrowser.dll est introuvable. Assurez-vous qu'elle soit dans le même répertoire que le programme puis réesayez.", MsgBoxStyle.Critical, "EManENTNewMailsInformer - Echec du démarrage.")
                End
            End If
            For Each comm In Application.CommandLineArgs
                If comm = "/q" Then
                    QuietStart = True
                    Form1.Opacity = 0
                    Form1.ShowInTaskbar = False
                    Exit For
                End If
            Next
        End Sub

        Private Sub MyApplication_StartupNextInstance(sender As Object, e As StartupNextInstanceEventArgs) Handles Me.StartupNextInstance
            If OpenForms.Count > 0 Then
                Form1.Show()
                Form1.TopMost = True
                Form1.TopMost = False
                Form1.BringToFront()
                Form1.Invoke(Sub() MsgBox("Le programme est déjà en cours d'exécution.", MsgBoxStyle.Exclamation, "Erreur"))
            Else
                MsgBox("Le programme est déjà en cours d'exécution.", MsgBoxStyle.Exclamation, "Erreur")
            End If
        End Sub
    End Class
End Namespace
