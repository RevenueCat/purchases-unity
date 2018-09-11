using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PurchasesWrapper
{
	void Setup(string gameObject, string apiKey, string appUserID);
	void AddAttributionData(string network, string data);
	void GetProducts(string[] productIdentifiers, string type = "subs");
	void MakePurchase(string productIdentifier, string type = "subs", string oldSku = null);
	void RestoreTransactions();
}