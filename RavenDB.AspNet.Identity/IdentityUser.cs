using System.Collections.Generic;
using Microsoft.AspNet.Identity;

namespace AspNet.Identity.RavenDB
{
    public class IdentityUser : IUser
    {
	    public virtual string Id { get; set; }
		public virtual string UserName { get; set; }
		public virtual string PasswordHash { get; set; }
		public virtual string SecurityStamp { get; set; }
		public virtual List<string> Roles { get; private set; }
		public virtual List<IdentityUserClaim> Claims { get; private set; }
		public virtual List<UserLoginInfo> Logins { get; private set; }

		public IdentityUser()
		{
			Claims = new List<IdentityUserClaim>();
			Roles = new List<string>();
			Logins = new List<UserLoginInfo>();
		}

		public IdentityUser(string userName)
			: this()
		{
			UserName = userName;
		}
	}
}
