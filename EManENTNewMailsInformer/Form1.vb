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
    Friend directoriesToDelete As New List(Of String)
    Private quitting As Boolean
    Friend password As String = Nothing

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
        If Not password = Nothing Or Not password = String.Empty Then
            cont = AES_Encrypt("Encrypted-personal-password", "ppvBR<q3kbb3%~tZ6urN:,4?&5T-9Dffa9#@UEP:D4x5[5NQ9Af)Vr9!2-7a8DXK5Gu4ULS!S.3996/u?f65%&aLcEE?9#.bP>$}M97r46Aw64q52-;)Z22v:s8fHV&n5H{Y6eH3Dnyg9ib8,4!N6C%jc~E59zMQ36Th#VYuRR5Qwt%LN]7N5jefZb9e!L/@sAJ)[=5(@FgVy*^4$p3Az4(zW694z#TM^7$X7u3P63Bm~z)j]@b3q6vB)(yU[j:+")
        End If
        Dim users As String = "EManENTNewMailsInformer-configuration file"
        For Each user In utilisateurs
            users &= user.ToString()
        Next
        If Not password = Nothing Or Not password = String.Empty Then
            cont += AES_Encrypt(users, "c#k&>*6<r5jGa889]kw4{79_>2gbG5tK]cAM246eVs?7;3Ft]5Di_53" + password + "3dM(3zbYnZ}*jb4Ua>96zH2bHr35j#H?Px$*LF_G9GhDt)TF[*,YMJ!&A;p987BfzE/qM2h779x^7MVpF:sA<J~9ySa;82g3/}WYe3ZM_]e26PP/*f7Wj}-D8x22>qr3n*K<[]42S^875u9K[74r}XG*8KG83F8zVbEeD45?!C9bS*UicnQRX9n!G@-v]]*aGZi@cp(u5")
        Else
            cont = AES_Encrypt(users, "c#k&>*6<r5jGa889]kw4{79_>2gbG5tK]cAM246eVs?7;3Ft]5Di_533dM(3zbYnZ}*jb4Ua>96zH2bHr35j#H?Px$*LF_G9GhDt)TF[*,YMJ!&A;p987BfzE/qM2h779x^7MVpF:sA<J~9ySa;82g3/}WYe3ZM_]e26PP/*f7Wj}-D8x22>qr3n*K<[]42S^875u9K[74r}XG*8KG83F8zVbEeD45?!C9bS*UicnQRX9n!G@-v]]*aGZi@cp(u5")
        End If
        Try
            IO.File.WriteAllText(Application.StartupPath & "\comptes.ini", cont)
        Catch ex As Exception
        End Try
    End Sub

    Friend Sub LoadUsers(Optional onStartup As Boolean = False, Optional askPassword As Boolean = True)
        On Error GoTo er
        If Not IO.File.Exists(Application.StartupPath & "\comptes.ini") Then Exit Sub
        Dim cont As String = IO.File.ReadAllText(Application.StartupPath & "\comptes.ini")
        If (onStartup Or askPassword) And cont.StartsWith(AES_Encrypt("Encrypted-personal-password", "ppvBR<q3kbb3%~tZ6urN:,4?&5T-9Dffa9#@UEP:D4x5[5NQ9Af)Vr9!2-7a8DXK5Gu4ULS!S.3996/u?f65%&aLcEE?9#.bP>$}M97r46Aw64q52-;)Z22v:s8fHV&n5H{Y6eH3Dnyg9ib8,4!N6C%jc~E59zMQ36Th#VYuRR5Qwt%LN]7N5jefZb9e!L/@sAJ)[=5(@FgVy*^4$p3Az4(zW694z#TM^7$X7u3P63Bm~z)j]@b3q6vB)(yU[j:+")) Then
            If Not LoginForm4.Visible Then
                If Not LoginForm4.ShowDialog() = DialogResult.OK Then
                    Quit()
                End If
            End If
        End If
        If Not password = Nothing Or Not password = String.Empty Then
            cont = Mid(cont, AES_Encrypt("Encrypted-personal-password", "ppvBR<q3kbb3%~tZ6urN:,4?&5T-9Dffa9#@UEP:D4x5[5NQ9Af)Vr9!2-7a8DXK5Gu4ULS!S.3996/u?f65%&aLcEE?9#.bP>$}M97r46Aw64q52-;)Z22v:s8fHV&n5H{Y6eH3Dnyg9ib8,4!N6C%jc~E59zMQ36Th#VYuRR5Qwt%LN]7N5jefZb9e!L/@sAJ)[=5(@FgVy*^4$p3Az4(zW694z#TM^7$X7u3P63Bm~z)j]@b3q6vB)(yU[j:+").Length + 1)
            cont = AES_Decrypt(cont, "c#k&>*6<r5jGa889]kw4{79_>2gbG5tK]cAM246eVs?7;3Ft]5Di_53" & password & "3dM(3zbYnZ}*jb4Ua>96zH2bHr35j#H?Px$*LF_G9GhDt)TF[*,YMJ!&A;p987BfzE/qM2h779x^7MVpF:sA<J~9ySa;82g3/}WYe3ZM_]e26PP/*f7Wj}-D8x22>qr3n*K<[]42S^875u9K[74r}XG*8KG83F8zVbEeD45?!C9bS*UicnQRX9n!G@-v]]*aGZi@cp(u5")
            If cont = Nothing OrElse Not cont.StartsWith("EManENTNewMailsInformer-configuration file") Then
                MsgBox("Le mot de passe est incorrect. Veuillez réessayer.", MsgBoxStyle.Critical, "Mot Decimal passe incorrect.")
                LoadUsers(askPassword:=True)
                Exit Sub
            End If
        Else
            cont = AES_Decrypt(cont, "c#k&>*6<r5jGa889]kw4{79_>2gbG5tK]cAM246eVs?7;3Ft]5Di_533dM(3zbYnZ}*jb4Ua>96zH2bHr35j#H?Px$*LF_G9GhDt)TF[*,YMJ!&A;p987BfzE/qM2h779x^7MVpF:sA<J~9ySa;82g3/}WYe3ZM_]e26PP/*f7Wj}-D8x22>qr3n*K<[]42S^875u9K[74r}XG*8KG83F8zVbEeD45?!C9bS*UicnQRX9n!G@-v]]*aGZi@cp(u5")
        End If
        cont = Mid(cont, "EManENTNewMailsInformer-configuration file".Length + 1)
        Dim com As String() = cont.Split("}"c)
        For Each Co In com
            If Co.StartsWith("Compte{") Then
                utilisateurs.Add(EManENTNewMailsInformer.Compte.Join(Co & "}"))
            End If
        Next
        RefreshUsers()
        If onStartup Then
            CheckStartup()
            retoursThread.Start()
        End If
        Exit Sub
