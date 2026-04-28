using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using System.Net;

public class DBTester : MonoBehaviour
{
    [SerializeField]
    private string _Url = "https://parseapi.back4app.com";

    [SerializeField]
    private string _AppID = "I35rhH6ylsr3IsUSVNin3k7RUILz73Gw6LKPXZen";

    [SerializeField]
    private string _ApiKey = "bp4rO0HIAqOxrNtRX9zWteQsnfYDNPPRAhCToIeN";


    [System.Serializable]
    private struct TopScoreSimple
    {
        public string UserName;
        public int Score;
    }

    private class TopScoreList
    {
        public TopScoreSimple[] results;
    }

    private void Start()
    {
        Http http = new Http(_Url, _AppID, _ApiKey);

        // private struct
        var record = new TopScoreSimple()
        {
            UserName = "Scott Henshaw",
            Score = 88
        };
        var payload = JsonUtility.ToJson(record);
        var pst = http.Put("classes/TopScoresTest/m6pZW0WuMM", payload);

        print(http.Get("classes/TopScoresTest"));
    }

    private void OnGetCallback(IAsyncResult result)
    {
        print("OnGetCallback");
        var tmp = result.AsyncState as TopScoreList;
        foreach (var item in tmp.results)
        {
            print($" UserName: {item.UserName} - Score: {item.Score}");
        }
    }


}
