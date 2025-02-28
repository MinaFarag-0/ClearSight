using Microsoft.AspNetCore.Identity;

namespace ClearSight.Core.Mosels
{
    public class User : IdentityUser
    {
        public string DisplayName { get; set; }
        public string ProfileImagePath { get; set; }
        public ICollection<UserCode> UserCodes { get; set; } = [];
        public List<RefreshToken>? RefreshTokens { get; set; } = [];
        public override string PhoneNumber { get => null; set { } }
        public override bool PhoneNumberConfirmed { get => false; set { } }
        public ICollection<UserPhoneNumber> PhoneNumbers { get; set; } = [];

    }
}
