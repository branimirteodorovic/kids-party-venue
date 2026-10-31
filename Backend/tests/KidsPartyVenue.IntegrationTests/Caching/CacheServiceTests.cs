using AwesomeAssertions;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.IntegrationTests.Caching;

// The cache is shared by the whole test collection, so every test uses its own random keys and tags
public sealed class CacheServiceTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private readonly ICacheService _cache = factory.Services.GetRequiredService<ICacheService>();

    [Fact]
    public async Task GetOrCreateAsync_ShouldRunFactoryAgain_WhenTagIsRemoved()
    {
        // Arrange
        string key = NewKey();
        string tag = NewTag();
        CountingFactory counter = new();
        await _cache.GetOrCreateAsync(key, counter.Create, tags: [tag]);

        // Act
        await _cache.RemoveByTagAsync(tag);
        int value = await _cache.GetOrCreateAsync(key, counter.Create, tags: [tag]);

        // Assert
        value.Should().Be(2);
        counter.Calls.Should().Be(2);
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldRunFactoryAgain_WhenAnyOfItsTagsIsRemoved()
    {
        // Arrange
        string key = NewKey();
        string firstTag = NewTag();
        string secondTag = NewTag();
        CountingFactory counter = new();
        await _cache.GetOrCreateAsync(key, counter.Create, tags: [firstTag, secondTag]);

        // Act
        await _cache.RemoveByTagAsync(secondTag);
        int value = await _cache.GetOrCreateAsync(key, counter.Create, tags: [firstTag, secondTag]);

        // Assert
        value.Should().Be(2);
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldKeepCachedValue_WhenAnotherTagIsRemoved()
    {
        // Arrange
        string taggedKey = NewKey();
        string untaggedKey = NewKey();
        string tag = NewTag();
        CountingFactory taggedFactory = new();
        CountingFactory untaggedFactory = new();
        await _cache.GetOrCreateAsync(taggedKey, taggedFactory.Create, tags: [tag]);
        await _cache.GetOrCreateAsync(untaggedKey, untaggedFactory.Create);

        // Act
        await _cache.RemoveByTagAsync(NewTag());
        int taggedValue = await _cache.GetOrCreateAsync(taggedKey, taggedFactory.Create, tags: [tag]);
        int untaggedValue = await _cache.GetOrCreateAsync(untaggedKey, untaggedFactory.Create);

        // Assert
        taggedValue.Should().Be(1);
        untaggedValue.Should().Be(1);
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldCacheNewEntry_WhenTagWasRemovedBeforeItWasCreated()
    {
        // Arrange
        string key = NewKey();
        string tag = NewTag();
        CountingFactory counter = new();
        await _cache.RemoveByTagAsync(tag);

        // Act
        await _cache.GetOrCreateAsync(key, counter.Create, tags: [tag]);
        int value = await _cache.GetOrCreateAsync(key, counter.Create, tags: [tag]);

        // Assert
        value.Should().Be(1);
        counter.Calls.Should().Be(1);
    }

    [Fact]
    public async Task RemoveByTagAsync_ShouldNotThrow_WhenNoEntryHasTag()
    {
        // Act
        Func<Task> act = async () => await _cache.RemoveByTagAsync(NewTag());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveTaggedEntry_WhenKeyIsRemoved()
    {
        // Arrange
        string key = NewKey();
        CountingFactory counter = new();
        await _cache.GetOrCreateAsync(key, counter.Create, tags: [NewTag()]);

        // Act
        await _cache.RemoveAsync(key);
        int value = await _cache.GetOrCreateAsync(key, counter.Create);

        // Assert
        value.Should().Be(2);
    }

    [Fact]
    public async Task SetAsync_ShouldTagEntry_WhenTagsAreGiven()
    {
        // Arrange
        string key = NewKey();
        string tag = NewTag();
        CountingFactory counter = new();
        await _cache.SetAsync(key, 100, tags: [tag]);

        // Act
        int cachedValue = await _cache.GetOrCreateAsync(key, counter.Create);
        await _cache.RemoveByTagAsync(tag);
        int value = await _cache.GetOrCreateAsync(key, counter.Create);

        // Assert
        cachedValue.Should().Be(100);
        value.Should().Be(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("*")]
    public async Task GetOrCreateAsync_ShouldThrow_WhenTagIsInvalid(string? tag)
    {
        // Act
        Func<Task> act = async () => await _cache.GetOrCreateAsync(NewKey(), new CountingFactory().Create, tags: [tag!]);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("*")]
    public async Task SetAsync_ShouldThrow_WhenTagIsInvalid(string? tag)
    {
        // Act
        Func<Task> act = async () => await _cache.SetAsync(NewKey(), 1, tags: [tag!]);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("*")]
    public async Task RemoveByTagAsync_ShouldThrow_WhenTagIsInvalid(string? tag)
    {
        // Act
        Func<Task> act = async () => await _cache.RemoveByTagAsync(tag!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    private static string NewKey() => $"test:{Guid.NewGuid()}";

    private static string NewTag() => $"test-tag:{Guid.NewGuid()}";

    // Returns how many times it has run, so a test can tell a cached value from a new one
    private sealed class CountingFactory
    {
        public int Calls { get; private set; }

        public ValueTask<int> Create(CancellationToken cancellationToken) => ValueTask.FromResult(++Calls);
    }
}
