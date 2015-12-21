Imports System
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Text
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.InteropServices
<Serializable()> _
Public Class mData
    Public xPosition As Integer = vbNullString
    Public yPosition As Integer = vbNullString
    Public eMouseEvent As String = vbNullString
    Public keyBoardEvent As String = vbNullString
    Public keyB As String = vbNullString
End Class
Public Class Cliente
    <StructLayout(LayoutKind.Sequential)> _
    Public Structure CURSORINFO
        Public cbSize As Int32
        Public flags As Int32
        Public hCursor As IntPtr
        Public ptScreenPos As Point
    End Structure
    <DllImport("user32.dll", EntryPoint:="GetCursorInfo")> _
    Public Shared Function GetCursorInfo(ByRef pci As CURSORINFO) As Boolean
    End Function
#Region "VARIABLES"
    Private Stm As Stream 'Utilizado para enviar datos al Servidor y recibir datos del mismo
    Private m_IPDelHost As String 'Direccion del objeto de la clase Servidor
    Private m_PuertoDelHost As String 'Puerto donde escucha el objeto de la clase Servidor
#End Region

#Region "EVENTOS"
    Public Event ConexionTerminada()
    Public Event DatosRecibidos(ByVal datos As String)
    Public Event imagenRecibida(ByVal bmp As Bitmap)
    Dim tcpClnt As TcpClient
    Dim ns As NetworkStream
    Public widthSourceImage As Integer
    Public heightSourceImage As Integer
#End Region

#Region "PROPIEDADES"
    Public Property IPDelHost() As String
        Get
            IPDelHost = m_IPDelHost
        End Get

        Set(ByVal Value As String)
            m_IPDelHost = Value
        End Set
    End Property

    Public Property PuertoDelHost() As String
        Get
            PuertoDelHost = m_PuertoDelHost
        End Get
        Set(ByVal Value As String)
            m_PuertoDelHost = Value
        End Set
    End Property
#End Region

#Region "METODOS"
    Public Sub Conectar()
        Try
            Dim tcpThd As Thread 'Se encarga de escuchar mensajes enviados por el Servidor
            tcpClnt = New TcpClient()
            'Me conecto al objeto de la clase Servidor,
            '  determinado por las propiedades IPDelHost y PuertoDelHost
            tcpClnt.Connect(IPDelHost, PuertoDelHost)
            Stm = tcpClnt.GetStream()
            'Creo e inicio un thread para que escuche los mensajes enviados por el Servidor
            tcpThd = New Thread(AddressOf LeerSocket)
            tcpThd.Start()
        Catch ex As Exception
            MessageBox.Show(ex.ToString)
        End Try

    End Sub
    Public Sub sendData(ByVal data As mData)
        Try
            If ns.CanWrite Then
                Dim bf As New BinaryFormatter
                If data.eMouseEvent <> vbNullString Then
                    'Write mouse positions
                    Dim indH As Double
                    Dim indW As Double
                    Dim Hpb As Long = Form1.PictureBox1.Height
                    Dim Wpb As Long = Form1.PictureBox1.Width
                    Dim x As Long
                    Dim y As Long
                    'SI EL ALTO DE LA IMAGEN ORIGEN ES MAYOR AL DE EL IMAGE VIEW
                    If heightSourceImage >= Hpb Then
                        indH = heightSourceImage / Hpb
                        y = Math.Floor(data.yPosition * indH)
                    Else
                        indH = Hpb / heightSourceImage
                        y = Math.Floor(data.yPosition / indH)
                    End If
                    'SI EL ANCHO DE LA IMAGEN ORIGEN ES MAYOR AL IMAGEN VIEW
                    If widthSourceImage >= Wpb Then
                        indW = widthSourceImage / Wpb
                        x = Math.Floor(data.xPosition * indW)
                    Else
                        indW = Wpb / widthSourceImage
                        x = Math.Floor(data.xPosition / indW)
                    End If
                    Dim obMouse As New Hashtable
                    obMouse.Add("xPosition", x)
                    obMouse.Add("yPosition", y)
                    obMouse.Add("stringEvent", data.eMouseEvent)
                    bf.Serialize(ns, obMouse)
                End If
                'Send key 
                If data.keyBoardEvent <> vbNullString Then
                    Dim obMouse As New Hashtable
                    obMouse.Add("key", data.keyB)
                    obMouse.Add("keyBoardEvent", data.keyBoardEvent)
                    bf.Serialize(ns, obMouse)
                End If
                ns.Flush()
            Else
                Form1.Label1.Text &= "no se puede escribir"
            End If
        Catch ex As Exception
            MessageBox.Show(ex.ToString)
        End Try
    End Sub

    Public Sub EnviarDatos(ByVal Datos As String)
        Dim BufferDeEscritura() As Byte
        BufferDeEscritura = Encoding.ASCII.GetBytes(Datos)
        If Not (Stm Is Nothing) Then
            'Envio los datos al Servidor
            Stm.Write(BufferDeEscritura, 0, BufferDeEscritura.Length)
        End If
    End Sub
#End Region

#Region "FUNCIONES PRIVADAS"
    Private Sub LeerSocket()
        Dim bf As New BinaryFormatter
        Try
            While tcpClnt.Connected = True
                ns = tcpClnt.GetStream
                RaiseEvent imagenRecibida(bf.Deserialize(ns))
                ns.Flush()
            End While
        Catch ex As Exception
            RaiseEvent ConexionTerminada()
        End Try
    End Sub
#End Region

End Class
