Public Structure UtilisateurBrowser
    Private u As Compte
    Private b As DotNetBrowser.Browser
    Friend Property Utilisateur As Compte
        Get
            Return u
        End Get
        Set(value As Compte)
            u = value
        End Set
    End Property

    Friend Property Browser As DotNetBrowser.Browser
        Get
            Return b
        End Get
        Set(value As DotNetBrowser.Browser)
            b = Browser
        End Set
    End Property

    Friend Sub New(ByVal Utilisateur As Compte, ByVal Browser As DotNetBrowser.Browser)
        u = Utilisateur
        b = Browser
    End Sub
End Structure
