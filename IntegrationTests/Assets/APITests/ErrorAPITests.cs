using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class ErrorAPITests : MonoBehaviour
    {
        private void Start()
        {
            // TODO: another place where properties are lowercase
            // and readonly
            Purchases.Error error = new Purchases.Error(null);
            string message = error.message;
            int code = error.code;
            string underlyingErrorMessage = error.underlyingErrorMessage;
            string readableErrorCode = error.readableErrorCode;
        }
    }
}