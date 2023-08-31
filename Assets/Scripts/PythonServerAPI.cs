using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Net.Sockets;
using System.Text;

using System.IO;
using System.Threading.Tasks;
using System;
using UnityEditor;
using Unity.VisualScripting;
using System.Threading;

public class PythonServerAPI : MonoBehaviour
{
    private Process pythonProcess;
    private TcpClient tcpClient;
    private NetworkStream networkStream;
    //public LoadingIndicator loadingIndicator;
    private bool serverBussy = false;
    private CancellationTokenSource cancellationTokenSource;

    public UIMenuController uiMenuController;

    private async void Start()
    {
        string apiPath = Application.streamingAssetsPath + "/PythonServer/3Drummers_api.py";
        string startServerCommand = $"python \"{apiPath}\"";
        cancellationTokenSource = new CancellationTokenSource();
        // Verificar si la conexión TCP está cerrada
        
        if (tcpClient != null && tcpClient.Connected)
        {
            CloseConnection();
        }
  
        StartPythonServer(startServerCommand);


        //loadingIndicator.ShowLoadingIndicator("System loading...");
        uiMenuController.BlockUI(true);
        await WaitForServerReady(cancellationTokenSource.Token);

        uiMenuController.BlockUI(false);
        //loadingIndicator.HideLoadingIndicator();
    }

    public void StartPythonServer(string startCommand) 
    { 
        ProcessStartInfo processStartInfo = new ProcessStartInfo();
        processStartInfo.FileName = "cmd.exe";
        processStartInfo.RedirectStandardInput = true;
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardError = true;
        processStartInfo.UseShellExecute = false;
        processStartInfo.CreateNoWindow = true;
        pythonProcess = new Process();
        pythonProcess.StartInfo = processStartInfo;

        pythonProcess.OutputDataReceived += (sender, e) => { Debug.Log("Python Output: " + e.Data); };
        pythonProcess.ErrorDataReceived += (sender, e) => { Debug.LogError("Python Error: " + e.Data); };


        pythonProcess.Start();
        pythonProcess.StandardInput.WriteLine("conda activate 3Drummers");
        pythonProcess.StandardInput.WriteLine(startCommand);

        pythonProcess.BeginOutputReadLine();
        pythonProcess.BeginErrorReadLine();

    }

    private void OnApplicationQuit()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            cancellationTokenSource?.Cancel();
            Debug.Log("Asesine al Task");
            CloseConnection();
        }
    }
    //Capa de red
    private void ConnectToPythonServer() 
    {
        string host = "127.0.0.1";
        int port = 2444;

        tcpClient = new TcpClient(host, port);
        networkStream = tcpClient.GetStream();
    }
    private void SendCommandToServer(string command, Dictionary<string, string> parameters = null)
    {
        ConnectToPythonServer();
        string jsonMessage = ConstructJsonMessage(command, parameters);

        byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
        networkStream.Write(messageBytes, 0, messageBytes.Length);
    }
    private async Task<string> ReceiveResponseFromServer()
    {
        byte[] responseBytes = new byte[1024];
        int bytesRead = await networkStream.ReadAsync(responseBytes, 0, responseBytes.Length);
        string response = Encoding.UTF8.GetString(responseBytes, 0, bytesRead);
        return response;
    }
    private void ProcessServerResponse(string response)
    {
        ResponseData responseData = JsonUtility.FromJson<ResponseData>(response);
        string serverResponse = responseData.response;
    }

    private string ConstructJsonMessage(string command, Dictionary<string, string> parameters)
    {
        string parametersJson = parameters != null ? ConstructParametersJson(parameters) : "{}";
        string jsonString = $"{{\"command\": \"{command}\", \"parameters\": {parametersJson}}}";
        return jsonString;
    }

    private string ConstructParametersJson(Dictionary<string, string> parameters)
    {
        string parametersJson = "{";
        foreach (var kvp in parameters)
        {
            parametersJson += $"\"{kvp.Key}\": \"{kvp.Value}\",";
        }
        parametersJson = parametersJson.TrimEnd(',') + "}";
        return parametersJson;
    }
    private async Task WaitForServerReady(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                serverBussy = true;
                ConnectToPythonServer();

                string readyCheckMessage = "{\"command\": \"ready_check\"}";
                byte[] readyCheckBytes = Encoding.UTF8.GetBytes(readyCheckMessage);

                await networkStream.WriteAsync(readyCheckBytes, 0, readyCheckBytes.Length);

                byte[] buffer = new byte[1024];
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var response = JsonUtility.FromJson<ResponseData>(message);

                while (response.response != "ready")
                {
                    bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    response = JsonUtility.FromJson<ResponseData>(message);
                    await Task.Yield();
                }

                serverBussy = false;
                break;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                await Task.Delay(10000); // Esperar 10 segundos antes de intentar nuevamente
            }
        }
    }

    //Está es la función que hay que usar XD
    public async Task CreateCommand(string command, Dictionary<string, string> parameters = null) {
        if (serverBussy) {
            return;
        }
        serverBussy = true;
        try
        {
            SendCommandToServer(command, parameters);
            string response = await ReceiveResponseFromServer();
            ProcessServerResponse(response);
        }
        catch (Exception ex) 
        {
            Debug.LogError(ex.Message);
            return;
        }
        finally { serverBussy = false; }
    }

    //TEST
    private void CloseConnection() 
    {
        SendCommandToServer("shutdown");
        if (tcpClient != null)
        {
            Debug.Log("Cerre conexión");
            tcpClient.Close();
            pythonProcess.Kill();
        }
    }
}

//Json Format
[System.Serializable]
public class ResponseData
{
    public string response;
}