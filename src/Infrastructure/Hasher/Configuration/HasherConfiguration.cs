using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Hasher.Configuration;

public sealed class HasherConfiguration
{
    [Required, MinLength(32)]
    public required string Hashkey { get; set; }
}