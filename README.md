### Todo List
-[] Add SMTP integration for job notifications
-[] Add HTTP client for relaying to backend



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
    "jobStatus":"Job in progress..."
}
```

The `jobId` is always a GUID, and `jobStatus` should always say "Job in progress..." unless the operation failed immediately.

This `jobId` can be reused by passing it as a subdirectory to the `/jobStatus` endpoint. This will return a simple text string
with a description of the current state of the job, or an error message if no status string is found.

The job GUIDs provide a means for users to independently check on the status of their running jobs.
Though, the ideal solution would be to integrate with an automatic mailer to send notifications to users of either
success or failure.

Included for convenience is the `/httprepl` directory, which contains a collection of example JSON payloads to `POST` with the `httprepl` tool, e.g.:
```
> connect http://localhost:7071
> post /api/submitJob -f httprepl/long.json
```