' -*- coding: utf-8 -*-
Imports System.Net
Imports System.Text
Imports System.Web.Script.Serialization
Imports System.Runtime.InteropServices
Imports System.Collections
Imports System.Collections.Generic
Imports System.Threading
Imports System.IO

Namespace MisDatosSDK

    ''' <summary>
    ''' Clase auxiliar para representar la respuesta HTTP al estilo de 'requests.Response' de Python.
    ''' </summary>
    <ComVisible(True)>
    Public Class ApiResponse
        Public Property StatusCode As Integer
        Public Property Body As String

        Public Function Json() As Dictionary(Of String, Object)
            Dim serializer As New JavaScriptSerializer()
            Return serializer.Deserialize(Of Dictionary(Of String, Object))(Body)
        End Function
    End Class

    ''' <summary>
    ''' Cliente API nativo para interactuar con los servicios de MisDatos.
    ''' </summary>
    <ComVisible(True)>
    Public Class mdapi2

        ' -- Campos privados que respaldan las propiedades --
        Private _ultimomensajeerror As String = ""
        Private _usuario As String = ""
        Private _password As String = ""
        Private _modo As Integer = 1 ' 1 = Producción, otro valor = Test
        Private _endpoint As String = "https://api.misdatos.com.ar/"
        Private _endpointtest As String = "http://127.0.0.1:8000/gae2api/"
        Private _service As Object = Nothing ' Representa el estado de sesión conectada
        Private _revision As String = "010.90"
        Private _respuesta As Object = Nothing

        ' -- Constructor (__init__) --
        Public Sub New()
            _ultimomensajeerror = ""
            _usuario = ""
            _password = ""
            _modo = 1 ' 1 = Producción, otro valor = Test
            _endpoint = "https://api.misdatos.com.ar/"
            _endpointtest = "http://127.0.0.1:8000/gae2api/"
            _service = Nothing
            _revision = "010.90"
            _respuesta = Nothing
        End Sub

        Public Function conectar() As Boolean
            _ultimomensajeerror = ""
            Dim bresultado As Boolean = False
            If String.IsNullOrEmpty(_password) Then
                _ultimomensajeerror = "Debe asignar un token antes de conectar"
                Return False
            End If

            Try
                ' 1. Instanciamos la sesión (En .NET 4.0 marcamos el objeto de estado de sesión)
                ' 2 y 3. Configuramos la estrategia de reintentos.
                ' Nota: Al no existir un equivalente nativo a requests.Session con acople de adaptadores
                ' en .NET 4.0, la lógica de reintento exponencial se gestiona en la función PostRequest.
                _service = New Object() ' Sesión preparada de forma exitosa
                
                ' 4. Actualizamos los headers (Se aplicarán dinámicamente en cada petición mediante PostRequest)
                bresultado = True
            Catch e As Exception
                _ultimomensajeerror = "Error al preparar conexión API: " & e.Message
            End Try

            Return bresultado
        End Function

        ''' <summary>
        ''' Método de utilidad para recorrer la respuesta JSON estructurada.
        ''' </summary>
        Public Function leerPropiedad(Optional cmetodo As String = "", Optional cpropiedad As String = "", Optional nindice As Integer = 0, Optional nsubindice As Integer = 0, Optional nsubsubindice As Integer = 0) As String
            Dim cresultado As String = ""
            _ultimomensajeerror = ""
            cpropiedad = cpropiedad.Trim()
            cmetodo = cmetodo.ToLower().Trim()
            Dim lpropiedad As String() = cpropiedad.Split("."c)
            Dim nindicediccionario As Integer = 0
            Dim nindicelista As Integer = 0
            Dim ldiccionario As New List(Of Dictionary(Of String, Object))()
            Dim llista As New List(Of IList)()

            Dim cmetodosCompatibles As New List(Of String)(New String() {"mdwhatsappenlacev01", "mdwhatssappenviarplantilla01v01", "mdsumarv01", "respuesta"})

            If cmetodosCompatibles.Contains(cmetodo) Then
                Dim objeto As Object = _respuesta
                If objeto IsNot Nothing Then
                    If TypeOf objeto Is IList AndAlso Not TypeOf objeto Is String Then
                        llista.Add(DirectCast(objeto, IList))
                    ElseIf TypeOf objeto Is Dictionary(Of String, Object) Then
                        ldiccionario.Add(DirectCast(objeto, Dictionary(Of String, Object)))
                    End If

                    Try
                        Dim nindice_prop As Integer = 0
                        While nindice_prop <= lpropiedad.Length - 1
                            If lpropiedad(nindice_prop) = "diccionario" Then
                                nindice_prop += 1
                                If lpropiedad(nindice_prop) = "itemcantidad" Then
                                    objeto = ldiccionario(nindicediccionario).Count.ToString()
                                Else
                                    objeto = ldiccionario(nindicediccionario)(lpropiedad(nindice_prop))
                                    nindicediccionario += 1
                                End If
                            ElseIf lpropiedad(nindice_prop) = "lista" Then
                                nindice_prop += 1
                                If lpropiedad(nindice_prop) = "itemcantidad" Then
                                    objeto = llista(nindicelista).Count.ToString()
                                Else
                                    Dim idx As Integer = Convert.ToInt32(lpropiedad(nindice_prop))
                                    objeto = llista(nindicelista)(idx)
                                    nindicelista += 1
                                End If
                            End If

                            cresultado = If(objeto IsNot Nothing, objeto.ToString(), "")
                            nindice_prop += 1

                            If objeto IsNot Nothing Then
                                If TypeOf objeto Is IList AndAlso Not TypeOf objeto Is String Then
                                    llista.Add(DirectCast(objeto, IList))
                                ElseIf TypeOf objeto Is Dictionary(Of String, Object) Then
                                    ldiccionario.Add(DirectCast(objeto, Dictionary(Of String, Object)))
                                End If
                            End If
                        End While
                    Catch e As Exception
                        _ultimomensajeerror = "error objeto " & nindice_prop & e.Message
                        cresultado = ""
                    End Try
                End If
            Else
                _ultimomensajeerror = "Método no definido o no compatible con leerPropiedad"
            End If

            Return cresultado
        End Function

        Public Function mdwhatsappenlaceurlv01(Optional nid_enlace As Integer = 0, Optional cruta_archivo As String = "") As String
            _ultimomensajeerror = ""
            Dim cresultado As String = "0"

            If _service Is Nothing Then
                _ultimomensajeerror = "No hay conexión. Ejecute conectar() primero."
                Return cresultado
            End If

            If Not File.Exists(cruta_archivo) Then
                _ultimomensajeerror = "El archivo no existe: " & cruta_archivo
                Return cresultado
            End If

            ' Validación de tamaño en el lado del cliente (aprox 1000 MB)
            Dim LIMITE_KB As Long = 1000000
            Dim LIMITE_BYTES As Long = LIMITE_KB * 1024
            Dim fileInfo As New FileInfo(cruta_archivo)
            Dim peso_archivo As Long = fileInfo.Length

            If peso_archivo > LIMITE_BYTES Then
                _ultimomensajeerror = "El archivo supera el tamaño máximo permitido de " & LIMITE_KB & " KB."
                Return cresultado
            End If

            Try
                Dim contenido_bytes As Byte() = File.ReadAllBytes(cruta_archivo)
                Dim archivo_b64 As String = Convert.ToBase64String(contenido_bytes)
                Dim nombre_archivo As String = Path.GetFileName(cruta_archivo)

                Dim payload As New Dictionary(Of String, Object)()
                payload("id_enlace") = nid_enlace
                payload("archivo_b64") = archivo_b64
                payload("nombre_archivo") = nombre_archivo

                Dim url As String = If(_modo = 1, _endpoint, _endpointtest) & "mdwhatsappenlaceurlv01"
                Dim respuestaHttp As ApiResponse = PostRequest(url, payload)

                If respuestaHttp IsNot Nothing AndAlso respuestaHttp.StatusCode = 200 Then
                    Dim datos_respuesta As Dictionary(Of String, Object) = respuestaHttp.Json()
                    _respuesta = datos_respuesta
                    If datos_respuesta.ContainsKey("status") AndAlso datos_respuesta("status").ToString() = "ok" Then
                        cresultado = If(datos_respuesta.ContainsKey("resultado") AndAlso datos_respuesta("resultado") IsNot Nothing, datos_respuesta("resultado").ToString(), "0")
                    Else
                        _ultimomensajeerror = If(datos_respuesta.ContainsKey("error"), datos_respuesta("error").ToString(), "Error lógico en servidor")
                    End If
                Else
                    If respuestaHttp IsNot Nothing Then
                        _ultimomensajeerror = "HTTP " & respuestaHttp.StatusCode & ": " & respuestaHttp.Body
                    End If
                End If

            Catch e As Exception
                _ultimomensajeerror = "Error local al ejecutar mdwhatsappenlaceurlv01: " & e.Message
            End Try

            Return cresultado
        End Function

        Public Function mdobteneraccesov02(Optional nproveedor As Integer = 0, Optional cplan As String = "gratuito", Optional cservicio As String = "", Optional csubservicio As String = "", Optional cdestino As String = "", Optional ncantidad As Integer = 1) As String
            _ultimomensajeerror = ""
            Dim cresultado As String = "0"

            If _service Is Nothing Then
                _ultimomensajeerror = "No hay conexión. Ejecute conectar() primero."
                Return cresultado
            End If

            Try
                Dim payload As New Dictionary(Of String, Object)()
                payload("proveedor") = nproveedor
                payload("plan") = cplan
                payload("servicio") = cservicio
                payload("subservicio") = csubservicio
                payload("destino") = cdestino
                payload("cantidad") = ncantidad

                Dim url As String = If(_modo = 1, _endpoint, _endpointtest) & "mdobteneraccesov02"
                Dim respuestaHttp As ApiResponse = PostRequest(url, payload)

                If respuestaHttp IsNot Nothing AndAlso respuestaHttp.StatusCode = 200 Then
                    Dim datos_respuesta As Dictionary(Of String, Object) = respuestaHttp.Json()
                    _respuesta = datos_respuesta
                    If datos_respuesta.ContainsKey("status") AndAlso datos_respuesta("status").ToString() = "ok" Then
                        cresultado = If(datos_respuesta.ContainsKey("resultado") AndAlso datos_respuesta("resultado") IsNot Nothing, datos_respuesta("resultado").ToString(), "0")
                    Else
                        _ultimomensajeerror = If(datos_respuesta.ContainsKey("error"), datos_respuesta("error").ToString(), "Error lógico en servidor")
                    End If
                Else
                    If respuestaHttp IsNot Nothing Then
                        _ultimomensajeerror = "HTTP " & respuestaHttp.StatusCode & ": " & respuestaHttp.Body
                    End If
                End If

            Catch e As Exception
                _ultimomensajeerror = "Error local al ejecutar mdobteneraccesov02: " & e.Message
            End Try

            Return cresultado
        End Function

        Public Function mdsumarv01(Optional numero1 As Single = 0, Optional numero2 As Single = 0) As Single
            _ultimomensajeerror = ""
            If _service Is Nothing Then
                _ultimomensajeerror = "No hay conexión. Ejecute conectar() primero."
                Return 0.0F
            End If

            Try
                Dim payload As New Dictionary(Of String, Object)()
                payload("numero1") = numero1
                payload("numero2") = numero2

                Dim url As String = If(_modo = 1, _endpoint, _endpointtest) & "mdsumarv01"
                Dim respuestaHttp As ApiResponse = PostRequest(url, payload)

                If respuestaHttp IsNot Nothing AndAlso respuestaHttp.StatusCode = 200 Then
                    Dim datos_respuesta As Dictionary(Of String, Object) = respuestaHttp.Json()
                    If datos_respuesta.ContainsKey("status") AndAlso datos_respuesta("status").ToString() = "ok" Then
                        Return Convert.ToSingle(If(datos_respuesta.ContainsKey("resultado"), datos_respuesta("resultado"), 0.0F))
                    Else
                        _ultimomensajeerror = "Error lógico en servidor"
                    End If
                Else
                    If respuestaHttp IsNot Nothing Then
                        _ultimomensajeerror = "HTTP " & respuestaHttp.StatusCode & ": " & respuestaHttp.Body
                    End If
                End If

            Catch e As Exception
                _ultimomensajeerror = "Error al ejecutar sumar: " & e.Message
            End Try

            Return 0.0F
        End Function

        Public Function mdwhatsappdestinov01(Optional cdestino As String = "", Optional ccodigo As String = "", Optional cemail As String = "", Optional nestadoproveedor As String = "", Optional cfecha As String = "", Optional cnombre As String = "", Optional caccionclave As String = "") As String
            _ultimomensajeerror = ""
            Dim cresultado As String = "0"

            If _service Is Nothing Then
                _ultimomensajeerror = "No hay conexión. Ejecute conectar() primero."
                Return cresultado
            End If

            Try
                Dim payload As New Dictionary(Of String, Object)()
                If cdestino <> "" Then payload("destino") = cdestino
                If cemail <> "" Then payload("email") = cemail
                If nestadoproveedor <> "" Then payload("estadoproveedor") = nestadoproveedor
                If cfecha <> "" Then payload("fecha") = cfecha
                If cnombre <> "" Then payload("nombre") = cnombre
                If ccodigo <> "" Then payload("codigo") = ccodigo
                If caccionclave <> "" Then payload("accionclave") = caccionclave

                Dim url As String = If(_modo = 1, _endpoint, _endpointtest) & "mdwhatsappdestinov01"
                Dim respuestaHttp As ApiResponse = PostRequest(url, payload)

                If respuestaHttp IsNot Nothing AndAlso respuestaHttp.StatusCode = 200 Then
                    Dim datos_respuesta As Dictionary(Of String, Object) = respuestaHttp.Json()
                    _respuesta = datos_respuesta
                    If datos_respuesta.ContainsKey("status") AndAlso datos_respuesta("status").ToString() = "ok" Then
                        cresultado = If(datos_respuesta.ContainsKey("resultado") AndAlso datos_respuesta("resultado") IsNot Nothing, datos_respuesta("resultado").ToString(), "0")
                    Else
                        _ultimomensajeerror = If(datos_respuesta.ContainsKey("error"), datos_respuesta("error").ToString(), "Error lógico en servidor")
                    End If
                Else
                    If respuestaHttp IsNot Nothing Then
                        _ultimomensajeerror = "HTTP " & respuestaHttp.StatusCode & ": " & respuestaHttp.Body
                    End If
                End If

            Catch e As Exception
                _ultimomensajeerror = "Error local al ejecutar mdwhatsappdestinov01: " & e.Message
            End Try

            Return cresultado
        End Function

        Public Function mdwhatsappenlacev01(Optional ctipo As String = "", Optional ccodigo As String = "", Optional cnombre As String = "", Optional cfecha As String = "", Optional cenlace As String = "", Optional cusuariosql As String = "", Optional cdescripcion As String = "", Optional ntotal As String = "", Optional nprocesado As String = "", Optional caccionclave As String = "") As String
            Return mdhwatsappenlacev01(ctipo, ccodigo, cnombre, cfecha, cenlace, cusuariosql, cdescripcion, ntotal, nprocesado, caccionclave)
        End Function

        Public Function mdhwatsappenlacev01(Optional ctipo As String = "", Optional ccodigo As String = "", Optional cnombre As String = "", Optional cfecha As String = "", Optional cenlace As String = "", Optional cusuariosql As String = "", Optional cdescripcion As String = "", Optional ntotal As String = "", Optional nprocesado As String = "", Optional caccionclave As String = "") As String
            _ultimomensajeerror = ""
            Dim cresultado As String = "0"

            If _service Is Nothing Then
                _ultimomensajeerror = "No hay conexión. Ejecute conectar() primero."
                Return cresultado
            End If

            Try
                Dim payload As New Dictionary(Of String, Object)()
                If ctipo <> "" Then payload("tipo") = ctipo
                If ccodigo <> "" Then payload("codigo") = ccodigo
                If cnombre <> "" Then payload("nombre") = cnombre
                If cfecha <> "" Then payload("fecha") = cfecha
                If cenlace <> "" Then payload("enlace") = cenlace
                If cusuariosql <> "" Then payload("usuariosql") = cusuariosql
                If cdescripcion <> "" Then payload("descripcion") = cdescripcion
                If ntotal <> "" Then payload("total") = ntotal
                If nprocesado <> "" Then payload("procesado") = nprocesado
                If caccionclave <> "" Then payload("accionclave") = caccionclave

                Dim url As String = If(_modo = 1, _endpoint, _endpointtest) & "mdhwatsappenlacev01"
                Dim respuestaHttp As ApiResponse = PostRequest(url, payload)

                If respuestaHttp IsNot Nothing AndAlso respuestaHttp.StatusCode = 200 Then
                    Dim datos_respuesta As Dictionary(Of String, Object) = respuestaHttp.Json()
                    _respuesta = datos_respuesta
                    If datos_respuesta.ContainsKey("status") AndAlso datos_respuesta("status").ToString() = "ok" Then
                        cresultado = If(datos_respuesta.ContainsKey("resultado") AndAlso datos_respuesta("resultado") IsNot Nothing, datos_respuesta("resultado").ToString(), "0")
                    Else
                        _ultimomensajeerror = If(datos_respuesta.ContainsKey("error"), datos_respuesta("error").ToString(), "Error lógico en servidor")
                    End If
                Else
                    If respuestaHttp IsNot Nothing Then
                        _ultimomensajeerror = "HTTP " & respuestaHttp.StatusCode & ": " & respuestaHttp.Body
                    End If
                End If

            Catch e As Exception
                _ultimomensajeerror = "Error local al ejecutar mdhwatsappenlacev01: " & e.Message
            End Try

            Return cresultado
        End Function

        Public Function mdwhatssappenviarplantilla01v01(Optional cdestino As String = "", Optional ccodigo As String = "", Optional caccion As String = "") As Single
            _ultimomensajeerror = ""
            If _service Is Nothing Then
                _ultimomensajeerror = "No hay conexión. Ejecute conectar() primero."
                Return 0.0F
            End If

            Try
                Dim payload As New Dictionary(Of String, Object)()
                payload("destino") = cdestino
                payload("codigo") = ccodigo
                payload("accion") = caccion

                Dim url As String = If(_modo = 1, _endpoint, _endpointtest) & "mdswhatssappenviarplantilla01v01"
                Dim respuestaHttp As ApiResponse = PostRequest(url, payload)

                If respuestaHttp IsNot Nothing AndAlso respuestaHttp.StatusCode = 200 Then
                    Dim datos_respuesta As Dictionary(Of String, Object) = respuestaHttp.Json()
                    _respuesta = datos_respuesta
                    If datos_respuesta.ContainsKey("status") AndAlso datos_respuesta("status").ToString() = "ok" Then
                        Return Convert.ToSingle(If(datos_respuesta.ContainsKey("resultado"), datos_respuesta("resultado"), 0.0F))
                    Else
                        _ultimomensajeerror = If(datos_respuesta.ContainsKey("error"), datos_respuesta("error").ToString(), "Error lógico en servidor")
                    End If
                Else
                    If respuestaHttp IsNot Nothing Then
                        _ultimomensajeerror = "HTTP " & respuestaHttp.StatusCode & ": " & respuestaHttp.Body
                        Try
                            Dim datos_respuesta As Dictionary(Of String, Object) = respuestaHttp.Json()
                            If datos_respuesta IsNot Nothing AndAlso datos_respuesta.ContainsKey("error") Then
                                _ultimomensajeerror = datos_respuesta("error").ToString()
                            End If
                        Catch
                            ' No hacer nada si falla el parseo secundario del error
                        End Try
                    End If
                End If

            Catch e As Exception
                _ultimomensajeerror = "Error al ejecutar plantilla: " & e.Message
            End Try

            Return 0.0F
        End Function

        ' -- Implementación Nv. de Reintentos Exponenciales de requests de Python --
        Private Function PostRequest(ByVal endpointUrl As String, ByVal payload As Object) As ApiResponse
            Dim serializer As New JavaScriptSerializer()
            Dim jsonString As String = serializer.Serialize(payload)
            Dim maxRetries As Integer = 3 ' total=3
            Dim backoffFactor As Double = 0.5 ' backoff_factor=0.5
            Dim statusForceList As New List(Of Integer)(New Integer() {429, 500, 502, 503, 504}) ' status_forcelist

            Dim attempt As Integer = 0
            Dim responseObj As ApiResponse = Nothing

            While attempt <= maxRetries
                attempt += 1
                Try
                    Dim request As HttpWebRequest = DirectCast(WebRequest.Create(endpointUrl), HttpWebRequest)
                    request.Method = "POST"
                    request.ContentType = "application/json"
                    request.Headers("Authorization") = "Bearer " & _usuario & " " & _password
                    request.Timeout = 100000 ' 100 segundos por defecto de .NET

                    ' Escribir el cuerpo del payload
                    Dim bytes As Byte() = Encoding.UTF8.GetBytes(jsonString)
                    request.ContentLength = bytes.Length
                    Using requestStream As Stream = request.GetRequestStream()
                        requestStream.Write(bytes, 0, bytes.Length)
                    End Using

                    ' Obtener respuesta
                    Using response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)
                        responseObj = New ApiResponse()
                        responseObj.StatusCode = Convert.ToInt32(response.StatusCode)
                        Using reader As New StreamReader(response.GetResponseStream(), Encoding.UTF8)
                            responseObj.Body = reader.ReadToEnd()
                        End Using
                        Return responseObj ' Éxito directo, salimos del bucle
                    End Using

                Catch ex As WebException
                    Dim shouldRetry As Boolean = False
                    Dim statusCode As Integer = 0

                    If ex.Response IsNot Nothing Then
                        Dim webResponse As HttpWebResponse = DirectCast(ex.Response, HttpWebResponse)
                        statusCode = Convert.ToInt32(webResponse.StatusCode)

                        responseObj = New ApiResponse()
                        responseObj.StatusCode = statusCode
                        Try
                            Using reader As New StreamReader(webResponse.GetResponseStream(), Encoding.UTF8)
                                responseObj.Body = reader.ReadToEnd()
                            End Using
                        Catch
                            responseObj.Body = ex.Message
                        End Try

                        ' Reintentamos solo si el código HTTP está en la lista de forzados
                        If statusForceList.Contains(statusCode) Then
                            shouldRetry = True
                        End If
                    Else
                        ' Sin respuesta (fallo de DNS, Timeout de red, conexión perdida). Reintentamos.
                        shouldRetry = True
                    End If

                    ' Detener si no aplica reintento o superamos el límite máximo
                    If Not shouldRetry OrElse attempt > maxRetries Then
                        If responseObj IsNot Nothing Then
                            _ultimomensajeerror = "HTTP " & responseObj.StatusCode & ": " & responseObj.Body
                        Else
                            _ultimomensajeerror = ex.Message
                        End If
                        Exit While
                    End If

                    ' Cálculo del backoff exponencial: factor * (2 ^ (intento - 1))
                    Dim sleepMs As Integer = Convert.ToInt32(backoffFactor * Math.Pow(2, attempt - 1) * 1000)
                    Thread.Sleep(sleepMs)

                Catch ex As Exception
                    ' Error local inesperado (fuera de fallos HTTP de red), detenemos reintentos
                    _ultimomensajeerror = ex.Message
                    Exit While
                End Try
            End While

            Return responseObj
        End Function

        ' -- Propiedades (Replica fiel de los decoradores @property de Python) --
        Public Property ultimomensajeerror As String
            Get
                Return _ultimomensajeerror
            End Get
            Set(ByVal value As String)
                _ultimomensajeerror = value
            End Set
        End Property

        Public Property password As String
            Get
                Return _password
            End Get
            Set(ByVal value As String)
                _password = value
            End Set
        End Property

        Public Property usuario As String
            Get
                Return _usuario
            End Get
            Set(ByVal value As String)
                _usuario = value
            End Set
        End Property

        Public Property modo As Integer
            Get
                Return _modo
            End Get
            Set(ByVal value As Integer)
                _modo = value
            End Set
        End Property

        Public ReadOnly Property revision As String
            Get
                Return _revision
            End Get
        End Property

        Public ReadOnly Property service As Object
            Get
                Return _service
            End Get
        End Property

        Public ReadOnly Property endpoint As String
            Get
                Return _endpoint
            End Get
        End Property

        Public ReadOnly Property endpointtest As String
            Get
                Return _endpointtest
            End Get
        End Property

        Public Property respuesta As Object
            Get
                Return _respuesta
            End Get
            Set(ByVal value As Object)
                _respuesta = value
            End Set
        End Property

    End Class

End Namespace
