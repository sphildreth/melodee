namespace Melodee.Common.Models.OpenSubsonic;

public record User(string Username, bool AdminRole, string Email, bool StreamRole, bool ScrobblingEnabled, bool DownloadRow, bool ShareRole, bool JukeboxRole);
