namespace Common.Isekai.Client;

using System;
using System.Net.Http;
using System.Reflection;
using Common.Isekai.Services;

public static class IsekaiClient
{
    public static T Create<T>(HttpClient http) where T : class, IIsekaiService
    {
        var proxy = DispatchProxy.Create<T, IsekaiClientProxy>() as IsekaiClientProxy
                    ?? throw new InvalidOperationException("Failed to create Isekai proxy");

        proxy.SetHttpClient(http);
        return proxy as T ?? throw new InvalidOperationException("Failed to cast proxy");
    }
}
