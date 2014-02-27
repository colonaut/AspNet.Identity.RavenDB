﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Raven.Client;

namespace AspNet.Identity.RavenDB
{
	public class UserStore<TUser> : IUserStore<TUser>,
        IUserLoginStore<TUser>,
        IUserClaimStore<TUser>,
        IUserRoleStore<TUser>,
		IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>
		where TUser : IdentityUser
	{
		private bool _disposed;
		private readonly Func<IDocumentSession> _getSessionFunc;
		private IDocumentSession _session;

		private IDocumentSession Session
		{
			get
			{
				if (_session == null)
					_session = _getSessionFunc();
				return _session;
			}
		}

		public UserStore(Func<IDocumentSession> getSession)
		{
			_getSessionFunc = getSession;
		}

		public UserStore(IDocumentSession session)
		{
			_session = session;
		}

		public Task CreateAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			user.Id = UsernameToDocumentId(user.UserName);
			Session.Store(user);
			return Task.FromResult(true);
		}

		private string UsernameToDocumentId(string userName)
		{
			var conventions = Session.Advanced.DocumentStore.Conventions;
			string typeTagName = conventions.GetTypeTagName(typeof (TUser));
			string tag = conventions.TransformTypeTagNameToDocumentKeyPrefix(typeTagName);
			return String.Format("{0}{1}{2}", tag, conventions.IdentityPartsSeparator, userName);
		}

		public Task DeleteAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			Session.Delete(user);
			return Task.FromResult(true);
		}

		public Task<TUser> FindByIdAsync(string userId)
		{
			var user = Session.Load<TUser>(userId);
			return Task.FromResult(user);
		}

		public Task<TUser> FindByNameAsync(string userName)
		{
			string userDocId = UsernameToDocumentId(userName);
			return FindByIdAsync(userDocId);
		}

		public Task UpdateAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			return Task.FromResult(true);
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(GetType().Name);
		}

		public void Dispose()
		{
			_disposed = true;
		}

		public Task AddLoginAsync(TUser user, UserLoginInfo login)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			if (!user.Logins.Any(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey))
			{
				user.Logins.Add(login);

				Session.Store(new IdentityUserLogin
				{
					Id = Util.GetLoginId(login),
					UserId = user.Id,
					Provider = login.LoginProvider,
					ProviderKey = login.ProviderKey
				});
			}

			return Task.FromResult(true);
		}

		public Task<TUser> FindAsync(UserLoginInfo login)
		{
			string loginId = Util.GetLoginId(login);

			var loginDoc = Session.Include<IdentityUserLogin>(x => x.UserId)
				.Load(loginId);

			TUser user = null;

			if (loginDoc != null)
				user = Session.Load<TUser>(loginDoc.UserId);

			return Task.FromResult(user);
		}

		public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			return Task.FromResult(user.Logins.ToIList());
		}

		public Task RemoveLoginAsync(TUser user, UserLoginInfo login)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			string loginId = Util.GetLoginId(login);
			var loginDoc = Session.Load<IdentityUserLogin>(loginId);
			if (loginDoc != null)
				Session.Delete(loginDoc);

			user.Logins.RemoveAll(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);
			
			return Task.FromResult(0);
		}

		public Task AddClaimAsync(TUser user, Claim claim)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			if (!user.Claims.Any(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value))
			{
				user.Claims.Add(new IdentityUserClaim
				{
					ClaimType = claim.Type,
					ClaimValue = claim.Value
				});
			}
			return Task.FromResult(0);
		}

		public Task<IList<Claim>> GetClaimsAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			IList<Claim> result = user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
			return Task.FromResult(result);
		}

		public Task RemoveClaimAsync(TUser user, Claim claim)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			user.Claims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
			return Task.FromResult(0);
		}

		public Task<string> GetPasswordHashAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			return Task.FromResult(user.PasswordHash);
		}

		public Task<bool> HasPasswordAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			return Task.FromResult<bool>(user.PasswordHash != null);
		}

		public Task SetPasswordHashAsync(TUser user, string passwordHash)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			user.PasswordHash = passwordHash;
			return Task.FromResult(0);
		}

		public Task<string> GetSecurityStampAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");
			
			return Task.FromResult(user.SecurityStamp);
		}

		public Task SetSecurityStampAsync(TUser user, string stamp)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			user.SecurityStamp = stamp;
			return Task.FromResult(0);
		}

		public Task AddToRoleAsync(TUser user, string role)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			if (!user.Roles.Contains(role, StringComparer.InvariantCultureIgnoreCase))
				user.Roles.Add(role);

			return Task.FromResult(0);
		}

		public Task<IList<string>> GetRolesAsync(TUser user)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			return Task.FromResult<IList<string>>(user.Roles);
		}

		public Task<bool> IsInRoleAsync(TUser user, string role)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			return Task.FromResult(user.Roles.Contains(role, StringComparer.InvariantCultureIgnoreCase));
		}

		public Task RemoveFromRoleAsync(TUser user, string role)
		{
			ThrowIfDisposed();
			if (user == null)
				throw new ArgumentNullException("user");

			user.Roles.RemoveAll(r => String.Equals(r, role, StringComparison.InvariantCultureIgnoreCase));

			return Task.FromResult(0);
		}
	}
}
