using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AppBuilder
{
    public class BuildConfigureRecorder
    {
        private readonly StringBuilder _builder = new();

        private readonly Queue<Action> _configureActions = new();

        private readonly Queue<(string item, string message)> _configureMessages = new();

        public (string item, string message)[] GetMessages() => _configureMessages.ToArray();

        public void Enqueue(Action execute, string item, string message)
        {
            _configureActions.Enqueue(execute);
            Write(item, message);
        }

        public void Write(string item, string message)
        {
            _configureMessages.Enqueue((item, message));
        }

        public override string ToString()
        {
            _builder.Clear();
            foreach (var (item, message) in _configureMessages)
            {
                _builder.AppendLine($"{item,-50}{message,-50}");
            }
            return _builder.ToString();
        }
    }
}