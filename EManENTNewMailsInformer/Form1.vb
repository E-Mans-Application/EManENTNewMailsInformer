Option Strict On
Option Explicit On
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports Microsoft.Win32
Imports DotNetBrowser
Imports DotNetBrowser.WinForms
Imports System.IO
Imports System.Security

Public Class Form1

    Dim NewPoint As New Point
    Dim X, Y As Integer

    Friend utilisateurs As New List(Of Compte)
    Friend ReadOnly urlsplatforms As String() = {"https://www.agora06.fr/", "https://www.atrium-paca.fr/", "https://ent.enteduc.fr/"}
    Friend UtilisateursRecherche As New List(Of Compte)
    Friend utilisateursThreads As New List(Of Thread)
    Friend utilisateursBrowser As New List(Of UtilisateurBrowser)
    Friend retoursThread As New Thread(AddressOf GererRetours)
    Friend retours As New List(Of RetourTraitement)
    Private quitting As Boolean

    Public Function AES_Encrypt(ByVal input As String, ByVal pass As String) As String
        Dim AES As New System.Security.Cryptography.RijndaelManaged
        Dim Hash_AES As New System.Security.Cryptography.MD5CryptoServiceProvider
        Dim encrypted As String = ""
        Try
            Dim hash(31) As Byte
            Dim temp As Byte() = Hash_AES.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(pass))
            Array.Copy(temp, 0, hash, 0, 16)
            Array.Copy(temp, 0, hash, 15, 16)
            AES.Key = hash
            AES.Mode = System.Security.Cryptography.CipherMode.ECB
            Dim DESEncrypter As System.Security.Cryptography.ICryptoTransform = AES.CreateEncryptor
            Dim Buffer As Byte() = System.Text.ASCIIEncoding.ASCII.GetBytes(input)
            encrypted = Convert.ToBase64String(DESEncrypter.TransformFinalBlock(Buffer, 0, Buffer.Length))
            Return encrypted
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function AES_Decrypt(ByVal input As String, ByVal pass As String) As String
        Dim AES As New System.Security.Cryptography.RijndaelManaged
        Dim Hash_AES As New System.Security.Cryptography.MD5CryptoServiceProvider
        Dim decrypted As String = ""
        Try
            Dim hash(31) As Byte
            Dim temp As Byte() = Hash_AES.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(pass))
            Array.Copy(temp, 0, hash, 0, 16)
            Array.Copy(temp, 0, hash, 15, 16)
            AES.Key = hash
            AES.Mode = System.Security.Cryptography.CipherMode.ECB
            Dim DESDecrypter As System.Security.Cryptography.ICryptoTransform = AES.CreateDecryptor
            Dim Buffer As Byte() = Convert.FromBase64String(input)
            decrypted = System.Text.ASCIIEncoding.ASCII.GetString(DESDecrypter.TransformFinalBlock(Buffer, 0, Buffer.Length))
            Return decrypted
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Friend Sub SaveUsers()
        Dim cont As String = Nothing
        For Each user In utilisateurs
            cont &= user.ToString()
        Next
        cont = AES_Encrypt(cont, "c#k&>*6<r5jGa889]kw4{79_>2gbG5tK]cAM246eVs?7;3Ft]5Di_533dM(3zbYnZ}*jb4Ua>96zH2bHr35j#H?Px$*LF_G9GhDt)TF[*,YMJ!&A;p987BfzE/qM2h779x^7MVpF:sA<J~9ySa;82g3/}WYe3ZM_]e26PP/*f7Wj}-D8x22>qr3n*K<[]42S^875u9K[74r}XG*8KG83F8zVbEeD45?!C9bS*UicnQRX9n!G@-v]]*aGZi@cp(u5")
        Try
            IO.File.WriteAllText(Application.StartupPath & "\comptes.ini", cont)
        Catch ex As Exception
        End Try
    End Sub

    Friend Sub LoadUsers()
        Try
            If Not IO.File.Exists(Application.StartupPath & "\comptes.ini") Then Exit Sub
            Dim cont As String = IO.File.ReadAllText(Application.StartupPath & "\comptes.ini")
            cont = AES_Decrypt(cont, "c#k&>*6<r5jGa889]kw4{79_>2gbG5tK]cAM246eVs?7;3Ft]5Di_533dM(3zbYnZ}*jb4Ua>96zH2bHr35j#H?Px$*LF_G9GhDt)TF[*,YMJ!&A;p987BfzE/qM2h779x^7MVpF:sA<J~9ySa;82g3/}WYe3ZM_]e26PP/*f7Wj}-D8x22>qr3n*K<[]42S^875u9K[74r}XG*8KG83F8zVbEeD45?!C9bS*UicnQRX9n!G@-v]]*aGZi@cp(u5")
            Dim com As String() = cont.Split("}"c)
            For Each Co In com
                If Co.StartsWith("Compte{") Then
                    utilisateurs.Add(EManENTNewMailsInformer.Compte.Join(Co & "}"))
                End If
            Next
            RefreshUsers()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Panel2_MouseEnter(sender As Object, e As EventArgs) Handles Panel1.MouseEnter, Panel2.MouseEnter, Panel3.MouseEnter, Panel4.MouseEnter
        On Error Resume Next
        If Not DirectCast(sender, Panel).Enabled Then Exit Sub
        DirectCast(sender, Panel).BackColor = Color.FromArgb(192, 210, 192)
    End Sub

    Private Sub Panel2_MouseLeave(sender As Object, e As EventArgs) Handles Panel1.MouseLeave, Panel2.MouseLeave, Panel3.MouseLeave, Panel4.MouseLeave
        On Error Resume Next
        If Not DirectCast(sender, Panel).Enabled Then Exit Sub
        DirectCast(sender, Panel).BackColor = DefaultBackColor
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        On Error Resume Next
        LoadUsers()
        CheckStartup()
        retoursThread.Start()
    End Sub

    Friend Sub RefreshUsers(Optional forceNewThreads As Boolean = True)
        On Error Resume Next
        If utilisateurs.Count = 0 Then
            Label1.Text = "Veuillez configurer un utilisateur."
            Label1.ForeColor = Color.DarkOrange
            PictureBox1.Image = My.Resources.male_user_warning_256
            Exit Sub
        End If
        Label1.Text = "Initialisation en cours..."
        Label1.ForeColor = Color.Blue
        PictureBox1.Image = My.Resources.attente
        SaveUsers()
        DataGridView1.Rows.Clear()
        If forceNewThreads Then
            For Each th In utilisateursThreads
                th.Interrupt()
                th.Join()
            Next
            utilisateursThreads.Clear()
        End If
        For Each user In utilisateurs
            DataGridView1.Rows.Add(New Object() {My.Resources.attente, user.UserName, user.Plateform})
            If forceNewThreads Then
                Dim th As New Thread(AddressOf RechercheMails)
                th.Name = user.UserName
                th.Start(user)
                utilisateursThreads.Add(th)
            End If
        Next
    End Sub

    Private Sub NewMails(ByVal sender As Compte, ByVal count As UShort)
        If count > 0 Then
            Me.Invoke(Sub()
                          DataGridView1.Rows(utilisateurs.IndexOf(sender)).Cells(0).Value = My.Resources.nouveauxmessages
                          DataGridView1.Rows(utilisateurs.IndexOf(sender)).Cells(0).ToolTipText = "Nouveaux messages !"
                          If UtilisateursRecherche.Count = utilisateurs.Count Then
                              Label1.Text = "Nouveaux messages."
                              Label1.ForeColor = Color.Green
                              PictureBox1.Image = My.Resources.nouveauxmessages
                          End If
                          Dim a As New Dialog5
                          If sender.Plateform = EManENTNewMailsInformer.Compte.Plateforme.Agora06 Then
                              a.Label3.Text &= " Agora06 "
                          ElseIf sender.Plateform = EManENTNewMailsInformer.Compte.Plateforme.Atrium Then
                              a.Label3.Text &= " Atrium "
                          ElseIf sender.Plateform = EManENTNewMailsInformer.Compte.Plateforme.Enteduc Then
                              a.Label3.Text &= " Enteduc "
                          End If
                          a.Label3.Text &= sender.UserName
                          a.Show()
                          a.BringToFront()
                      End Sub)
        ElseIf label1.Text = "Nouveaux messages." Then
            Label1.Text = "Recherche des nouveaux messages..."
            Label1.ForeColor = Color.Green
            PictureBox1.Image = My.Resources.validé
        End If
    End Sub

    Friend Function doesBrowserExist(ByVal compte As Compte) As Boolean
        Return utilisateursBrowser.Exists(Function(x) x.Utilisateur Is compte)
    End Function

    Friend Function getBrowser(ByVal compte As Compte) As Browser
        Return utilisateursBrowser.Find(Function(x) x.Utilisateur Is compte).Browser
    End Function
    Friend Function getUtilisateurBrowser(ByVal compte As Compte) As UtilisateurBrowser
        Return utilisateursBrowser.Find(Function(x) x.Utilisateur Is compte)
    End Function

    Friend Sub RechercheMails(data As Object)
        Dim browser As Browser = Nothing
        Dim compte As Compte = CType(data, Compte)
        Dim handled As Boolean = False
        Dim waitEvent As New ManualResetEvent(False)
        Try
            Dim news As UShort
            Dim platformeurl As String = urlsplatforms(compte.Plateform)
            Dim directoryContext As String = Path.Combine(Application.StartupPath, compte.UserName)
            If Not Directory.Exists(directoryContext) Then
                Directory.CreateDirectory(directoryContext)
            End If
            If doesBrowserExist(compte) Then
                getBrowser(compte).Dispose()
                utilisateursBrowser.Remove(getUtilisateurBrowser(compte))
            End If
            browser = BrowserFactory.Create(New BrowserContext(New BrowserContextParams(directoryContext)))
            utilisateursBrowser.Add(New UtilisateurBrowser(compte, browser))
            AddHandler browser.FinishLoadingFrameEvent, Sub(sender, e)
                                                            If (e.IsMainFrame) Then
                                                                waitEvent.Set()
                                                            End If
                                                        End Sub
            browser.LoadURL(platformeurl)
            waitEvent.WaitOne()
            waitEvent.Reset()
            If quitting Then
                Exit Sub
            End If
            Dim u As DOM.DOMInputElement = Nothing
            Dim p As DOM.DOMInputElement = Nothing
            Dim s As DOM.DOMElement = Nothing
            If platformeurl.Contains("agora06") Or platformeurl.Contains("enteduc") Then
                u = CType(browser.GetDocument().GetElementById("input_1"), DOM.DOMInputElement)
                p = CType(browser.GetDocument().GetElementById("input_2"), DOM.DOMInputElement)
                s = CType(browser.GetDocument().GetElementById("SubmitCreds"), DOM.DOMInputElement)
            ElseIf platformeurl.Contains("atrium") Then
                u = CType(browser.GetDocument().GetElementById("_atriumconnexion_WAR_atriumportlet_username"), DOM.DOMInputElement)
                p = CType(browser.GetDocument().GetElementById("_atriumconnexion_WAR_atriumportlet_password"), DOM.DOMInputElement)
                Dim blist As List(Of DOM.DOMNode) = browser.GetDocument().GetElementById("_atriumconnexion_WAR_atriumportlet_loginFormContainer").GetElementsByTagName("button")
                For Each butt As DOM.DOMElement In blist
                    If butt.GetAttribute("class") = "btn btn btn-primary btn-primary" And butt.GetAttribute("type") = "submit" Then s = butt
                Next
            End If
            If u Is Nothing Or p Is Nothing Or s Is Nothing Then
                retours.Add(New RetourTraitement(compte, Retour.CONNECTION_FAILED))
                handled = True
                Exit Sub
            End If
            u.SetAttribute("value", compte.UserName)
            p.Value = AES_Decrypt(compte.Password, "R)=@V74$9(HpY9u?b%D#~FJc6jy85{*3J8R6kd^a.Uk25!.JP[y-i=8" & compte.UserName & "nUb66=&S%Tv4YBnYa34_Jx3;a4L896itJg=wwW7?YUE4y")
            s.Click()
            Do
                Thread.Sleep(1000)
            Loop While browser.Loading And Not quitting
            If quitting Then
                Exit Sub
            End If
            If platformeurl.Contains("agora06") Or platformeurl.Contains("enteduc") Then
                If browser.GetDocument().GetElementById("credentials_table_postheader").GetElementsByTagName("font").Count > 0 Then
                    retours.Add(New RetourTraitement(compte, Retour.BAD_CREDENTIALS))
                    handled = True
                    Exit Sub
                End If
                Dim n As DOM.DOMElement = browser.GetDocument().GetElementById("Ident_MailsUtilisateur")
                If n Is Nothing Then
                    retours.Add(New RetourTraitement(compte, Retour.UNEXPECTED_ERROR))
                    handled = True
                    Exit Sub
                End If
                retours.Add(New RetourTraitement(compte, Retour.CONNECTED))
                If CUShort(n.TextContent) > news Then
                    compte.ReceiveMails(CUShort(n.TextContent))
                End If
                news = CUShort(n.TextContent)
            ElseIf platformeurl.Contains("atrium") Then
                If browser.URL.ToString.Contains("?connection-state=bad-credentials") Or browser.URL.ToString.Contains("/undefined") Then
                    retours.Add(New RetourTraitement(compte, Retour.BAD_CREDENTIALS))
                    handled = True
                    Exit Sub
                End If
                If browser.URL.ToString.Contains("?connection-state=closed-service") Then
                    retours.Add(New RetourTraitement(compte, Retour.SERVICE_UNAVAILABLE))
                    handled = True
                    Exit Sub
                End If
                If browser.GetDocument().GetElementById("_145_navAccountControls") Is Nothing Then
                    retours.Add(New RetourTraitement(compte, Retour.UNEXPECTED_ERROR))
                    handled = True
                    Exit Sub
                End If
                retours.Add(New RetourTraitement(compte, Retour.CONNECTED))
                Dim m As DOM.DOMNode = Nothing
                Dim slist As List(Of DOM.DOMNode) = browser.GetDocument().GetElementById("_145_navAccountControls").GetElementsByClassName("badge badge-notify")
                If Not slist.Count = 0 Then
                    m = slist(0)
                    If Not m.TextContent = "" Then
                        If CUShort(m.TextContent) > news Then
                            compte.ReceiveMails(CUShort(m.TextContent))
                        End If
                        news = CUShort(m.TextContent)
                    End If
                End If
            End If
                While Not quitting
                Thread.Sleep(30000)
                Dim nm As UShort
                Try
                    nm = CheckMails(compte, browser, waitEvent)
                Catch ex As HandledException
                    handled = True
                    Exit Sub
                Catch ex As Exception
                    Exit Sub
                End Try
                If nm > news Or nm = 0US Then
                    compte.ReceiveMails(nm)
                End If
                news = nm
            End While
        Catch ex As ThreadInterruptedException
            retours.Add(New RetourTraitement(compte, Retour.INTERRUPTED))
            handled = True
        Catch ex As Exception
            retours.Add(New RetourTraitement(compte, Retour.UNEXPECTED_ERROR))
            handled = True
        Finally
            UtilisateursRecherche.Remove(compte)
            utilisateursThreads.Remove(Thread.CurrentThread)
            If Not handled Then
                retours.Add(New RetourTraitement(compte, Retour.END_OF_THREAD))
            End If
            If Not browser Is Nothing Then
                browser.Dispose()
                utilisateursBrowser.Remove(New UtilisateurBrowser(compte, browser))
            End If
        End Try
    End Sub

    Private Function CheckMails(utilisateur As Compte, browser As Browser, waitEvent As ManualResetEvent) As UShort
        browser.LoadURL(browser.URL)
        waitEvent.WaitOne()
        waitEvent.Reset()
        Do
            Thread.Sleep(100)
        Loop While browser.Loading And Not quitting
        If quitting Then
            Throw New ThreadInterruptedException
        End If
        Dim platformeurl As String = urlsplatforms(utilisateur.Plateform)
        If platformeurl.Contains("agora06") Or platformeurl.Contains("enteduc") Then
            If browser.GetDocument().GetElementById("credentials_table_postheader").GetElementsByTagName("font").Count > 0 Then
                retours.Add(New RetourTraitement(utilisateur, Retour.BAD_CREDENTIALS))
                Throw New HandledException
            End If
            Dim n As DOM.DOMElement = browser.GetDocument().GetElementById("Ident_MailsUtilisateur")
            If n Is Nothing Then
                retours.Add(New RetourTraitement(utilisateur, Retour.UNEXPECTED_ERROR))
                Throw New HandledException
            End If
            Return CUShort(n.TextContent)
        ElseIf platformeurl.Contains("atrium") Then
            If browser.URL.ToString.Contains("?connection-state=bad-credentials") Or browser.URL.ToString.Contains("/undefined") Then
                MsgBox(browser.URL)
                retours.Add(New RetourTraitement(utilisateur, Retour.BAD_CREDENTIALS))
                Throw New HandledException
            End If
            If browser.URL.ToString.Contains("?connection-state=closed-service") Then
                retours.Add(New RetourTraitement(utilisateur, Retour.SERVICE_UNAVAILABLE))
                Throw New HandledException
            End If
            If browser.GetDocument().GetElementById("_145_navAccountControls") Is Nothing Then
                retours.Add(New RetourTraitement(utilisateur, Retour.UNEXPECTED_ERROR))
                Throw New HandledException
            End If
            Dim slist As List(Of DOM.DOMNode) = browser.GetDocument().GetElementById("_145_navAccountControls").GetElementsByClassName("badge badge-notify")
            If Not slist.Count = 0 Then
                Dim m As DOM.DOMNode = slist(0)
                If Not m.TextContent = "" Then
                    Return CUShort(m.TextContent)
                Else
                    Return 0US
                End If
            Else
                Return 0US
            End If
        End If
        Return 0US
    End Function

    Private Sub GererRetours(e As Object)
        While Not quitting
            While Not retours.Count = 0
                Dim retou = retours(0)
                retours.RemoveAt(0)
                If retou.Reponse = Retour.UNEXPECTED_ERROR Then
                    UtilisateursRecherche.Remove(retou.Utilisateur)
                    RemoveHandler retou.Utilisateur.NewMails, AddressOf NewMails
                    Me.Invoke(Sub()
                                  DataGridView1.Rows(utilisateurs.IndexOf(retou.Utilisateur)).Cells(0).Value = My.Resources.erreur_icone_4913_128
                                  DataGridView1.Rows(utilisateurs.IndexOf(retou.Utilisateur)).Cells(0).ToolTipText = "Erreur innatendue"
                                  If utilisateursThreads.Count > 0 Then
                                      Label1.Text = "Erreur innatendue pour certains utlisateurs."
                                      Label1.ForeColor = Color.OrangeRed
                                      PictureBox1.Image = My.Resources.avertissement_icone_9768_128
                                  Else
                                      Label1.Text = "Erreur innatendue."
                                      Label1.ForeColor = Color.DarkRed
                                      PictureBox1.Image = My.Resources.erreur_icone_4913_128
                                  End If
                                  Dim a As New Dialog3
                                  a.Label3.Text &= retou.Utilisateur.UserName
                                  a.ShowDialog()
                                  a.BringToFront()
                              End Sub)

                ElseIf retou.Reponse = Retour.CONNECTION_FAILED Then
                    UtilisateursRecherche.Remove(retou.Utilisateur)
                    RemoveHandler retou.Utilisateur.NewMails, AddressOf NewMails
                    Me.Invoke(Sub()
                                  DataGridView1.Rows(utilisateurs.IndexOf(retou.Utilisateur)).Cells(0).Value = My.Resources.erreur_icone_4913_128
                                  DataGridView1.Rows(utilisateurs.IndexOf(retou.Utilisateur)).Cells(0).ToolTipText = "Echec de la connexion"
                                  If utilisateursThreads.Count > 0 Then
                                      Label1.Text = "Echec de la connexion pour certains utlisateurs."
                                      Label1.ForeColor = Color.OrangeRed
                                      PictureBox1.Image = My.Resources.avertissement_icone_9768_128
                                  Else
                                      Label1.Text = "Echec de la connexion."
                                      Label1.ForeColor = Color.DarkRed
                                      PictureBox1.Image = My.Resources.erreur_icone_4913_128
                                  End If
                                  Dim a As New Dialog1
                                  a.Label3.Text &= retou.Utilisateur.UserName
                                  a.ShowDialog()
                                  a.BringToFront()
                              End Sub)

                ElseIf retou.Reponse = Retour.BAD_CREDENTIALS Then
                    UtilisateursRecherche.Remove(retou.Utilisateur)
                    RemoveHandler retou.Utilisateur.NewMails, AddressOf NewMails
                    Me.Invoke(Sub()
                                  DataGridView1.Rows(utilisateurs.IndexOf(retou.Utilisateur)).Cells(0).Value = My.Resources.erreur_icone_4913_128
                                  DataGridView1.Rows(utilisateurs.IndexOf(retou.Utilisateur)).Cells(0).ToolTipText = "Identifiants incorrects"
                                  If utilisateursThreads.Count > 0 Then
                                      Label1.Text = "Identifiants incorrects pour certains utlisateurs."
                                      Label1.ForeColor = Color.OrangeRed
                                      PictureBox1.Image = My.Resources.avertissement_icone_9768_128
                                  Else
                                      Label1.Text = "Identifiants incorrects."
                                      Label1.ForeColor = Color.DarkRed
                                      PictureBox1.Image = My.Resources.erreur_icone_4913_128
                                  End If
                                  Dim a As New Dialog2
                                  a.Label3.Text &= retou.Utilisateur.UserName
                                  a.ShowDialog()
                                  a.BringToFront()
                              End Sub)
                ElseIf retou.Reponse = Retour.SERVICE_UNAVAILABLE Then
                    UtilisateursRecherche.Remove(retou.Utilisateur)
                    RemoveHandler retou.Utilisateur.NewMails, AddressOf NewMails
                    Me.Invoke(Sub()
                                  DataGridView1.Rows(utilisateurs.IndexOf(retou.Utilisateur)).Cells(0).Value = My.Resources.erreur_icone_4913_128
                                  DataGridView1.Rows(utilisateurs.IndexOf(retou.Utilisateur)).Cells(0).ToolTipText = "Identifiants incorrects"
                                  If utilisateursThreads.Count > 0 Then
                                      Label1.Text = "ENT indisponible pour certains utilisateurs."
                                      Label1.ForeColor = Color.OrangeRed
                                      PictureBox1.Image = My.Resources.avertissement_icone_9768_128
                                  Else
                                      Label1.Text = "ENT indisponible."
                                      Label1.ForeColor = Color.DarkRed
                                      PictureBox1.Image = My.Resources.erreur_icone_4913_128
                                  End If
                                  Dim a As New Dialog6
                                  a.Label3.Text &= retou.Utilisateur.UserName
                                  a.ShowDialog()
                                  a.BringToFront()
                              End Sub)
                ElseIf retou.Reponse = Retour.END_OF_THREAD Then
                    UtilisateursRecherche.Remove(retou.Utilisateur)
                    RemoveHandler retou.Utilisateur.NewMails, AddressOf NewMails
                    Me.Invoke(Sub()
                                  DataGridView1.Rows(utilisateurs.IndexOf(retou.Utilisateur)).Cells(0).Value = My.Resources.erreur_icone_4913_128
                                  DataGridView1.Rows(utilisateurs.IndexOf(retou.Utilisateur)).Cells(0).ToolTipText = "Recherche interrompue"
                                      If utilisateursThreads.Count > 0 Then
                                          Label1.Text = "Recherche interrompue pour certains utilisateurs."
                                          Label1.ForeColor = Color.OrangeRed
                                          PictureBox1.Image = My.Resources.avertissement_icone_9768_128
                                      Else
                                          Label1.Text = "Recherche interrompue."
                                          Label1.ForeColor = Color.DarkRed
                                          PictureBox1.Image = My.Resources.erreur_icone_4913_128
                                      End If
                                      Dim a As New Dialog7
                                      a.Label3.Text &= retou.Utilisateur.UserName
                                      a.ShowDialog()
                                  a.BringToFront()
                              End Sub)
                ElseIf retou.Reponse = Retour.CONNECTED Then
                    UtilisateursRecherche.Add(retou.Utilisateur)
                    AddHandler retou.Utilisateur.NewMails, AddressOf NewMails
                    Me.Invoke(Sub()
                                  DataGridView1.Rows(utilisateurs.IndexOf(retou.Utilisateur)).Cells(0).Value = My.Resources.validé
                                  DataGridView1.Rows(utilisateurs.IndexOf(retou.Utilisateur)).Cells(0).ToolTipText = "Recherche des nouveaux messages..."
                                  If UtilisateursRecherche.Count = utilisateurs.Count Then
                                      Label1.Text = "Recherche des nouveaux messages..."
                                      Label1.ForeColor = Color.Green
                                      PictureBox1.Image = My.Resources.validé
                                  End If
                              End Sub)
                End If
            End While
        End While
    End Sub

    Private Sub Panel2_Click(sender As Object, e As EventArgs) Handles Panel2.Click
        On Error Resume Next
        Dim a As New LoginForm1
        If a.ShowDialog() = DialogResult.OK Then
            Dim t As Thread = findUserThread(a.editedUser.UserName)
            t.Interrupt()
            t.Join()
            utilisateursThreads.Remove(t)
            Dim th As New Thread(AddressOf RechercheMails)
            th.Name = a.editedUser.UserName
            th.Start(a.editedUser)
            utilisateursThreads.Add(th)
        End If

    End Sub

    Private Function findUserThread(ByVal username As String) As Thread
        Return utilisateursThreads.Find(Function(x) x.Name = username)
    End Function

    Private Sub Panel4_Click(sender As Object, e As EventArgs) Handles Panel4.Click
        Try
            If utilisateurs.Count = 0 Then
                Label1.Text = "Veuillez configurer un utilisateur."
                Label1.ForeColor = Color.DarkOrange
                PictureBox1.Image = My.Resources.male_user_warning_256
                Exit Sub
            End If
            If UtilisateursRecherche.Count = utilisateurs.Count Then
                Exit Sub
            End If
            Label1.Text = "Initialisation de certains comptes en cours..."
            Label1.ForeColor = Color.Blue
            PictureBox1.Image = My.Resources.attente
            For Each item In utilisateurs
                If Not utilisateursThreads.Exists(Function(x) x.Name = item.UserName And x.IsAlive) Then
                    DataGridView1.Rows(utilisateurs.IndexOf(item)).Cells(0).Value = My.Resources.attente
                    Dim th As New Thread(AddressOf RechercheMails)
                    th.Name = item.UserName
                    th.Start(item)
                    utilisateursThreads.Add(th)
                End If
            Next
        Catch ex As Exception
            MsgBox("Une erreur est survenue pendant l'initialisation. Veuillez réessayer.", MsgBoxStyle.Critical, "Erreur")
        End Try
    End Sub

    Private Sub DataGridView1_SelectionChanged(sender As Object, e As EventArgs) Handles DataGridView1.SelectionChanged
        On Error Resume Next
        If DataGridView1.SelectedRows.Count = 0 Then
            If DataGridView1.SelectedCells.Count = 1 Then
                Dim i As Integer = DataGridView1.SelectedCells(0).RowIndex
                DataGridView1.ClearSelection()
                DataGridView1.Rows(i).Selected = True
            End If
        End If
    End Sub

    Private Sub Panel1_Click(sender As Object, e As EventArgs) Handles Panel1.Click
        On Error Resume Next
        If Not DataGridView1.SelectedRows.Count = 0 Then
            Dim a As New LoginForm2
            a.user = utilisateurs(DataGridView1.SelectedRows(0).Cells(0).RowIndex)
            a.UsernameTextBox.Text = a.user.UserName
            a.ComboBox1.SelectedIndex = CInt(a.user.Plateform)
            If a.ShowDialog() = DialogResult.OK Then
                Dim t As Thread = findUserThread(a.editedUser.UserName)
                t.Interrupt()
                t.Join()
                utilisateursThreads.Remove(t)
                Dim th As New Thread(AddressOf RechercheMails)
                th.Name = a.editedUser.UserName
                th.Start(a.editedUser)
                utilisateursThreads.Add(th)
            End If
        End If
    End Sub

    Private Sub Panel3_Click(sender As Object, e As EventArgs) Handles Panel3.Click
        On Error Resume Next
        If Not DataGridView1.SelectedRows.Count = 0 Then
            utilisateurs.RemoveAt(DataGridView1.SelectedRows(0).Cells(0).RowIndex)
            RefreshUsers()
        End If
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        Try
            If CheckBox1.Checked Then
                My.Computer.Registry.CurrentUser.OpenSubKey("SOFTWARE", True).OpenSubKey("Microsoft", True).OpenSubKey("Windows", True).
                     OpenSubKey("CurrentVersion", True).OpenSubKey("Run", True).SetValue("EManENTNewMailsInformer", """" & Application.ExecutablePath & """ /q", RegistryValueKind.String)
            Else
                My.Computer.Registry.CurrentUser.OpenSubKey("SOFTWARE", True).OpenSubKey("Microsoft", True).OpenSubKey("Windows", True).
                     OpenSubKey("CurrentVersion", True).OpenSubKey("Run", True).DeleteValue("EManENTNewMailsInformer")
            End If
        Catch ex As UnauthorizedAccessException
            MsgBox("Impossible de définir le démarrage automatique du programme. Veuillez démarrer EManENTNewMailsInformer en tant qu'administrateur puis réessayer.", MsgBoxStyle.Critical, "Erreur")
        End Try
        CheckStartup()
    End Sub

    Private Sub CheckStartup()
        On Error Resume Next
        RemoveHandler CheckBox1.CheckedChanged, AddressOf CheckBox1_CheckedChanged
        CheckBox1.Checked = Not My.Computer.Registry.CurrentUser.OpenSubKey("SOFTWARE").OpenSubKey("Microsoft").OpenSubKey("Windows").
             OpenSubKey("CurrentVersion").OpenSubKey("Run").GetValue("EManENTNewMailsInformer", Nothing) Is Nothing
        AddHandler CheckBox1.CheckedChanged, AddressOf CheckBox1_CheckedChanged
    End Sub

    Private Sub Label5_Click(sender As Object, e As EventArgs) Handles Label5.Click, QuitterToolStripMenuItem.Click
        Quit()
    End Sub

    Private Sub Quit()
        quitting = True
        Label1.Text = "Arrêt en cours..."
        Label1.ForeColor = Color.Black
        PictureBox1.Image = My.Resources.attente
        UtilisateursRecherche.Clear()
        DataGridView1.Rows.Clear()
        retoursThread.Interrupt()
        retoursThread.Join()
        For Each th In utilisateursThreads
            th.Interrupt()
            th.Join()
        Next
        Application.Exit()
        End
    End Sub

    Private Sub Label6_Click(sender As Object, e As EventArgs) Handles Label6.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseDoubleClick
        Me.Show()
        Me.BringToFront()
        NotifyIcon1.Visible = False
    End Sub

    Private Sub Label7_Click(sender As Object, e As EventArgs) Handles Label7.Click
        Me.Hide()
        NotifyIcon1.Visible = True
        NotifyIcon1.ShowBalloonTip(1000, "Programme en arrière plan", "Double-cliquez sur cette icône pour faire apparaître la fenêtre à nouveau.", ToolTipIcon.Info)
    End Sub

    Private Sub Label5_MouseEnter(sender As Object, e As EventArgs) Handles Label5.MouseEnter
        Label5.BackColor = Color.MistyRose
    End Sub

    Private Sub Label5_MouseLeave(sender As Object, e As EventArgs) Handles Label5.MouseLeave
        Label5.BackColor = Color.Silver
    End Sub

    Private Sub Label7_MouseEnter(sender As Object, e As EventArgs) Handles Label7.MouseEnter, Label6.MouseEnter
        DirectCast(sender, Label).BackColor = Color.LightGray
    End Sub

    Private Sub Label6_MouseLeave(sender As Object, e As EventArgs) Handles Label7.MouseLeave, Label6.MouseLeave
        DirectCast(sender, Label).BackColor = Color.Silver
    End Sub

    Private Sub Panel5_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Panel5.MouseDown, PictureBox2.MouseDown, Label4.MouseDown
        X = Control.MousePosition.X - Me.Location.X
        Y = Control.MousePosition.Y - Me.Location.Y
    End Sub

    Private Sub Panel5_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Panel5.MouseMove, PictureBox2.MouseMove, Label4.MouseMove
        If e.Button = MouseButtons.Left Then
            NewPoint = Control.MousePosition
            NewPoint.Y -= (Y)
            NewPoint.X -= (X)
            Me.Location = NewPoint
        End If
    End Sub

    Private Sub Timer3_Tick(sender As Object, e As EventArgs) Handles Timer3.Tick
        Timer3.Stop()
        If My.Application.QuietStart Then
            Me.Hide()
            NotifyIcon1.Visible = True
            Opacity = 1
            ShowInTaskbar = True
        End If
    End Sub

    Private Sub AfficherLaFenêtreToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AfficherLaFenêtreToolStripMenuItem.Click
        Me.Show()
        Me.BringToFront()
        NotifyIcon1.Visible = False
    End Sub

    Private Sub Form1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Quit()
    End Sub
End Class
