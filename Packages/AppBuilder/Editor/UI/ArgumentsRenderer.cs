using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public class ArgumentsRenderer
    {
        private interface IRenderer
        {
            void Init(ArgumentsRenderer renderer, BuildInfo build);
            void Render(Dashboard dashboard, ArgumentValue arg);
        }


        private readonly Dictionary<ArgumentCategory, IRenderer> _renderers = new();

        private VisualElement _inputs;
        private VisualElement _args;


        public ArgumentsRenderer()
        {
            _renderers.Add(ArgumentCategory.Input, new InputRenderer());
            _renderers.Add(ArgumentCategory.Reserve, new ReserveRenderer());

            var defaultRender = new DefaultRenderer();
            _renderers.Add(ArgumentCategory.Custom, defaultRender);
            _renderers.Add(ArgumentCategory.None, defaultRender);
        }

        public void Render(Dashboard dashboard, BuildInfo build, BuildPlayer.Report report)
        {
            _inputs = dashboard.rootVisualElement.Q("input");
            _args = dashboard.rootVisualElement.Q("args");

            _inputs.Clear();
            _args.Clear();

            if (report == null) return;

            foreach (var renderer in _renderers)
            {
                renderer.Value.Init(this, build);
            }

            foreach (var pair in report.Args)
            {
                if (_renderers.TryGetValue(pair.Value.Category, out var renderer))
                {
                    renderer.Render(dashboard, pair.Value);
                }
                else
                {
                    _renderers[ArgumentCategory.None].Render(dashboard, pair.Value);
                }
            }
        }

        private class ReserveRenderer : IRenderer
        {
            private VisualElement _parent;

            private readonly HashSet<string> _reservedArgName = new()
            {
                "appsettings",
                "variant",
            };

            public ReserveRenderer()
            {
            }

            public void Init(ArgumentsRenderer renderer, BuildInfo build)
            {
                _parent = renderer._args;
            }

            public void Render(Dashboard dashboard, ArgumentValue arg)
            {
                var key = arg.Key;
                if (_reservedArgName.Contains(key))
                {
                    key = key.Insert(0, "* ");
                }

                _parent.Add(new Argument(key)
                {
                    IsValue = true,
                    Value = arg.Value
                });
            }
        }

        private class InputRenderer : IRenderer
        {
            private VisualElement _parent;
            private Dictionary<string, InputAttribute> _inputs;
            private BuildInfo _build;

            public InputRenderer()
            {
            }

            public void Init(ArgumentsRenderer renderer, BuildInfo build)
            {
                _parent = renderer._inputs;
                _build = build;
                _inputs = build.Inputs.ToDictionary(i => i.Name, i => i);
            }

            public void Render(Dashboard dashboard, ArgumentValue arg)
            {
                var key = arg.Key;
                if (!_inputs.TryGetValue(key, out var input)) return;

                var inputComponent = new Argument(key);
                switch (input.Options)
                {
                    case InputOptions.Directory:
                        inputComponent.IsInput = true;
                        inputComponent.IsFolder = true;
                        inputComponent.Value = BuildCache.GetString(_build, key, arg.Value);
                        inputComponent.RegisterInputChangedCallback(e =>
                        {
                            BuildCache.SetString(_build, key, e.newValue);
                            dashboard.ExecuteAndRender(BuildController.BuildMode.Preview);
                        });
                        break;
                    case InputOptions.File:
                        inputComponent.IsInput = true;
                        inputComponent.IsFile = true;
                        inputComponent.FileExtension = input.Extension;
                        inputComponent.Value = BuildCache.GetString(_build, key, arg.Value);
                        inputComponent.RegisterInputChangedCallback(e =>
                        {
                            BuildCache.SetString(_build, key, e.newValue);
                            dashboard.ExecuteAndRender(BuildController.BuildMode.Preview);
                        });
                        break;
                    case InputOptions.Dropdown:
                        inputComponent.IsDropdown = true;
                        inputComponent.Choices = new List<string>()
                        {
                            "None",
                            // "Auto"
                        }.Concat(input.Values).ToList();
                        var dropdownValue = arg.Value;
                        BuildCache.SetString(_build, key, dropdownValue);
                        if (string.IsNullOrEmpty(dropdownValue))
                        {
                            dropdownValue = "None";
                        }

                        inputComponent.Value = dropdownValue;
                        inputComponent.RegisterDropdownChangedCallback(e =>
                        {
                            BuildCache.SetString(_build, key, e.newValue);
                            dashboard.ExecuteAndRender(BuildController.BuildMode.Preview);
                        });
                        break;
                    default:
                        inputComponent.IsInput = true;
                        inputComponent.Value = BuildCache.GetString(_build, key, input.Value);
                        inputComponent.RegisterInputChangedCallback(e =>
                        {
                            BuildCache.SetString(_build, key, e.newValue);
                            dashboard.ExecuteAndRender(BuildController.BuildMode.Preview);
                        });
                        break;
                }

                _parent.Add(inputComponent);
            }
        }

        private class DefaultRenderer : IRenderer
        {
            private VisualElement _parent;

            public DefaultRenderer()
            {
            }

            public void Init(ArgumentsRenderer renderer, BuildInfo build)
            {
                _parent = renderer._args;
            }

            public void Render(Dashboard dashboard, ArgumentValue arg)
            {
                _parent.Add(new Argument(arg.Key)
                {
                    IsValue = true,
                    Value = arg.Value
                });
            }
        }
    }
}