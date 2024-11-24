namespace Melodee.Common.Models.OpenSubsonic;

public record Directory(string Id, string Parent, string Name, bool Starred, int UserRating, int AverageRating, long PlayCount, Child[] Child);
