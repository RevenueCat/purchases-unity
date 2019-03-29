using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PurchasesWrapper
{
	void Setup(string gameObject, string apiKey, string appUserID);
	void AddAttributionData(int network, string data);
	void GetProducts(string[] productIdentifiers, string type = "subs");
	void MakePurchase(string productIdentifier, string type = "subs", string oldSku = null);
	void RestoreTransactions();
	void CreateAlias(string newAppUserID);
	void Identify(string appUserID);
	void Reset();
	void SetFinishTransactions(bool finishTransactions);
}