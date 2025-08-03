public class RefreshTokenStore
{
    private static Dictionary<string, string> _store = [];

    public void Save(string userId, string refreshToken)
    {
        _store[userId] = refreshToken;
    }

    public bool Validate(string userId, string refreshToken)
    {
        return _store.ContainsKey(userId) && _store[userId] == refreshToken;
    }

    public void Rotate(string userId, string newRefreshToken)
    {
        _store[userId] = newRefreshToken;
    }
}
