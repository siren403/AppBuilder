using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AppBuilder.ConsoleApp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using PackageInfo = AppBuilder.UI.PackageInfo;

namespace Editor.Component
{
    public class BWindow : UIToolkitWindow
    {
        [MenuItem("AppBuilder/BWindow")]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow<BWindow>();
            window.Show();
        }

        protected override void Render()
        {
            Load(
                PackageInfo.GetPath("Editor/Component/BWindow.uxml"),
                PackageInfo.GetPath("Editor/Component/BWindow.uss")
            );
            Add(new SharedCounter());
        }
    }

    #region BWindow

    public class CounterDisposeButton : Component
    {
        private readonly Button _button;

        public CounterDisposeButton()
        {
            Add(new Button() {text = "dispose"}, out _button);
        }

        protected override void Init()
        {
        }

        public new class UxmlFactory : UxmlFactory<CounterDisposeButton>
        {
        }
    }

    public class SharedCounter : Component
    {
        private static int _count;

        private readonly Label _labelCount;
        private readonly Button _button;

        public SharedCounter()
        {
            style.flexDirection = FlexDirection.Row;

            Add(new Label("0"), out _labelCount);
            Add(new Button()
            {
                text = "Add"
            }, out _button);
            Add(new Button(RemoveFromHierarchy) {name = "X"});
        }

        protected override void Init()
        {
            _button.clicked += () =>
            {
                using (Window.SendEvent<ChangedEvent>(out var e))
                {
                    e.Count = ++_count;
                }
            };

            Window.RegisterCallback<ChangedEvent>(e => { _labelCount.text = e.Count.ToString(); });
        }

        public class ChangedEvent : EventBase<ChangedEvent>
        {
            public int Count;
        }

        public new class UxmlFactory : UxmlFactory<SharedCounter>
        {
        }
    }

    public class DisplayProgressBarScope : IDisposable
    {
        private readonly string _title;
        private readonly string _info;

        public float Progress
        {
            set => EditorUtility.DisplayProgressBar(_title, _info, value);
        }

        public DisplayProgressBarScope(string title, string info, float progress)
        {
            _title = title;
            _info = info;
            Progress = progress;
        }

        public void Dispose()
        {
            EditorUtility.ClearProgressBar();
        }
    }

    public class FtpUpload : Component
    {
        private TextField _tool;
        private TextField _host;
        private TextField _user;
        private TextField _passwd;
        private TextField _local;
        private TextField _remote;

        private Button _upload;

        public FtpUpload()
        {
            Add(new TextField("tool") {viewDataKey = "tool"}, out _tool);
            Add(new TextField("host") {viewDataKey = "host"}, out _host);
            Add(new TextField("user") {viewDataKey = "user"}, out _user);
            Add(new TextField("passwd") {viewDataKey = "passwd"}, out _passwd);
            Add(new TextField("local") {viewDataKey = "local"}, out _local);
            Add(new TextField("remote") {viewDataKey = "remote"}, out _remote);

            Add(new Button(Upload) {text = "upload"}, out _upload);
        }

        protected override void Init()
        {
        }

        private void Upload()
        {
            var command = "ftp upload";
            using var scope = new DisplayProgressBarScope("execute", command, 0);
            
            var args = new Dictionary<string, string>
            {
                ["host"] = _host.value,
                ["user"] = _user.value,
                ["passwd"] = _passwd.value,
                ["local"] = _local.value,
                ["remote"] = _remote.value
            };

            using var process = new ProcessExecutor(_tool.value);
            process.Execute(command, args);
            
            foreach (var line in process.ReadLines())
            {
                Debug.Log(line);
            }
        }

        public new class UxmlFactory : UxmlFactory<FtpUpload>
        {
        }
    }

    #endregion
}