using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Members.LeaveCampaign;

public sealed record LeaveCampaignCommand(long CampaignId) : ICommand;
