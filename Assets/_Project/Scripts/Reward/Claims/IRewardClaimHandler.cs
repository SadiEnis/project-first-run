using ProjectFirstRun.Items;

namespace ProjectFirstRun.Rewards.Claims
{
    public interface IRewardClaimHandler
    {
        bool Supports(
            ItemDefinition definition);

        RewardClaimResult TryClaim(
            ItemDefinition definition);
    }
}