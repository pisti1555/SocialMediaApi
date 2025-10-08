using Infrastructure.Hasher;
using Microsoft.Extensions.Configuration;

namespace UnitTests.Infrastructure.Hasher;

public class AppHasherTest
{
    private readonly AppHasher _hasher;
    
    private const string Input = "Test input";
    
    public AppHasherTest()
    {
        var testConfig = new Dictionary<string, string?>
        {
            { "HashKey", "super-secret-key-for-test-hashing" }
        };
            
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(testConfig)
            .Build();

        _hasher = new AppHasher(config);
    }
    
    [Fact]
    public void CreateHash_WhenCalled_ShouldBeBase64EncodedHash()
    {
        // Act
        var hash = _hasher.CreateHash(Input);
            
        // Assert
        Assert.False(string.IsNullOrWhiteSpace(hash));
        var bytes = Convert.FromBase64String(hash);
        Assert.Equal(32, bytes.Length);
    }
    
    [Fact]
    public void CreateHash_WhenSameInput_ShouldBeTheSameHash()
    {
        // Arrange
        const string otherButSameInput = "Test input";
            
        // Act
        var hash1 = _hasher.CreateHash(Input);
        var hash2 = _hasher.CreateHash(otherButSameInput);
            
        // Assert
        Assert.Equal(hash1, hash2);
    }
    
    [Fact]
    public void CreateHash_WhenDifferentInputs_ShouldBeDifferentHashes()
    {
        // Arrange
        const string differentInput = "Different input";
            
        // Act
        var hash1 = _hasher.CreateHash(Input);
        var hash2 = _hasher.CreateHash(differentInput);
            
        // Assert
        Assert.NotEqual(hash1, hash2);
    }
    
    [Fact]
    public void CreateHash_WhenHashKeyMissing_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var hasher = new AppHasher(config);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => hasher.CreateHash(Input));
    }
}