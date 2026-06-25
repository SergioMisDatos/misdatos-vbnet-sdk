Imports System.Net
Imports System.Text
Imports System.Web.Script.Serialization
Imports System.Runtime.InteropServices
Imports System.Collections

Namespace MisDatosSDK

    <ComVisible(True)>
    Public Class MdApi2

        ' 1. CORRECCIÓN: En VS 2010 declaramos las propiedades sin inicializarlas aquí
        Public Property UltimoMensajeError As String
        Public Property Usuario As String
        Public Property Password As String
        Public Property Modo As Integer
        Public Property Respuesta As Object

        ' Propiedad de solo lectura clásica para VS 2010
        Private _revision As String = "010.90"
        Public ReadOnly Property Revision As String
            Get
                Return _revision
            End Get
        End Property

        Private ReadOnly _endpoint As String = "https://api.misdatos.com.ar/"
        Private ReadOnly _endpointtest As String = "http://127.0.0.1:8000/gae2api/"

        Public Sub New()
            ' 2. CORRECCIÓN: Inicializamos los valores por defecto dentro del constructor
            UltimoMensajeError = ""
            Usuario = ""
            Password = ""
            Modo = 1
        End Sub

        Public Function Conectar() As Boolean
            UltimoMensajeError = ""
            If String.IsNullOrEmpty(Password) Then
                UltimoMensajeError = "Debe asignar un token antes de conectar"
                Return False
            End If
            Return True
        End Function

        Public Function MdSumarV01(Optional numero1 As Single = 0, Optional numero2 As Single = 0) As Single
            UltimoMensajeError = ""

            Try
                Dim payload = New With {.numero1 = numero1, .numero2 = numero2}
                Dim url As String = If(Modo = 1, _endpoint, _endpointtest) & "mdsumarv01"
                Dim responseBody As String = PostRequest(url, payload)

                If Not String.IsNullOrEmpty(responseBody) Then
                    Dim serializer As New JavaScriptSerializer()
                    Dim datosRespuesta As Dictionary(Of String, Object) = serializer.Deserialize(Of Dictionary(Of String, Object))(responseBody)

                    ' Ahora 'Respuesta' será reconocida sin problemas
                    Respuesta = datosRespuesta

                    If datosRespuesta.ContainsKey("status") AndAlso datosRespuesta("status").ToString() = "ok" Then
                        Return Convert.ToSingle(datosRespuesta("resultado"))
                    End If

                    UltimoMensajeError = If(datosRespuesta.ContainsKey("error"), datosRespuesta("error").ToString(), "Error lógico en servidor")
                End If
            Catch e As Exception
                UltimoMensajeError = "Error al ejecutar sumar: " & e.Message
            End Try
            Return 0.0F
        End Function

        Public Function MdWhatsappDestinoV01(Optional cdestino As String = "", Optional ccodigo As String = "", Optional cemail As String = "", Optional nestadoproveedor As String = "", Optional cfecha As String = "", Optional cnombre As String = "", Optional caccionclave As String = "") As String
            UltimoMensajeError = ""
            Dim cresultado As String = "0"

            Try
                Dim payload As New Dictionary(Of String, String)()
                If Not String.IsNullOrEmpty(cdestino) Then payload("destino") = cdestino
                If Not String.IsNullOrEmpty(cemail) Then payload("email") = cemail
                If Not String.IsNullOrEmpty(nestadoproveedor) Then payload("estadoproveedor") = nestadoproveedor
                If Not String.IsNullOrEmpty(cfecha) Then payload("fecha") = cfecha
                If Not String.IsNullOrEmpty(cnombre) Then payload("nombre") = cnombre
                If Not String.IsNullOrEmpty(ccodigo) Then payload("codigo") = ccodigo
                If Not String.IsNullOrEmpty(caccionclave) Then payload("accionclave") = caccionclave

                Dim url As String = If(Modo = 1, _endpoint, _endpointtest) & "mdwhatsappdestinov01"
                Dim responseBody As String = PostRequest(url, payload)

                If Not String.IsNullOrEmpty(responseBody) Then
                    Dim serializer As New JavaScriptSerializer()
                    Dim datosRespuesta As Dictionary(Of String, Object) = serializer.Deserialize(Of Dictionary(Of String, Object))(responseBody)
                    Respuesta = datosRespuesta

                    If datosRespuesta.ContainsKey("status") AndAlso datosRespuesta("status").ToString() = "ok" Then
                        cresultado = If(datosRespuesta.ContainsKey("resultado") AndAlso datosRespuesta("resultado") IsNot Nothing, datosRespuesta("resultado").ToString(), "0")
                    Else
                        UltimoMensajeError = If(datosRespuesta.ContainsKey("error"), datosRespuesta("error").ToString(), "Error lógico en servidor")
                    End If
                End If
            Catch e As Exception
                UltimoMensajeError = "Error local al ejecutar mdwhatsappdestinov01: " & e.Message
            End Try
            Return cresultado
        End Function

        Public Function MdWhatsappEnlaceV01(Optional ctipo As String = "", Optional ccodigo As String = "", Optional cnombre As String = "", Optional cfecha As String = "", Optional cenlace As String = "", Optional cusuariosql As String = "", Optional cdescripcion As String = "", Optional ntotal As String = "", Optional nprocesado As String = "", Optional caccionclave As String = "") As String
            UltimoMensajeError = ""
            Dim cresultado As String = "0"

            Try
                Dim payload As New Dictionary(Of String, String)()
                If Not String.IsNullOrEmpty(ctipo) Then payload("tipo") = ctipo
                If Not String.IsNullOrEmpty(ccodigo) Then payload("codigo") = ccodigo
                If Not String.IsNullOrEmpty(cnombre) Then payload("nombre") = cnombre
                If Not String.IsNullOrEmpty(cfecha) Then payload("fecha") = cfecha
                If Not String.IsNullOrEmpty(cenlace) Then payload("enlace") = cenlace
                If Not String.IsNullOrEmpty(cusuariosql) Then payload("usuariosql") = cusuariosql
                If Not String.IsNullOrEmpty(cdescripcion) Then payload("descripcion") = cdescripcion
                If Not String.IsNullOrEmpty(ntotal) Then payload("total") = ntotal
                If Not String.IsNullOrEmpty(nprocesado) Then payload("procesado") = nprocesado
                If Not String.IsNullOrEmpty(caccionclave) Then payload("accionclave") = caccionclave

                Dim url As String = If(Modo = 1, _endpoint, _endpointtest) & "mdhwatsappenlacev01"
                Dim responseBody As String = PostRequest(url, payload)

                If Not String.IsNullOrEmpty(responseBody) Then
                    Dim serializer As New JavaScriptSerializer()
                    Dim datosRespuesta As Dictionary(Of String, Object) = serializer.Deserialize(Of Dictionary(Of String, Object))(responseBody)
                    Respuesta = datosRespuesta

                    If datosRespuesta.ContainsKey("status") AndAlso datosRespuesta("status").ToString() = "ok" Then
                        cresultado = If(datosRespuesta.ContainsKey("resultado") AndAlso datosRespuesta("resultado") IsNot Nothing, datosRespuesta("resultado").ToString(), "0")
                    Else
                        UltimoMensajeError = If(datosRespuesta.ContainsKey("error"), datosRespuesta("error").ToString(), "Error lógico en servidor")
                    End If
                End If
            Catch e As Exception
                UltimoMensajeError = "Error local al ejecutar mdhwatsappenlacev01: " & e.Message
            End Try
            Return cresultado
        End Function

        Public Function MdWhatsappEnviarPlantilla01V01(Optional cdestino As String = "", Optional ccodigo As String = "", Optional caccion As String = "") As Single
            UltimoMensajeError = ""

            Try
                Dim payload = New With {.destino = cdestino, .codigo = ccodigo, .accion = caccion}
                Dim url As String = If(Modo = 1, _endpoint, _endpointtest) & "mdswhatssappenviarplantilla01v01"
                Dim responseBody As String = PostRequest(url, payload)

                If Not String.IsNullOrEmpty(responseBody) Then
                    Dim serializer As New JavaScriptSerializer()
                    Dim datosRespuesta As Dictionary(Of String, Object) = serializer.Deserialize(Of Dictionary(Of String, Object))(responseBody)
                    Respuesta = datosRespuesta

                    If datosRespuesta.ContainsKey("status") AndAlso datosRespuesta("status").ToString() = "ok" Then
                        Return Convert.ToSingle(datosRespuesta("resultado"))
                    End If
                    UltimoMensajeError = If(datosRespuesta.ContainsKey("error"), datosRespuesta("error").ToString(), "Error lógico en servidor")
                End If
            Catch e As Exception
                UltimoMensajeError = "Error al ejecutar mdwhatssappenviarplantilla: " & e.Message
            End Try
            Return 0.0F
        End Function

        Public Function LeerPropiedad(Optional cmetodo As String = "", Optional cpropiedad As String = "", Optional nindice As Integer = 0, Optional nsubindice As Integer = 0, Optional nsubsubindice As Integer = 0) As String
            Dim cresultado As String = ""
            UltimoMensajeError = ""
            cpropiedad = cpropiedad.Trim()
            cmetodo = cmetodo.ToLower().Trim()
            Dim lpropiedad As String() = cpropiedad.Split("."c)

            If cmetodo = "mdwhatsappenlacev01" OrElse cmetodo = "mdwhatssappenviarplantilla01v01" OrElse cmetodo = "mdsumarv01" OrElse cmetodo = "respuesta" Then
                If Respuesta IsNot Nothing Then
                    Try
                        Dim current As Object = Respuesta
                        For Each prop In lpropiedad
                            If prop = "diccionario" OrElse prop = "lista" Then Continue For

                            If TypeOf current Is Dictionary(Of String, Object) Then
                                Dim dict = DirectCast(current, Dictionary(Of String, Object))
                                If dict.ContainsKey(prop) Then
                                    current = dict(prop)
                                Else
                                    current = Nothing
                                    Exit For
                                End If
                            ElseIf TypeOf current Is Object() OrElse TypeOf current Is ArrayList Then
                                Dim idx As Integer
                                If Integer.TryParse(prop, idx) Then
                                    If TypeOf current Is Object() Then
                                        Dim arr = DirectCast(current, Object())
                                        If idx < arr.Length Then current = arr(idx) Else current = Nothing
                                    Else
                                        Dim arr = DirectCast(current, ArrayList)
                                        If idx < arr.Count Then current = arr(idx) Else current = Nothing
                                    End If
                                Else
                                    current = Nothing
                                    Exit For
                                End If
                            Else
                                current = Nothing
                                Exit For
                            End If
                        Next
                        If current IsNot Nothing Then cresultado = current.ToString()
                    Catch e As Exception
                        UltimoMensajeError = "Error objeto: " & e.Message
                    End Try
                End If
            Else
                UltimoMensajeError = "Método no definido o no compatible con leerPropiedad"
            End If

            Return cresultado
        End Function

        Private Function PostRequest(endpointUrl As String, payload As Object) As String
            Dim serializer As New JavaScriptSerializer()
            Dim jsonString As String = serializer.Serialize(payload)

            Using client As New WebClient()
                Try
                    client.Headers(HttpRequestHeader.ContentType) = "application/json"
                    client.Headers(HttpRequestHeader.Authorization) = "Bearer " & Usuario & " " & Password
                    client.Encoding = Encoding.UTF8

                    Dim response As String = client.UploadString(endpointUrl, "POST", jsonString)
                    Return response

                Catch ex As WebException
                    If ex.Response IsNot Nothing Then
                        Using reader As New IO.StreamReader(ex.Response.GetResponseStream())
                            Dim errorBody As String = reader.ReadToEnd()
                            Try
                                Dim errDict = serializer.Deserialize(Of Dictionary(Of String, Object))(errorBody)
                                If errDict IsNot Nothing AndAlso errDict.ContainsKey("error") Then
                                    UltimoMensajeError = errDict("error").ToString()
                                Else
                                    ' 3. CORRECCIÓN: Concatenación clásica para VS 2010 (reemplaza el caracter $)
                                    UltimoMensajeError = "HTTP Error: " & errorBody
                                End If
                            Catch
                                UltimoMensajeError = "HTTP Error: " & errorBody
                            End Try
                        End Using
                    Else
                        UltimoMensajeError = ex.Message
                    End If
                    Return Nothing
                End Try
            End Using
        End Function

    End Class

End Namespace