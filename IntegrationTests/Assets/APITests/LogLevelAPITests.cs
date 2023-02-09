using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class LogLevelAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.LogLevel level = Purchases.LogLevel.Debug;

            switch (level)
            {
                case Purchases.LogLevel.Verbose:
                case Purchases.LogLevel.Debug:
                case Purchases.LogLevel.Info:
                case Purchases.LogLevel.Warn:
                case Purchases.LogLevel.Error:
                    break;
            }
        }
    }
}