er:
    End Sub

    Private Sub Panel2_MouseEnter(sender As Object, e As EventArgs) Handles Panel1.MouseEnter, Panel2.MouseEnter, Panel3.MouseEnter, Panel4.MouseEnter, Panel6.MouseEnter
        On Error Resume Next
        If Not DirectCast(sender, Panel).Enabled Then Exit Sub
        DirectCast(sender, Panel).BackColor = Color.FromArgb(192, 210, 192)
    End Sub

    Private Sub Panel2_MouseLeave(sender As Object, e As EventArgs) Handles Panel1.MouseLeave, Panel2.MouseLeave, Panel3.MouseLeave, Panel4.MouseLeave, Panel6.MouseLeave
        On Error Resume Next
        If Not DirectCast(sender, Panel).Enabled Then Exit Sub
        DirectCast(sender, Panel).BackColor = DefaultBackColor
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        On Error Resume Next
        LoadUsers(True)
    End Sub

    Private Sub ShowDisclaimer()
        Try
            If Not File.Exists(Application.StartupPath + "\disclaimer.ini") OrElse
          Not File.ReadAllText(Application.StartupPath + "\disclaimer.ini") = "show:false" Then
                If Not DNBDisclaimer.Visible Then DNBDisclaimer.ShowDialog()
            End If
        Catch ex As Exception
            If Not DNBDisclaimer.Visible Then DNBDisclaimer.ShowDialog()
        End Try
    End Sub

    Friend Sub RefreshUsers(Optional forceNewThreads As Boolean = True)
        On Error Resume Next
        DataGridView1.Rows.Clear()
        SaveUsers()
        If utilisateurs.Count = 0 Then
            Label1.Text = "Veuillez configurer un utilisateur."
            Label1.ForeColor = Color.DarkOrange
            PictureBox1.Image = My.Resources.male_user_warning_256
            Exit Sub
        End If
        If forceNewThreads Then
            Label1.Text = "Initialisation en cours..."
            Label1.ForeColor = Color.Blue
            PictureBox1.Image = My.Resources.attente
            For Each th In utilisateursThreads
                th.Interrupt()
                th.Join()
            Next
            utilisateursThreads.Clear()
        End If
        For Each user In utilisateurs
            If (Not doesUserThreadExist(user.UserName, user.Plateform) And user.Etat = Nothing) Or forceNewThreads Then
                DataGridView1.Rows.Add(New Object() {My.Resources.attente, user.UserName, user.Plateform})
                Dim th As New Thread(AddressOf RechercheMails)
                th.Name = user.UserName + CStr(user.Plateform)
                th.Start(user)
                user.Etat = Retour.INITIALIZATION
                utilisateursThreads.Add(th)
                Continue For
            End If
            Select Case user.Etat
                Case Retour.INITIALIZATION
                    DataGridView1.Rows.Add(New Object() {My.Resources.attente, user.UserName, user.Plateform})
                    Exit Select
                Case Retour.BAD_CREDENTIALS
                    DataGridView1.Rows.Add(New Object() {My.Resources.erreur_icone_4913_128, user.UserName, user.Plateform})
                    DataGridView1.Rows(DataGridView1.RowCount - 1).Cells(0).ToolTipText = "Identifiants incorrects"
                    Exit Select
                Case Retour.CONNECTED
                    DataGridView1.Rows.Add(New Object() {My.Resources.validé, user.UserName, user.Plateform})
                    DataGridView1.Rows(DataGridView1.RowCount - 1).Cells(0).ToolTipText = "Recherche des nouveaux messages..."
                    Exit Select
                Case Retour.NEW_MAILS
                    DataGridView1.Rows.Add(New Object() {My.Resources.newemail, user.UserName, user.Plateform})
                    DataGridView1.Rows(DataGridView1.RowCount - 1).Cells(0).ToolTipText = "Nouveaux messages !"
                    Exit Select
                Case Retour.CONNECTION_FAILED
                    DataGridView1.Rows.Add(New Object() {My.Resources.erreur_icone_4913_128, user.UserName, user.Plateform})
                    DataGridView1.Rows(DataGridView1.RowCount - 1).Cells(0).ToolTipText = "Echec de la connnexion"
                    Exit Select
                Case Retour.END_OF_THREAD, Retour.INTERRUPTED
                    DataGridView1.Rows.Add(New Object() {My.Resources.erreur_icone_4913_128, user.UserName, user.Plateform})
                    DataGridView1.Rows(DataGridView1.RowCount - 1).Cells(0).ToolTipText = "Recherche interrompue"
                    Exit Select
                Case Retour.SERVICE_UNAVAILABLE
                    DataGridView1.Rows.Add(New Object() {My.Resources.erreur_icone_4913_128, user.UserName, user.Plateform})
                    DataGridView1.Rows(DataGridView1.RowCount - 1).Cells(0).ToolTipText = "ENT indisponible"
                    Exit Select
                Case Retour.UNEXPECTED_ERROR
                    DataGridView1.Rows.Add(New Object() {My.Resources.erreur_icone_4913_128, user.UserName, user.Plateform})
                    DataGridView1.Rows(DataGridView1.RowCount - 1).Cells(0).ToolTipText = "Erreur inattendue"
                    Exit Select
            End Select
        Next
    End Sub

    Private Sub NewMails(ByVal sender As Compte, ByVal count As UShort)
        If count > 0 Then
            Me.Invoke(Sub()
                          sender.Etat = Retour.NEW_MAILS
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
        Dim directoryContext As String = Path.Combine(Application.StartupPath, compte.UserName + CStr(compte.Plateform) + "-" + New Random().Next(Integer.MaxValue).ToString)
        Try
            Dim news As UShort
            Dim platformeurl As String = urlsplatforms(compte.Plateform)
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
            Loop While (browser.Loading Or browser.URL = "https://www.atrium-paca.fr/") And Not quitting
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
                If browser.GetDocument().GetElementById("_145_navAccountControls") Is Nothing Or
                     browser.GetDocument().GetElementsByClassName("icon-envelope icon-atrium-mega").Count = 0 Then
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
                Try
                    IO.Directory.Delete(directoryContext, True)
                Catch ex As Exception
                    directoriesToDelete.Add(directoryContext)
                End Try
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
                retou.Utilisateur.Etat = retou.Reponse
                If retou.Reponse = Retour.UNEXPECTED_ERROR Then
                    UtilisateursRecherche.Remove(retou.Utilisateur)
                    RemoveHandler retou.Utilisateur.NewMails, AddressOf NewMails
                    Me.Invoke(Sub()
                                  DataGridView1.Rows(utilisateurs.IndexOf(retou.Utilisateur)).Cells(0).Value = My.Resources.erreur_icone_4913_128
                                  DataGridView1.Rows(utilisateurs.IndexOf(retou.Utilisateur)).Cells(0).ToolTipText = "Erreur inattendue"
                                  If utilisateursThreads.Count > 0 Then
                                      Label1.Text = "Erreur inattendue pour certains utlisateurs."
                                      Label1.ForeColor = Color.OrangeRed
                                      PictureBox1.Image = My.Resources.avertissement_icone_9768_128
                                  Else
                                      Label1.Text = "Erreur inattendue."
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
                                  DataGridView1.Rows(utilisateurs.IndexOf(retou.Utilisateur)).Cells(0).ToolTipText = "ENT indisponible"
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
            Dim t As Thread = findUserThread(a.editedUser.UserName, a.editedUser.Plateform)
            t.Interrupt()
            t.Join()
            utilisateursThreads.Remove(t)
            Dim th As New Thread(AddressOf RechercheMails)
            th.Name = a.editedUser.UserName + CStr(a.editedUser.Plateform)
            th.Start(a.editedUser)
            a.editedUser.Etat = Retour.INITIALIZATION
            utilisateursThreads.Add(th)
        End If

    End Sub
    Private Function doesUserThreadExist(ByVal username As String, ByVal platform As Compte.Plateforme) As Boolean
        Return utilisateursThreads.Exists(Function(x) x.Name = username + CStr(platform))
    End Function

    Private Function findUserThread(ByVal username As String, ByVal platform As Compte.Plateforme) As Thread
        Return utilisateursThreads.Find(Function(x) x.Name = username + CStr(platform))
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
            For Each item In utilisateurs
                If Not doesUserThreadExist(item.UserName, item.Plateform) Then
                    Label1.Text = "Initialisation de certains comptes en cours..."
                    Label1.ForeColor = Color.Blue
                    PictureBox1.Image = My.Resources.attente
                    DataGridView1.Rows(utilisateurs.IndexOf(item)).Cells(0).Value = My.Resources.attente
                    DataGridView1.Rows(utilisateurs.IndexOf(item)).Cells(0).ToolTipText = Nothing
                    Dim th As New Thread(AddressOf RechercheMails)
                    th.Name = item.UserName + CStr(item.Plateform)
                    th.Start(item)
                    item.Etat = Retour.INITIALIZATION
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
                If utilisateurs.Count = 1 Then
                    Label1.Text = "Initialisation en cours..."
                Else
                    Label1.Text = "Initialisation de certains comptes en cours..."
                End If
                Label1.ForeColor = Color.Blue
                PictureBox1.Image = My.Resources.attente
                a.editedUser.Etat = Retour.INITIALIZATION
                RefreshUsers(False)
                Dim t As Thread = findUserThread(a.editedUser.UserName, a.editedUser.Plateform)
                t.Interrupt()
                t.Join()
                utilisateursThreads.Remove(t)
                Dim th As New Thread(AddressOf RechercheMails)
                th.Name = a.editedUser.UserName + CStr(a.editedUser.Plateform)
                th.Start(a.editedUser)
                utilisateursThreads.Add(th)
            End If
        End If
    End Sub

    Private Sub Panel3_Click(sender As Object, e As EventArgs) Handles Panel3.Click
        On Error Resume Next
        If Not DataGridView1.SelectedRows.Count = 0 Then
            Dim t As Thread = findUserThread(CStr(DataGridView1.SelectedRows(0).Cells(1).Value), CType(DataGridView1.SelectedRows(0).Cells(2).Value, Compte.Plateforme))
            t.Interrupt()
            t.Join()
            utilisateursThreads.Remove(t)
            removeUser(CStr(DataGridView1.SelectedRows(0).Cells(1).Value), CType(DataGridView1.SelectedRows(0).Cells(2).Value, Compte.Plateforme))
            RefreshUsers(False)
        End If
    End Sub

    Private Sub removeUser(ByVal userName As String, ByVal platform As Compte.Plateforme)
        utilisateurs.RemoveAll(Function(x) x.UserName = userName And x.Plateform = platform)
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
        On Error GoTo er
        quitting = True
        If Not password = Nothing Then password = Nothing
        Label1.Text = "Arrêt en cours..."
        Label1.ForeColor = Color.Black
        PictureBox1.Image = My.Resources.attente
        UtilisateursRecherche.Clear()
        DataGridView1.Rows.Clear()
        If retoursThread.IsAlive Then
            retoursThread.Interrupt()
            retoursThread.Join()
        End If
        While utilisateursThreads.Count > 0
            If Not utilisateursThreads(0).IsAlive Then
                utilisateursThreads.RemoveAt(0)
                Continue While
            End If
            utilisateursThreads(0).Interrupt()
            utilisateursThreads(0).Join()
        End While
        For Each directory In directoriesToDelete
            If IO.Directory.Exists(directory) Then
                On Error Resume Next
                IO.Directory.Delete(directory, True)
                On Error GoTo er
            End If
        Next
        directoriesToDelete.Clear()
        Application.Exit()
er:
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

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        ShowDisclaimer()
    End Sub

    Private Sub Panel6_Click(sender As Object, e As EventArgs) Handles Panel6.Click
        If Not LoginForm3.Visible Then LoginForm3.ShowDialog()
    End Sub

    Private Sub Form1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Quit()
    End Sub
End Class
