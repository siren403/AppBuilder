using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AppBuilder.ConsoleApp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
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

    #region Border

    class BorderFactory : UxmlFactory<Border>
    { }

    class Border : ImmediateModeElement, IDisposable
    {
        Material m_Mat;

        static Mesh s_Mesh;

        public Border()
        {
            RecreateResources();
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        void RecreateResources()
        {
            if (s_Mesh == null)
            {
                s_Mesh = new Mesh();
                int verticeCount = 16;

                var vertices = new Vector3[verticeCount];
                var uvsBorder = new Vector2[verticeCount];

                for (int ix = 0; ix < 4; ++ix)
                {
                    for (int iy = 0; iy < 4; ++iy)
                    {
                        vertices[ix + iy * 4] = new Vector3(ix < 2 ? -1 : 1, iy < 2 ? -1 : 1, 0);
                        uvsBorder[ix + iy * 4] = new Vector2(ix == 0 || ix == 3 ? 1 : 0, iy == 0 || iy == 3 ? 1 : 0);
                    }
                }

                var indices = new int[4 * 8];

                for (int ix = 0; ix < 3; ++ix)
                {
                    for (int iy = 0; iy < 3; ++iy)
                    {
                        int quadIndex = (ix + iy * 3);
                        if (quadIndex == 4)
                            continue;
                        else if (quadIndex > 4)
                            --quadIndex;

                        int vertIndex = quadIndex * 4;
                        indices[vertIndex] = ix + iy * 4;
                        indices[vertIndex + 1] = ix + (iy + 1) * 4;
                        indices[vertIndex + 2] = ix + 1 + (iy + 1) * 4;
                        indices[vertIndex + 3] = ix + 1 + iy * 4;
                    }
                }

                s_Mesh.vertices = vertices;
                s_Mesh.uv = uvsBorder;
                s_Mesh.SetIndices(indices, MeshTopology.Quads, 0);
            }
            if (m_Mat == null)
                m_Mat = new Material(Shader.Find("Hidden/VFX/GradientBorder"));
        }

        void IDisposable.Dispose()
        {
            Object.DestroyImmediate(m_Mat);
        }

        Color m_StartColor;
        public Color startColor
        {
            get { return m_StartColor; }
        }

        Color m_EndColor;
        public Color endColor
        {
            get { return m_EndColor; }
        }

        static readonly CustomStyleProperty<Color> s_StartColorProperty = new CustomStyleProperty<Color>("--start-color");
        static readonly CustomStyleProperty<Color> s_EndColorProperty = new CustomStyleProperty<Color>("--end-color");
        private void OnCustomStyleResolved(CustomStyleResolvedEvent e)
        {
            var customStyle = e.customStyle;
            customStyle.TryGetValue(s_StartColorProperty, out m_StartColor);
            customStyle.TryGetValue(s_EndColorProperty, out m_EndColor);
        }

        protected override void ImmediateRepaint()
        {
            RecreateResources();
            var view = GetFirstAncestorOfType<BWindow>();
            var scale = 1;
            // if (view != null && m_Mat != null)
            // {
                float radius = resolvedStyle.borderTopLeftRadius;

                float realBorder = style.borderLeftWidth.value * scale;//view.scale;

                Vector4 size = new Vector4(layout.width * .5f, layout.height * 0.5f, 0, 0);
                m_Mat.SetVector("_Size", size);
                m_Mat.SetFloat("_Border", realBorder < 1.75f ? 1.75f / scale/*view.scale*/ : style.borderLeftWidth.value);
                m_Mat.SetFloat("_Radius", radius);

                m_Mat.SetColor("_ColorStart", (QualitySettings.activeColorSpace == ColorSpace.Linear) ? startColor.gamma : startColor);
                m_Mat.SetColor("_ColorEnd", (QualitySettings.activeColorSpace == ColorSpace.Linear) ? endColor.gamma : endColor);

                m_Mat.SetPass(0);

                Graphics.DrawMeshNow(s_Mesh, Matrix4x4.Translate(new Vector3(size.x, size.y, 0)));
            // }
        }
    }

    #endregion
}