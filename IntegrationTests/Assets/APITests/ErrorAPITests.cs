using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class ErrorAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.Error error = new Purchases.Error(null);
            string message = error.Message;
            int code = error.Code;
            string underlyingErrorMessage = error.UnderlyingErrorMessage;
            string readableErrorCode = error.ReadableErrorCode;
        }
    }
}