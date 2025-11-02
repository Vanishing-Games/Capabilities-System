using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VanishingGames.ECC.Runtime
{
    public static class EccLogger
    {
        private const string SystemTag = "<color=#00FFFF><b>[ECC System]</b></color>";

        public static void LogInfo(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var sb = new StringBuilder();
            sb.Append(SystemTag);
            sb.Append(" <color=#00FF00>[INFO]</color> ");
            sb.Append($"<color=#808080>[{timestamp}]</color>");
            sb.Append("  →");
            sb.AppendLine();
            sb.AppendLine(message);
            Debug.Log(sb.ToString());
        }

        public static void LogWarning(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var sb = new StringBuilder();
            sb.Append(SystemTag);
            sb.Append(" <color=#FFFF00>[WARNING]</color> ");
            sb.Append($"<color=#808080>[{timestamp}]</color>");
            sb.Append("  →");
            sb.AppendLine();
            sb.AppendLine(message);
            Debug.LogWarning(sb.ToString());
        }

        public static void LogError(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var sb = new StringBuilder();
            sb.Append(SystemTag);
            sb.Append(" <color=#FF0000>[ERROR]</color> ");
            sb.Append($"<color=#808080>[{timestamp}]</color>");
            sb.Append("  →");
            sb.AppendLine();
            sb.AppendLine(message);
            Debug.LogError(sb.ToString());
        }
    }
}
