# Async Function App Toy Model

This program is an Azure Function App designed to demonstrate a possible structural solution to provide an user-facing application
with a non-blocking API to handle very slow and very large batch jobs.

The API, in this case, is simple, consisting of only two endpoints:

- `POST /api/submitJob`
- `GET /api/jobStatus/{guid}`

The `/submitJob` endpoint expects a JSON document of the following structure:
```json
{
    "name":/* string */,
    "data":/* arbitrary JSON */
}
```
The `name` can be either `"short"`, `"medium"`, or `"long"`. Other values deliberately produce a failure state.

The arbitrary JSON can be any payload. The app logger will print out this section.

This endpoint will repond with a JSON document of the following structure:
```json
{
    "jobId":"250e5882-aa98-4c69-af78-366aaa5fa273",
    "jobStatus": "{\"message\":\"Job in progress\"}..."
}
```

The `jobId` is always a GUID, and `jobStatus` is always another serialized JSON payload.

This `jobId` can be reused by passing it as a subdirectory to the `/jobStatus` endpoint, which will return a JSON document.
```json
{
    "message":"Job in progress...",
    "running":true,
    "success":false
}
```
> `success` should only be `true` if `running` is `false`.

> Note that this object is the same as the serialized JSON object included in the `/submitJob` endpoint response.

The job GUIDs provide a means for users to independently check on the status of their running jobs.
Though, the ideal solution would be to integrate with an automatic mailer to send notifications to users of either
success or failure.

## Test App

> accessible at <http://localhost:7071/static/index.html> in development environment.

The app also provides a collection of static files under `/wwwroot`. The `index.html` is a small JavaScript app to quickly send test requests to the API.

Clicking on one of the pre-written payloads, or typing in one manually, will `POST` that payload to `/submitJob`, and a create a new entry in a list with the job's GUID, a status message, and a delete button.

The app polls the `/jobStatus` endpoint every second to update the status message and determine if the job is finished, after which it will grey out and stop polling the server. If the job fails, the box will turn red instead.

The delete button stops the app polling for that job GUID if it hasn't already, and removes the entry from the list.