using System.Net;
using System.IO;
using System.Text;
using System;

public class Http
{
    public string Url { get; private set; }
    public string AppId { get; private set; }
    public string ApiKey { get; private set; }

    public Http(string url, string appId, string apiKey)
    {
        Url = url;
        AppId = appId;
        ApiKey = apiKey;
    }

    public string Post(string command, string jsonPayload = "")
    {
        var request = GenerateRequest(command, jsonPayload, "POST");
        return DoAjax(request);
    }

    public void PostAsync(AsyncCallback callback, string command, string jsonPayload = "")
    {
        var request = GenerateRequest(command, jsonPayload, "POST");
        DoAjaxAsync(callback, request);
    }

    public string Delete(string command, string jsonPayload = "")
    {
        var request = GenerateRequest(command, jsonPayload, "DELETE");
        return DoAjax(request);
    }

    public void DeleteAsync(AsyncCallback callback, string command, string jsonPayload = "")
    {
        var request = GenerateRequest(command, jsonPayload, "DELETE");
        DoAjaxAsync(callback, request);
    }


    public string Put(string command, string jsonPayload = "")
    {
        var request = GenerateRequest(command, jsonPayload, "PUT");
        return DoAjax(request);
    }

    public void PutAsync(AsyncCallback callback, string command, string jsonPayload = "")
    {
        var request = GenerateRequest(command, jsonPayload, "PUT");
        DoAjaxAsync(callback, request);
    }


    public string Get(string command, string jsonPayload = "")
    {
        var request = GenerateRequest(command);
        return DoAjax(request);
    }

    public void GetAsync(AsyncCallback callback, string command, string jsonPayload = "")
    {
        var request = GenerateRequest(command);
        DoAjaxAsync(callback, request);
    }


    private void DoAjaxAsync(AsyncCallback callback, HttpWebRequest request)
    {
        request.BeginGetResponse(callback, request);
    }

    private string DoAjax(HttpWebRequest request)
    {
        var response = (HttpWebResponse)request.GetResponse();
        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        {
            return reader.ReadToEnd();
        }
    }

    private HttpWebRequest GenerateRequest(string command, string jsonPayload = "", string method = "GET")
    {
        if (command?.Length == 0) return null;

        command = command.StartsWith("/") ? command : "/" + command;
        command = command.EndsWith("/") ? command : command + "/";
        var completeUrl = Url + command;
        var request = (HttpWebRequest)WebRequest.Create(completeUrl);
        request.Method = method;
        request.ContentType = "application/json";
        request.Headers.Add("X-Parse-Application-Id", AppId);
        request.Headers.Add("X-Parse-REST-API-Key", ApiKey);

        if(jsonPayload?.Length == 0) return request;

        ASCIIEncoding encoding = new ASCIIEncoding();
        byte[] byteArr = encoding.GetBytes(jsonPayload);
        request.ContentLength = byteArr.Length;
        Stream newStream = request.GetRequestStream();
        newStream.Write(byteArr, 0, byteArr.Length);
        newStream.Close();

        return request;
    }


}
