Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Public Class Form1
    Dim pressExec As Boolean = False
    Dim flagSend As Boolean = False
    Dim WithEvents WinSockCliente As New Cliente
    Dim WithEvents tmrSendData As New Timer
    Private Sub btnConectar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnConectar.Click
        txtIP.Enabled = False
        txtPuerto.Enabled = False
        txtDummyContent.Focus()
        connetStart()
    End Sub
    Private Sub connetStart()
        Try
            With WinSockCliente
                'Determino a donde se quiere conectar el usuario
                .IPDelHost = txtIP.Text
                .PuertoDelHost = txtPuerto.Text
                'Me conecto
                .Conectar()
            End With
        Catch ex As Exception
        End Try
    End Sub
    Private Sub reciveImage(ByVal imagen As Bitmap)
        WinSockCliente.widthSourceImage = imagen.Width
        WinSockCliente.heightSourceImage = imagen.Height
        PictureBox1.Image = imagen
    End Sub
    Private Sub WinSockCliente_DatosRecibidos(ByVal datos As String) Handles WinSockCliente.DatosRecibidos
        MsgBox("El servidor envio el siguiente mensaje: " & datos)
    End Sub
    Private Sub WinSockCliente_imagenRecibida(ByVal imagen As Bitmap) Handles WinSockCliente.imagenRecibida
        reciveImage(imagen)
    End Sub

    Public Sub sendDataTmr() Handles tmrSendData.Tick

    End Sub
    Private Sub WinSockCliente_ConexionTerminada() Handles WinSockCliente.ConexionTerminada
        MsgBox("Finalizo la conexion")
    End Sub

    Private Sub btnEnviarMensaje_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEnviarMensaje.Click
        flagSend = True
    End Sub

    Private Sub Form1_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        End
    End Sub


    Private Sub Form1_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles MyBase.KeyPress
        Dim stateButton As String = "None"
        Debug.Print("char " & e.KeyChar)
        If flagSend Then
            pressExec = True
            Dim data As New mData
            data.keyBoardEvent = "Key"
            data.eMouseEvent = stateButton
            data.keyB = e.KeyChar
            WinSockCliente.sendData(data)
        End If
    End Sub

    Private Sub Form1_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyDown

        Dim stateButton As String = "None"

        Debug.Print("code => " & e.KeyCode)
        Debug.Print("val => " & e.KeyValue)
        Debug.Print("data => " & e.KeyData)
        If (e.KeyCode >= 65 And e.KeyCode <= 90) Or
            (e.KeyCode >= 96 And e.KeyCode <= 105) Or
            (e.KeyCode >= 48 And e.KeyCode <= 57) Then

            'Debug.Print("es una letra o numero")
            Return
        End If

        If flagSend Then
            'Debug.Print("keyDown => " & e.KeyValue.ToString)

            Dim dataKey As String = ""
            Select Case e.KeyCode
                Case Keys.Enter
                    dataKey = ""
                Case Keys.F1
                    dataKey = "{F1}"
                Case Keys.F2
                    dataKey = "{F2}"
                Case Keys.F3
                    dataKey = "{F3}"
                Case Keys.F4
                    dataKey = "{F4}"
                Case Keys.F5
                    dataKey = "{F5}"
                Case Else
                    dataKey = ""
            End Select
            If dataKey <> "" Then
                Debug.Print("keydown => " & dataKey)
                Dim data As New mData
                data.keyBoardEvent = "Key"
                data.eMouseEvent = stateButton
                data.keyB = dataKey
                WinSockCliente.sendData(data)
            Else
                Return
            End If

        End If
    End Sub


    Private Sub PictureBox1_MouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseClick
        If flagSend Then
            Dim data As New mData
            data.xPosition = e.Location.X
            data.yPosition = e.Location.Y
            data.eMouseEvent = e.Button.ToString
            WinSockCliente.sendData(data)
            Debug.Print("Click " & e.Button.ToString)
        End If
    End Sub

    Private Sub PictureBox1_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseDoubleClick
        If flagSend Then
            Dim stateButton As String = "None"
            Dim data As New mData
            data.xPosition = e.Location.X
            data.yPosition = e.Location.Y
            stateButton = "DClick"
            data.eMouseEvent = stateButton
            WinSockCliente.sendData(data)
            data.eMouseEvent = "None"
        End If
    End Sub

    Private Sub PictureBox1_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseMove
        Dim stateButton As String = "None"
        If flagSend Then
            Dim data As New mData
            data.xPosition = e.Location.X
            data.yPosition = e.Location.Y
            stateButton = e.Button.ToString
            data.eMouseEvent = stateButton
            WinSockCliente.sendData(data)
        End If
    End Sub

    Private Sub PictureBox1_DragDrop(ByVal sender As Object, ByVal e As DragEventArgs) Handles MyBase.DragEnter
        Debug.Print("DragENTER " & e.Data.GetDataPresent(DataFormats.FileDrop))
        If (e.Data.GetDataPresent(DataFormats.FileDrop)) Then
            Dim filePaths As String() = CType(e.Data.GetData(DataFormats.FileDrop), String())
            'Procesar el file para ser enviado
            If filePaths.Length = 1 Then
                Dim fileLoc As String = filePaths(0)
                If File.Exists(fileLoc) Then
                    Dim data As New mData
                    data.eMouseEvent = "DragDrop"
                    data.fileName = fileLoc
                    Dim stringData As String = ""
                    Using tr As TextReader = New StreamReader(fileLoc)
                        stringData = tr.ReadToEnd()
                    End Using
                    data.stringData = stringData
                    WinSockCliente.sendData(data)
                    Debug.Print(data.stringData)
                End If

            Else
                MessageBox.Show("Solo archivos por favor")
            End If
            ' Display the copy cursor.
            e.Effect = DragDropEffects.Copy
        Else
            ' Display the no-drop cursor.
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.AllowDrop = True
    End Sub
End Class
