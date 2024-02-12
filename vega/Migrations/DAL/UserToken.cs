public partial class UserToken
{
    public int UserId { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime ExpireTime {get; set; }
    
}