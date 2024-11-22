using Melodee.Common.Models.Scrobbling;
using Microsoft.Extensions.Primitives;

namespace Melodee.Common.Models.OpenSubsonic.Requests;

/// <summary>
///     Setup from Query/Post parameters for the Subsonic request.
/// </summary>S
/// <param name="RequestHeaders">All request headers for request.</param>
/// <param name="Username">(u) The username.</param>
/// <param name="Version">(v) The protocol version implemented by the client, i.e., the version of the subsonic-rest-api.xsd schema used</param>
/// <param name="Format">(f) Request data to be returned in this format.</param>
/// <param name="ApiKey">An API key used for authentication</param>
/// <param name="Password">(p) The password, either in clear text or hex-encoded with a “enc:” prefix.</param>
/// <param name="Token">(t) The authentication token computed as md5(password + salt).</param>
/// <param name="Salt">(s) A random string (“salt”) used as input for computing the password hash</param>
/// <param name="ApiRequestPlayer">Details on the request subsonic client application (aka player.)</param>
public record ApiRequest(IDictionary<string, StringValues>? RequestHeaders, string? Username, string? Version, string? Format, string? ApiKey, string? Password, string? Token, string? Salt, UserPlayer ApiRequestPlayer);
