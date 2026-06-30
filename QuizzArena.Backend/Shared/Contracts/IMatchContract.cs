using Shared.Contracts.DTOs;

namespace Shared.Contracts;

public interface IMatchContract
{
    public Task<Guid> CreateAutomaticMatch(MatchCreationAutomaticRequestDTO matchAutomaticCreationDto);
}
