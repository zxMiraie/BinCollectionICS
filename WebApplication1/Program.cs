var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

string UPRN = Environment.GetEnvironmentVariable("UPRN");

string ApiEndpoint = $"https://api.westnorthants.digital/openapi/v1/unified-waste-collections/{UPRN}";


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

