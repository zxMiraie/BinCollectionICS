using System.Globalization;

using (var httpClient = new HttpClient())
{
    await GetAsync(httpClient);
}

static async Task GetAsync(HttpClient httpClient)
{
    var uprn = Environment.GetEnvironmentVariable("UPRN");
    
    var response = await httpClient.GetAsync($"https://api.westnorthants.digital/openapi/v1/unified-waste-collections/{uprn}");
    if (response.IsSuccessStatusCode)
    {
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(content);
    }
    else
    {
        Console.WriteLine($"Request failed with status code: {response.StatusCode}");
    }
}
