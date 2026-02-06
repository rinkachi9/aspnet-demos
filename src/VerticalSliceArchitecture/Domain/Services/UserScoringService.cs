namespace VerticalSliceArchitecture.Domain.Services;

public class UserScoringService
{
    public int CalculateInitialScore(string email)
    {
        if (email.EndsWith("@company.com"))
        {
            return 100;
        }
        return 10;
    }
}
