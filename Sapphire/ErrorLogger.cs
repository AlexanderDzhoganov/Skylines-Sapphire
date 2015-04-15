using System;
using UnityEngine;

namespace Sapphire
{
    public static class ErrorLogger
    {

        public static void LogError(string message)
        {
            if (GameObject.Find("ModTools") == null)
            {
                ExceptionDialog.Show(message);
            }

            Debug.LogError(message);
        }

        public static void LogErrorFormat(string message, params object[] args)
        {
        if (GameObject.Find("ModTools") == null)
            {
                ExceptionDialog.Show(String.Format(message, args));
            }

            Debug.LogErrorFormat(message, args);
        }

    }
}
