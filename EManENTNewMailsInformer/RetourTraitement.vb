Friend Enum Retour
    CONNECTION_FAILED
    BAD_CREDENTIALS
    UNEXPECTED_ERROR
    SERVICE_UNAVAILABLE
    CONNECTED
    INTERRUPTED
    END_OF_THREAD
End Enum

Friend Class RetourTraitement

    Private compte As Compte
    Private retour As Retour

    Friend Sub New(Utilisateur As Compte, Reponse As Retour)
        compte = Utilisateur
        retour = Reponse
    End Sub

    Friend ReadOnly Property Utilisateur As Compte
        Get
            Return compte
        End Get
    End Property
    Friend ReadOnly Property Reponse As Retour
        Get
            Return retour
        End Get
    End Property

End Class
