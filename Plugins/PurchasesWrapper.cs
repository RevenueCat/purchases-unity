using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PurchasesWrapper
{
	void Setup(string gameObject, string apiKey, string appUserID);
	void GetProducts(string[] productIdentifiers, string type = "subs");
	void MakePurchase(string productIdentifier, string type = "subs");
}