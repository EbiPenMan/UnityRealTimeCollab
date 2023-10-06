using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using ChistaGame.RealTimeCollab.Editor.Models;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using Debug = UnityEngine.Debug;
using Formatting = Newtonsoft.Json.Formatting;

namespace ChistaGame.RealTimeCollab.Editor
{
    public class RealTimeCollabManager : EditorWindow
    {
        private bool _isCreatedServer;
        private TcpListener _server;
        private TcpClient _client;
        private WebSocketServer _wssv;


        [MenuItem("ChistaGame/RealTime Collab/Main Window")]
        public static void ShowExample()
        {
            RealTimeCollabManager wnd = GetWindow<RealTimeCollabManager>();
            wnd.titleContent = new GUIContent("MyEditorWindow");
        }

        public RealTimeCollabManager()
        {
            Debug.unityLogger.Log("RealTimeCollabManager | Constructor | Start");
        }

        public async UniTask<bool> CreateServer()
        {
            if (_isCreatedServer) return true;
            Debug.unityLogger.Log("RealTimeCollabManager | CreateServer | Start");
            _server = new TcpListener(IPAddress.Parse("127.0.0.1"), 80);
            Debug.unityLogger.Log("RealTimeCollabManager | CreateServer | Create TcpListener");

            _server.Start();
            Debug.unityLogger.Log($"Server has started on 127.0.0.1:80.{Environment.NewLine}Waiting for a connection…");

            _client = await _server.AcceptTcpClientAsync();
            Debug.unityLogger.Log("RealTimeCollabManager | CreateServer | Create client");

            Debug.unityLogger.Log("A client connected.");

            await UniTask.Delay(TimeSpan.FromMilliseconds(100));

            _isCreatedServer = true;

            return true;
        }

        public async UniTask<bool> CreateServerNew()
        {
            if (_isCreatedServer) return true;
            Debug.unityLogger.Log("RealTimeCollabManager | CreateServerNew | Start");


            _wssv = new WebSocketServer("ws://127.0.0.1");
            Debug.unityLogger.Log("RealTimeCollabManager | CreateServerNew | Create WebSocketServer");
            _wssv.AddWebSocketService<Laputa>("/wss");
            _wssv.Start();

            Debug.unityLogger.Log("RealTimeCollabManager | CreateServerNew | Start WebSocketServer");
            await UniTask.Delay(TimeSpan.FromMilliseconds(100));

            _isCreatedServer = true;

            return true;
        }

        public void DestroyServer()
        {
            Debug.unityLogger.Log("RealTimeCollabManager | DestroyServer | Start");

            if (!_isCreatedServer) return;

            if (_client != null)
            {
                _client.Close();
                _client.Dispose();
                _client = null;
            }

            if (_server != null)
            {
                _server.Stop();
                _server = null;
            }

            _isCreatedServer = false;
            Debug.unityLogger.Log("RealTimeCollabManager | DestroyServer | End");
        }

        public void DestroyServerNew()
        {
            Debug.unityLogger.Log("RealTimeCollabManager | DestroyServer | Start");

            if (!_isCreatedServer) return;

            if (_wssv != null)
            {
                _wssv.Stop();
                _wssv = null;
            }

            _isCreatedServer = false;
            Debug.unityLogger.Log("RealTimeCollabManager | DestroyServer | End");
        }


        private void OnEnable()
        {
            Debug.unityLogger.Log("RealTimeCollabManager | OnEnable | Start");
            if (!_isCreatedServer) CreateServerNew().Forget();
            CompareFileHistory().Forget();
        }

        private void OnDisable()
        {
            Debug.unityLogger.Log("RealTimeCollabManager | OnDisable | Start");
            DestroyServerNew();
        }

        private void OnDestroy()
        {
            Debug.unityLogger.Log("RealTimeCollabManager | OnDestroy | Start");
            DestroyServerNew();
        }

        private async UniTask CompareFileHistory(
            string filePath = "Assets/ChistaGame/UnityRealTimeCollab/Prefabs/Cube.prefab")
        {
            Debug.unityLogger.Log("RealTimeCollabManager | CompareFileHistory | start");
            // string prettyFormat1 = ;
            // string formatString = "--pretty=format:'{%n  \"commit\": \"%h\",%n  \"author\": \"%an\",%n  \"date\": \"%ad\",%n  \"message\": \"%s\"%n}'";
            string formatString =
                "--pretty=format:\"{%n  \\\"commit\\\": \\\"%h\\\",%n  \\\"author\\\": \\\"%an\\\",%n  \\\"date\\\": \\\"%ad\\\",%n  \\\"message\\\": \\\"%s\\\"%n},\"";

            string gitCommand1 = $"log --all --full-history {formatString} --date=iso8601";
            string gitCommand = $"{gitCommand1} -- \"{filePath}\"";

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = gitCommand,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;

            process.Start();

            string errorOutput = await process.StandardError.ReadToEndAsync();
            Debug.Log($"Error Output: {errorOutput}");

            // Read the output asynchronously
            string output = await ReadOutputAsync(process);


            process.WaitForExit();

            ProcessOutputData(output);

            Debug.unityLogger.Log("RealTimeCollabManager | CompareFileHistory | end");
        }

        private async UniTask<string> ReadOutputAsync(Process process)
        {
            Debug.unityLogger.Log("RealTimeCollabManager | ReadOutputAsync | start");
            using (StreamReader reader = process.StandardOutput)
            {
                return await reader.ReadToEndAsync();
            }
        }


        private void ProcessOutputData(string data)
        {
            Debug.unityLogger.Log($"RealTimeCollabManager | ProcessOutputData | start data:{data}");
            if (!string.IsNullOrEmpty(data))
            {
                data = data.Substring(0, data.Length - 1);
                data = "[" + data + "]";

                List<GitLogResultModel> gitLogResult = JsonConvert.DeserializeObject<List<GitLogResultModel>>(data);
                
                JArray commitObject = JArray.Parse(data);
                Debug.Log($"Commit JSON:\n{commitObject.ToString(Formatting.Indented)}");
            }
        }
    }

    public class Laputa : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            var msg = e.Data == "BALUS"
                ? "I've been balused already..."
                : "I'm not available now.";

            Send(msg);
        }
    }
}