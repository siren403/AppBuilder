using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AppBuilder;
using AppBuilder.ConsoleApp;
using UnityEditor;
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
            this.Add(new Button() {text = "dispose"}, out _button);
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

            this.Add(new Label("0"), out _labelCount);
            this.Add(new Button()
            {
                text = "Add"
            }, out _button);
            Add(new Button(RemoveFromHierarchy) {name = "X"});
        }

        // protected override void Init()
        // {
        //     _button.clicked += () =>
        //     {
        //         using (Window.SendEvent<ChangedEvent>(out var e))
        //         {
        //             e.Count = ++_count;
        //         }
        //     };
        //
        //     Window.RegisterCallback<ChangedEvent>(e => { _labelCount.text = e.Count.ToString(); });
        // }

        public class ChangedEvent : EventBase<ChangedEvent>
        {
            public int Count;
        }

        public new class UxmlFactory : UxmlFactory<SharedCounter>
        {
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
            this.Add(new TextField("tool") {viewDataKey = "tool"}, out _tool);
            this.Add(new TextField("host") {viewDataKey = "host"}, out _host);
            this.Add(new TextField("user") {viewDataKey = "user"}, out _user);
            this.Add(new TextField("passwd") {viewDataKey = "passwd"}, out _passwd);
            this.Add(new TextField("local") {viewDataKey = "local"}, out _local);
            this.Add(new TextField("remote") {viewDataKey = "remote"}, out _remote);

            this.Add(new Button(Upload) {text = "upload"}, out _upload);
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