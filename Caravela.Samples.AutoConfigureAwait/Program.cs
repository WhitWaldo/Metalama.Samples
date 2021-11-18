﻿using System;
using System.Threading.Tasks;

[assembly: AutoConfigureAwait]

internal class Program
{
    private static async Task Main()
    {
        var client = new FakeHttpClient();
        Console.WriteLine(await MakeRequest(client));
    }

    private static async Task<string> MakeRequest(FakeHttpClient client) =>
        await client.GetAsync("https://example.org");
}

internal class FakeHttpClient
{
    public async ValueTask<string> GetAsync(string url)
    {
        Console.WriteLine($"Pretenting to fetch {url}.");
        await Task.Yield();
        return "<html>";
    }
}