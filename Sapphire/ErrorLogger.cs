using System;
using UnityEngine;

namespace Sapphire
{
    public static class ErrorLogger
    {
        private static bool foundModTools = false;
        private static bool lookedForModTools = false;

        public static void ResetSettings()
        {
            lookedForModTools = false;
            foundModTools = false;
        }

        private static bool FoundModTools
        {
            get
            {
                if (!lookedForModTools)
                {
                    foundModTools = GameObject.Find("ModTools") != null;
                    lookedForModTools = true;
                }

                return foundModTools;
            }
        }

        public static void LogError(string message)
        {
            if (!FoundModTools)
            {
                ExceptionDialog.Show(message);
            }

            Debug.LogError(message);
        }

        public static void LogErrorFormat(string message, params object[] args)
        {
            if (!FoundModTools)
            {
                ExceptionDialog.Show(String.Format(message, args));
            }

            Debug.LogErrorFormat(message, args);
        }

    }
}
