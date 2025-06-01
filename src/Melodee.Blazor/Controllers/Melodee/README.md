# Melodee API
This API is intended for Melodee applications and is designed to have these advantages over the terrible OpenSubsonic API:
* Performance as a priority
* All list operations are paginated
* Name of methods and return objects are semantic
* Return objects are as light as possible
* Paginated requests include a "meta" property that outlines pagination data in a uniform best practices manner

## Authentication
All requests (save Song Stream) are expected to have Bearer Tokens provided in the `Authorization` header.
This bearer token can be obtained by calling the `/user/authenticate` endpoint.

## Song Controller
As many JavaScript and react audio controls don't handle bearer tokens well the song stream method uses a simple HMAC authentication mechanism.
