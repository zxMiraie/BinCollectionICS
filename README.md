# Waste Collection Calendar

A containerized ASP.NET Core application that provides waste collection schedules as an ICS calendar feed.

## Endpoints

| Endpoint | Description |
|----------|-------------|
| `/calendar.ics` | ICS calendar feed |
| `/test` | Human-readable collection list |
| `/health` | Health check endpoint |


## Calendar Auto-Refresh

The calendar includes headers that tell calendar apps to refresh weekly:
- `REFRESH-INTERVAL;VALUE=DURATION:P1W`
- `X-PUBLISHED-TTL:PT1W`
- `Cache-Control: public, max-age=604800`

Most calendar apps (Apple Calendar, Google Calendar, Outlook) will automatically refresh the subscription.

## Configuration

Environment variables:

| Variable | Description | Default |
|----------|-------------|---------|
| `UPRN` | Your property's UPRN | Required |
| `WasteApi__BaseUrl` | Waste API base URL | `https://api.westnorthants.digital/openapi/v1/` |
| `ASPNETCORE_URLS` | App listening URL | `http://+:8080` |

## Development

```bash
cd WebApplication1
dotnet run
```

