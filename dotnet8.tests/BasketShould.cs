using Microsoft.Extensions.Time.Testing;

namespace dotnet8.tests;

public class Basket(TimeProvider timeProvider, LoyaltyLevel loyaltyLevel)
{
    private readonly DateTimeOffset _createdAt = timeProvider.GetUtcNow();
    private readonly DateTimeOffset _expireAt = timeProvider.GetUtcNow()
        .AddDays(loyaltyLevel == LoyaltyLevel.Standard ? 1 : 7);

    public bool IsExpired => timeProvider.GetUtcNow() > _expireAt;
}

public enum LoyaltyLevel
{
    Standard,
    Gold,
}

public class BasketShould
{
    [Fact]
    public void GoldendUserShouldBeExpiredAfter7Days()
    {
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);

        var basket = new Basket(fakeTime, LoyaltyLevel.Gold);
        fakeTime.Advance(TimeSpan.FromDays(8));

        Assert.True(basket.IsExpired);
    }
}